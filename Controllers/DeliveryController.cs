using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers;

public class DeliveryController : Controller
{
    private readonly BakerydbContext _db;
    private readonly ILogger<DeliveryController> _logger;

    public DeliveryController(BakerydbContext db, ILogger<DeliveryController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Delivery()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdString, out var userId) || userId <= 0)
            return View("~/Views/Account/Delivery.cshtml", new DeliveryTrackingViewModel());

        var orders = _db.Orders
            .Include(o => o.Address)
            .Include(o => o.Promotion)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .Where(o => o.UserId == userId && o.Status != "Cancelled")
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderId)
            .ToList();

        var activeOrder = orders.FirstOrDefault(o => o.Status != "Completed");
        var userOrderIds = orders.Select(o => o.OrderId).ToList();
        var historyOrders = _db.Historyorders
            .Where(h => h.UserId == userId || (h.OrderId.HasValue && userOrderIds.Contains(h.OrderId.Value)))
            .OrderByDescending(h => h.CompletedAt)
            .ThenByDescending(h => h.HistoryOrderId)
            .ToList();

        var completedOrders = orders
            .Where(o => o.Status == "Completed")
            .ToList();

        return View("~/Views/Account/Delivery.cshtml", BuildDeliveryTrackingViewModel(activeOrder, historyOrders, completedOrders));
    }

    [HttpPost]
    public IActionResult CompleteOrder(int orderId)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdString, out var userId) || userId <= 0)
            return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อน" });

        try
        {
            var order = _db.Orders
                .Include(o => o.Address)
                .Include(o => o.Promotion)
                .Include(o => o.Orderdetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
                return Json(new { success = false, message = "ไม่พบออเดอร์" });

            if (order.Status != "Shipped")
                return Json(new { success = false, message = "ออเดอร์นี้ยังไม่อยู่ในสถานะจัดส่ง" });

            var existingHistory = _db.Historyorders.FirstOrDefault(h => h.OrderId == order.OrderId);
            if (existingHistory == null)
            {
                var deliveryAddress = string.Join(", ", new[]
                {
                    order.Address?.AddressLine,
                    order.Address?.District,
                    order.Address?.Province,
                    order.Address?.PostalCode
                }.Where(part => !string.IsNullOrWhiteSpace(part)));

                var itemSummary = string.Join(", ", order.Orderdetails.Select(d => $"{d.Product?.ProductName} x{d.Quantity}"));

                _db.Historyorders.Add(new Historyorder
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    OrderDate = order.OrderDate,
                    CompletedAt = DateTime.Now,
                    TotalAmount = order.TotalAmount,
                    Status = "Completed",
                    PaymentStatus = order.PaymentStatus,
                    DeliveryAddress = deliveryAddress,
                    ItemSummary = itemSummary
                });
            }

            order.Status = "Completed";
            _db.Update(order);
            _db.SaveChanges();

            return Json(new { success = true, message = "ปิดออเดอร์เรียบร้อย ขอบคุณที่ใช้บริการ" });
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError("Error completing order {OrderId}: {Message}", orderId, innerMsg);
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + innerMsg });
        }
    }

    private DeliveryTrackingViewModel BuildDeliveryTrackingViewModel(Order? order, List<Historyorder> historyOrders, List<Order>? completedOrders = null)
    {
        var history = historyOrders.Select(h => new DeliveryOrderHistoryItem
        {
            OrderId = h.OrderId ?? 0,
            OrderNumber = h.OrderId.HasValue ? $"ORD-{h.OrderId}" : "-",
            OrderStatus = h.Status ?? "Completed",
            PaymentStatus = h.PaymentStatus ?? "Pending",
            CreatedAtText = h.CompletedAt?.ToString("dd/MM/yyyy HH:mm") ?? h.OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "-",
            SortDate = h.CompletedAt ?? h.OrderDate ?? DateTime.MinValue,
            TotalAmount = h.TotalAmount ?? 0,
            DeliveryAddress = string.IsNullOrWhiteSpace(h.DeliveryAddress) ? "-" : h.DeliveryAddress,
            ItemSummary = string.IsNullOrWhiteSpace(h.ItemSummary) ? "-" : h.ItemSummary,
            PromotionName = "-",
            DiscountDisplay = "-",
            IsActive = false
        }).ToList();

        if (completedOrders != null && completedOrders.Count > 0)
        {
            var completedOrderMap = completedOrders.ToDictionary(o => o.OrderId);

            foreach (var item in history.Where(h => h.OrderId > 0))
            {
                if (completedOrderMap.TryGetValue(item.OrderId, out var matchedOrder))
                {
                    item.PromotionName = matchedOrder.Promotion?.PromotionName?.Trim() ?? "-";
                    item.DiscountDisplay = BuildDiscountDisplay(matchedOrder.Promotion);
                }
            }

            var historyOrderIds = history
                .Where(h => h.OrderId > 0)
                .Select(h => h.OrderId)
                .ToHashSet();

            var fallbackHistory = completedOrders
                .Where(o => !historyOrderIds.Contains(o.OrderId))
                .Select(o => new DeliveryOrderHistoryItem
                {
                    OrderId = o.OrderId,
                    OrderNumber = $"ORD-{o.OrderId}",
                    OrderStatus = string.IsNullOrWhiteSpace(o.Status) ? "Completed" : o.Status,
                    PaymentStatus = string.IsNullOrWhiteSpace(o.PaymentStatus) ? "Pending" : o.PaymentStatus,
                    CreatedAtText = o.OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "-",
                    SortDate = o.OrderDate ?? DateTime.MinValue,
                    TotalAmount = o.TotalAmount ?? 0,
                    DeliveryAddress = BuildDeliveryAddress(o),
                    ItemSummary = BuildItemSummary(o),
                    PromotionName = o.Promotion?.PromotionName?.Trim() ?? "-",
                    DiscountDisplay = BuildDiscountDisplay(o.Promotion),
                    IsActive = false
                });

            history.AddRange(fallbackHistory);
            history = history
                .OrderByDescending(h => h.SortDate)
                .ThenByDescending(h => h.OrderId)
                .ToList();
        }

        if (order == null)
        {
            return new DeliveryTrackingViewModel
            {
                OrderHistory = history
            };
        }

        var normalizedStatus = order.Status ?? "Pending";
        var normalizedPaymentStatus = order.PaymentStatus ?? "Pending";
        var trackingStage = normalizedStatus switch
        {
            "Completed" => 5,
            "Shipped" => 4,
            "Preparing" => 3,
            "Paid" => 2,
            _ => 1
        };

        var statusTitle = trackingStage switch
        {
            5 => "จัดส่งสำเร็จ",
            4 => "กำลังจัดส่ง",
            3 => "กำลังเตรียมออเดอร์",
            2 => "ชำระเงินสำเร็จ",
            _ when normalizedPaymentStatus == "PendingVerify" => "รอตรวจสอบสลิป",
            _ => "รอชำระเงิน"
        };

        var statusMessage = trackingStage switch
        {
            5 => "ผู้ใช้ยืนยันว่าได้รับออเดอร์แล้ว รายการนี้ถูกปิดและย้ายไปอยู่ในประวัติการสั่งซื้อ",
            4 => "ร้านจัดส่งออเดอร์แล้ว สามารถติดตามจากการ์ดนี้ได้ทันที",
            3 => "แอดมินรับออเดอร์แล้ว ตอนนี้ร้านกำลังเตรียมสินค้า",
            2 => "แอดมินยืนยันสลิปแล้ว รอร้านรับออเดอร์เพื่อเริ่มเตรียมสินค้า",
            _ when normalizedPaymentStatus == "PendingVerify" => "อัปโหลดสลิปแล้ว รอแอดมินตรวจสอบการชำระเงิน",
            _ => "ระบบสร้างออเดอร์แล้ว กรุณาไปหน้า Payment เพื่อชำระเงินและอัปโหลดสลิป"
        };

        var etaText = trackingStage switch
        {
            5 => "เสร็จสิ้น",
            4 => "กำลังจัดส่ง",
            3 => "เตรียมส่งเร็ว ๆ นี้",
            2 => "รอร้านรับออเดอร์",
            _ when normalizedPaymentStatus == "PendingVerify" => "รอตรวจสอบ",
            _ => "รอชำระเงิน"
        };

        var addressParts = new[]
        {
            order.Address?.AddressLine,
            order.Address?.District,
            order.Address?.Province,
            order.Address?.PostalCode
        }
        .Where(part => !string.IsNullOrWhiteSpace(part));

        return new DeliveryTrackingViewModel
        {
            HasOrder = true,
            OrderId = order.OrderId,
            OrderNumber = $"ORD-{order.OrderId}",
            OrderStatus = normalizedStatus,
            PaymentStatus = normalizedPaymentStatus,
            StatusTitle = statusTitle,
            StatusMessage = statusMessage,
            EtaText = etaText,
            TrackingStage = trackingStage,
            CreatedAtText = order.OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "-",
            TotalAmount = order.TotalAmount ?? 0,
            DeliveryAddress = string.Join(", ", addressParts),
            ItemSummary = string.Join(", ", order.Orderdetails.Select(d => $"{d.Product?.ProductName} x{d.Quantity}")),
            ShowPaymentAction = normalizedPaymentStatus != "Paid",
            PaymentUrl = Url.Action("Payment", "Order", new { orderId = order.OrderId }) ?? $"/Order/Payment?orderId={order.OrderId}",
            ShowDeliveryActions = normalizedStatus == "Shipped",
            IsCompleted = normalizedStatus == "Completed",
            OrderHistory = history
        };
    }

    private string BuildDeliveryAddress(Order order)
    {
        var addressParts = new[]
        {
            order.Address?.AddressLine,
            order.Address?.District,
            order.Address?.Province,
            order.Address?.PostalCode
        }
        .Where(part => !string.IsNullOrWhiteSpace(part));

        var address = string.Join(", ", addressParts);
        return string.IsNullOrWhiteSpace(address) ? "-" : address;
    }

    private string BuildItemSummary(Order order)
    {
        var itemSummary = string.Join(", ", order.Orderdetails.Select(d => $"{d.Product?.ProductName} x{d.Quantity}"));
        return string.IsNullOrWhiteSpace(itemSummary) ? "-" : itemSummary;
    }

    private string BuildDiscountDisplay(Promotion? promotion)
    {
        if (promotion == null)
            return "-";

        return promotion.DiscountType switch
        {
            "Percent" => $"{promotion.DiscountValue:0.##}%",
            "Baht" => $"฿{promotion.DiscountValue:0.##}",
            _ => $"{promotion.DiscountValue:0.##}"
        };
    }
}
