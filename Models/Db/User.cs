using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? RoleId { get; set; }

    public string? OtpCode { get; set; }

    public DateTime? OtpExpiredAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Historyorder> Historyorders { get; set; } = new List<Historyorder>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PromotionClaim> PromotionClaims { get; set; } = new List<PromotionClaim>();

    public virtual Role? Role { get; set; }
}
