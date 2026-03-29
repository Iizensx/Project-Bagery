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

    public string? ImagePath { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int PromoType { get; set; }

    public bool IsActive { get; set; }

    public int? BuyQuantity { get; set; }

    public int? RewardProductId { get; set; }

    public int? RewardQuantity { get; set; }

    public bool IsCombinable { get; set; }

    public bool RequiresProof { get; set; }

    public int MaxUsePerUser { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PromotionClaim> PromotionClaims { get; set; } = new List<PromotionClaim>();

    public virtual ICollection<PromotionRewardItem> PromotionRewardItems { get; set; } = new List<PromotionRewardItem>();
}
