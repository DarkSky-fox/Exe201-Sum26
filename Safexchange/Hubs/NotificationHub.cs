using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Safexchange.Models;
using Safexchange.Services;
using Microsoft.EntityFrameworkCore;

namespace Safexchange.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly SafexchangeDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public NotificationHub(SafexchangeDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUser.GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUser.GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task MarkAsRead(int notificationId)
    {
        var userId = _currentUser.GetUserId();
        
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _db.SaveChangesAsync();
            
            // Notify client to update UI
            await Clients.Group($"user_{userId}").SendAsync("NotificationRead", new
            {
                notificationId = notificationId
            });
        }
    }

    public async Task MarkAllAsRead()
    {
        var userId = _currentUser.GetUserId();
        
        var unreadNotifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }
        
        await _db.SaveChangesAsync();
        
        // Notify client to update UI
        await Clients.Group($"user_{userId}").SendAsync("AllNotificationsRead");
    }

    public async Task<int> GetUnreadCount()
    {
        var userId = _currentUser.GetUserId();
        return await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }
}
