using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public int? AddressId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public string? PaymentStatus { get; set; }

    public int? PromotionId { get; set; }

    public virtual Address? Address { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();

    public virtual Promotion? Promotion { get; set; }

    public virtual User? User { get; set; }

    public string? SlipImagePath { get; set; }
}
