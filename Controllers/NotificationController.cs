using Microsoft.AspNetCore.Mvc;
using _66022380.Models.Db;

namespace _66022380.Controllers;

public class NotificationController : Controller
{
    private readonly BakerydbContext _db;

    public NotificationController(BakerydbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetMyNotifications()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Unauthorized(new { success = false, notifications = Array.Empty<object>() });

        var notifications = _db.Notifications
            .Where(n => n.UserId == currentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(30)
            .Select(n => new
            {
                id = n.NotificationId,
                type = n.Type,
                title = n.Title,
                message = n.Message,
                href = n.LinkUrl,
                isRead = n.IsRead,
                createdAt = n.CreatedAt
            })
            .ToList();

        return Json(new { success = true, notifications });
    }

    [HttpPost]
    public IActionResult MarkAsRead(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Unauthorized(new { success = false });

        var notification = _db.Notifications.FirstOrDefault(n => n.NotificationId == id && n.UserId == currentUserId);
        if (notification == null)
            return Json(new { success = false, message = "Notification not found." });

        notification.IsRead = true;
        _db.SaveChanges();
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult MarkAllAsRead()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Unauthorized(new { success = false });

        var notifications = _db.Notifications
            .Where(n => n.UserId == currentUserId && !n.IsRead)
            .ToList();

        foreach (var notification in notifications)
            notification.IsRead = true;

        _db.SaveChanges();
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult ClearMyNotifications()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Unauthorized(new { success = false });

        var notifications = _db.Notifications
            .Where(n => n.UserId == currentUserId)
            .ToList();

        if (notifications.Count > 0)
        {
            _db.Notifications.RemoveRange(notifications);
            _db.SaveChanges();
        }

        return Json(new { success = true });
    }

    private int GetCurrentUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        return int.TryParse(userIdString, out var userId) ? userId : 0;
    }
}
