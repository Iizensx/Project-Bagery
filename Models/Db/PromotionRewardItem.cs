using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class PromotionRewardItem
{
    public int RewardItemId { get; set; }

    public int PromotionId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public int SortOrder { get; set; }

    public virtual Promotion Promotion { get; set; } = null!;

    public virtual Stock Product { get; set; } = null!;
}
