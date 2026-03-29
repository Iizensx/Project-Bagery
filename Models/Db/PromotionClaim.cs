using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class PromotionClaim
{
    public int ClaimId { get; set; }

    public int PromotionId { get; set; }

    public int UserId { get; set; }

    public string ProofImagePath { get; set; } = null!;

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RequestedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByUserId { get; set; }

    public string? ReviewNote { get; set; }

    public virtual Promotion Promotion { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
