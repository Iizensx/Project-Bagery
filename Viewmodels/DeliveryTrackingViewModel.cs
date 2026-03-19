namespace _66022380.Models;

public class DeliveryTrackingViewModel
{
    public bool HasOrder { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "-";
    public string OrderStatus { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Pending";
    public string StatusTitle { get; set; } = "ยังไม่มีออเดอร์";
    public string StatusMessage { get; set; } = "เริ่มสั่งสินค้าเพื่อให้ระบบสร้างออเดอร์และติดตามการจัดส่งได้";
    public string EtaText { get; set; } = "-";
    public int TrackingStage { get; set; }
    public string CreatedAtText { get; set; } = "-";
    public decimal TotalAmount { get; set; }
    public string DeliveryAddress { get; set; } = "-";
    public string ItemSummary { get; set; } = "-";
    public bool ShowPaymentAction { get; set; }
    public string PaymentUrl { get; set; } = "/Account/Checkout";
    public bool ShowDeliveryActions { get; set; }
    public bool IsCompleted { get; set; }
    public List<DeliveryOrderHistoryItem> OrderHistory { get; set; } = new();
}

public class DeliveryOrderHistoryItem
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "-";
    public string OrderStatus { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Pending";
    public string CreatedAtText { get; set; } = "-";
    public decimal TotalAmount { get; set; }
    public string ItemSummary { get; set; } = "-";
    public string DeliveryAddress { get; set; } = "-";
    public bool IsActive { get; set; }
}
