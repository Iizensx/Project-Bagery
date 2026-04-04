using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class Stock
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? Stock1 { get; set; }

    public int? CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();

    public virtual ICollection<PromotionRewardItem> PromotionRewardItems { get; set; } = new List<PromotionRewardItem>();
}
