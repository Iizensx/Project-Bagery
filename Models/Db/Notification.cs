using System;

namespace _66022380.Models.Db;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public string? LinkUrl { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
