using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string PromotionName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal DiscountValue { get; set; }

    public string DiscountType { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
