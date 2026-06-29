using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(int userId, string title, string content, string type, string? linkUrl = null);
    Task<int> GetUnreadCountAsync(int userId);
    Task<List<Notification>> GetRecentNotificationsAsync(int userId, int count = 10);
}

public class NotificationService : INotificationService
{
    private readonly SafexchangeDbContext _db;

    public NotificationService(SafexchangeDbContext db)
    {
        _db = db;
    }

    public async Task CreateNotificationAsync(int userId, string title, string content, string type, string? linkUrl = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Content = content,
            NotificationType = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<List<Notification>> GetRecentNotificationsAsync(int userId, int count = 10)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
