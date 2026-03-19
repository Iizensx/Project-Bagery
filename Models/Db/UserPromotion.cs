using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class UserPromotion
{
    public int UserId { get; set; }

    public int PromotionId { get; set; }

    public int IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Promotion Promotion { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
