using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;
using System.Security.Claims;

namespace Safexchange.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly SafexchangeDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public ChatHub(SafexchangeDbContext db, ICurrentUserService currentUser, INotificationService notificationService)
    {
        _db = db;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUser.GetUserId();
        
        // Add user to a group based on their user ID
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        
        // Add to all conversation groups the user is part of
        var conversations = await GetUserConversationsAsync(userId);
        foreach (var conv in conversations)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conv}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUser.GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(int conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    public async Task SendMessage(int conversationId, string messageText, string? attachmentUrl = null)
    {
        try
        {
            var senderId = _currentUser.GetUserId();
            var now = DateTime.UtcNow;

            // Create the message
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                MessageText = messageText,
                AttachmentUrl = attachmentUrl,
                IsRead = false,
                CreatedAt = now
            };

            _db.Messages.Add(message);
            
            // Update conversation status
            var conversation = await _db.Conversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.Status = "active";
            }
            
            await _db.SaveChangesAsync();

            // Get sender info
            var sender = await _db.Users.FindAsync(senderId);

            // Broadcast to everyone in the conversation group
            await Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", new
            {
                messageId = message.MessageId,
                conversationId = message.ConversationId,
                senderId = message.SenderId,
                senderName = sender?.FullName ?? "Unknown",
                messageText = message.MessageText,
                attachmentUrl = message.AttachmentUrl,
                createdAt = message.CreatedAt.ToString("o"),
                isRead = message.IsRead
            });

            // Also send notification to the other user (buyer or seller)
            if (conversation != null)
            {
                try
                {
                    var recipientId = conversation.BuyerId == senderId ? conversation.SellerId : conversation.BuyerId;
                    
                    // Create notification in database
                    await _notificationService.CreateNotificationAsync(
                        recipientId,
                        $"Tin nhắn mới từ {sender?.FullName ?? "Người dùng"}",
                        messageText.Length > 100 ? messageText[..100] + "..." : messageText,
                        "chat",
                        $"/Chat/Index/{conversationId}"
                    );

                    // Send real-time notification via SignalR
                    await Clients.Group($"user_{recipientId}").SendAsync("NewMessageNotification", new
                    {
                        conversationId = conversationId,
                        productId = conversation.ProductId,
                        senderName = sender?.FullName ?? "Someone",
                        preview = messageText.Length > 50 ? messageText[..50] + "..." : messageText,
                        messageId = message.MessageId,
                        title = $"Tin nhắn mới từ {sender?.FullName ?? "Người dùng"}",
                        content = messageText.Length > 100 ? messageText[..100] + "..." : messageText,
                        icon = "bi-chat-dots",
                        iconClass = "bg-info",
                        linkUrl = $"/Chat/Index/{conversationId}"
                    });
                }
                catch (Exception ex)
                {
                    // Log but don't fail - notification is optional
                    Console.WriteLine($"Notification error (non-critical): {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendMessage error: {ex.Message}");
            throw; // Re-throw so client knows there was an error
        }
    }

    public async Task MarkAsRead(int conversationId)
    {
        var userId = _currentUser.GetUserId();
        
        var unreadMessages = await _db.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
            .ToListAsync();

        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
        }
        
        await _db.SaveChangesAsync();

        // Notify the other user that messages were read
        await Clients.Group($"conversation_{conversationId}").SendAsync("MessagesRead", new
        {
            conversationId = conversationId,
            readBy = userId
        });
    }

    public async Task<int> GetUnreadCount()
    {
        var userId = _currentUser.GetUserId();
        var count = await _db.Messages
            .Where(m => !m.IsRead && m.SenderId != userId)
            .Where(m => m.Conversation.BuyerId == userId || m.Conversation.SellerId == userId)
            .CountAsync();
        return count;
    }

    private async Task<List<int>> GetUserConversationsAsync(int userId)
    {
        return await _db.Conversations
            .Where(c => c.BuyerId == userId || c.SellerId == userId)
            .Select(c => c.ConversationId)
            .ToListAsync();
    }
}
