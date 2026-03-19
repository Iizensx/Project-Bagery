using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class Historyorder
{
    public int HistoryOrderId { get; set; }

    public int? OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public string? PaymentStatus { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? ItemSummary { get; set; }

    public virtual User? User { get; set; }
}
