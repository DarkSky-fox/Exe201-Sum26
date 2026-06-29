using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Notifications;

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

    public List<NotificationViewModel> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _currentUser.GetUserId();

        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        foreach (var notif in notifications)
        {
            Notifications.Add(new NotificationViewModel
            {
                NotificationId = notif.NotificationId,
                Title = notif.Title,
                Content = notif.Content,
                NotificationType = notif.NotificationType,
                IsRead = notif.IsRead,
                CreatedAt = notif.CreatedAt,
                TimeAgo = GetTimeAgo(notif.CreatedAt),
                Icon = GetIconForType(notif.NotificationType),
                IconClass = GetIconClassForType(notif.NotificationType),
                LinkUrl = GetLinkForType(notif)
            });
        }

        UnreadCount = await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return Page();
    }

    public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
    {
        var userId = _currentUser.GetUserId();
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }

        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostMarkAllAsReadAsync()
    {
        var userId = _currentUser.GetUserId();
        var unreadNotifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notif in unreadNotifications)
        {
            notif.IsRead = true;
        }
        await _db.SaveChangesAsync();

        return new JsonResult(new { success = true });
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "Vừa xong";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} phút trước";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} giờ trước";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} ngày trước";

        return dateTime.ToString("dd/MM/yyyy HH:mm");
    }

    private static string GetIconForType(string type)
    {
        return type switch
        {
            "chat" => "bi-chat-dots",
            "order" => "bi-bag-check",
            "payment" => "bi-credit-card",
            "product" => "bi-box-seam",
            "system" => "bi-gear",
            "promotion" => "bi-tag",
            "report" => "bi-exclamation-triangle",
            _ => "bi-bell"
        };
    }

    private static string GetIconClassForType(string type)
    {
        return type switch
        {
            "chat" => "bg-info",
            "order" => "bg-success",
            "payment" => "bg-warning",
            "product" => "bg-primary",
            "system" => "bg-secondary",
            "promotion" => "bg-danger",
            "report" => "bg-warning",
            _ => "bg-primary"
        };
    }

    private static string GetLinkForType(Notification notification)
    {
        return notification.NotificationType switch
        {
            "chat" => $"/Chat/Index/{notification.NotificationId}",
            "order" => $"/Orders/Detail/{notification.NotificationId}",
            "payment" => $"/Orders/Detail/{notification.NotificationId}",
            "product" => $"/Products/Detail/{notification.NotificationId}",
            _ => "/Notifications/Index"
        };
    }
}

public class NotificationViewModel
{
    public int NotificationId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string NotificationType { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public string IconClass { get; set; } = null!;
    public string LinkUrl { get; set; } = null!;
}
