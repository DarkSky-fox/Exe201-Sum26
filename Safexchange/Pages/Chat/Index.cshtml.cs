using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Chat;

[Authorize]
public class IndexModel : PageModel
{
    private readonly SafexchangeDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public IndexModel(SafexchangeDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public int CurrentUserId { get; set; }
    public List<ConversationListItem> Conversations { get; set; } = new();
    public int? SelectedConversationId { get; set; }
    public Conversation? SelectedConversation { get; set; }
    public List<Message> Messages { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id = null)
    {
        CurrentUserId = _currentUser.GetUserId();

        var conversations = await _db.Conversations
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Product)
            .Include(c => c.Messages)
            .Where(c => c.BuyerId == CurrentUserId || c.SellerId == CurrentUserId)
            .OrderByDescending(c => c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.CreatedAt).FirstOrDefault())
            .ToListAsync();

        foreach (var conv in conversations)
        {
            var unreadCount = await _db.Messages
                .CountAsync(m => m.ConversationId == conv.ConversationId 
                    && m.SenderId != CurrentUserId 
                    && !m.IsRead);

            var lastMessage = conv.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            Conversations.Add(new ConversationListItem
            {
                ConversationId = conv.ConversationId,
                BuyerId = conv.BuyerId,
                SellerId = conv.SellerId,
                Buyer = conv.Buyer,
                Seller = conv.Seller,
                Product = conv.Product,
                UnreadCount = unreadCount,
                LastMessage = lastMessage
            });
        }

        if (id.HasValue)
        {
            SelectedConversationId = id.Value;
            SelectedConversation = await _db.Conversations
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.ConversationId == id.Value);

            if (SelectedConversation != null)
            {
                // Get messages
                Messages = await _db.Messages
                    .Include(m => m.Sender)
                    .Where(m => m.ConversationId == id.Value)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();

                // Mark messages as read
                var unreadMessages = await _db.Messages
                    .Where(m => m.ConversationId == id.Value && m.SenderId != CurrentUserId && !m.IsRead)
                    .ToListAsync();

                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _db.SaveChangesAsync();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSendMessageAsync(int conversationId, string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
            return BadRequest();

        var senderId = _currentUser.GetUserId();

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            MessageText = messageText,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        return new JsonResult(new { success = true, messageId = message.MessageId });
    }

    public async Task<IActionResult> OnPostCreateConversationAsync(int productId)
    {
        var buyerId = _currentUser.GetUserId();

        // Check if conversation already exists
        var existingConv = await _db.Conversations
            .FirstOrDefaultAsync(c => c.ProductId == productId && c.BuyerId == buyerId);

        if (existingConv != null)
        {
            return new JsonResult(new { success = true, conversationId = existingConv.ConversationId });
        }

        var product = await _db.Products.FindAsync(productId);
        if (product == null)
            return NotFound();

        var conversation = new Conversation
        {
            ProductId = productId,
            BuyerId = buyerId,
            SellerId = product.SellerId,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync();

        return new JsonResult(new { success = true, conversationId = conversation.ConversationId });
    }
}

public class ConversationListItem
{
    public int ConversationId { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public int UnreadCount { get; set; }
    public Message? LastMessage { get; set; }
}
