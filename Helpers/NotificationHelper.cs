using _66022380.Models.Db;

namespace _66022380.Helpers;

public static class NotificationHelper
{
    public static void AddNotification(
        BakerydbContext db,
        int userId,
        string type,
        string title,
        string? message,
        string? linkUrl = null,
        string? referenceType = null,
        int? referenceId = null)
    {
        if (userId <= 0)
            return;

        db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = string.IsNullOrWhiteSpace(type) ? "system" : type.Trim(),
            Title = string.IsNullOrWhiteSpace(title) ? "Notification" : title.Trim(),
            Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim(),
            LinkUrl = string.IsNullOrWhiteSpace(linkUrl) ? null : linkUrl.Trim(),
            ReferenceType = string.IsNullOrWhiteSpace(referenceType) ? null : referenceType.Trim(),
            ReferenceId = referenceId,
            IsRead = false,
            CreatedAt = DateTime.Now
        });
    }

    public static void AddNotifications(
        BakerydbContext db,
        IEnumerable<int> userIds,
        string type,
        string title,
        string? message,
        string? linkUrl = null,
        string? referenceType = null,
        int? referenceId = null)
    {
        foreach (var userId in userIds.Where(id => id > 0).Distinct())
        {
            AddNotification(db, userId, type, title, message, linkUrl, referenceType, referenceId);
        }
    }
}
