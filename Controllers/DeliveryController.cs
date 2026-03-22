using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers;

public class DeliveryController : Controller
{
    private readonly BakerydbContext _db;
    private readonly ILogger<DeliveryController> _logger;

    // รับ BakerydbContext และ Logger มาผ่าน Dependency Injection
    public DeliveryController(BakerydbContext db, ILogger<DeliveryController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: แสดงหน้าติดตามการจัดส่งของ user ที่ login อยู่
    public IActionResult Delivery()
    {
        // ดึง UserId จาก Session ถ้าไม่มีให้แสดง View เปล่า
        var userIdString = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdString, out var userId) || userId <= 0)
            return View("~/Views/Account/Delivery.cshtml", new DeliveryTrackingViewModel());

        // ดึง Order ทั้งหมดของ user พร้อม JOIN ข้อมูลที่เกี่ยวข้อง ยกเว้น Cancelled
        var orders = _db.Orders
            .Include(o => o.Address)
            .Include(o => o.Promotion)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .Where(o => o.UserId == userId && o.Status != "Cancelled")
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderId)
            .ToList();

        // หา Order ที่ยังไม่เสร็จ (active) เอาอันแรกที่เจอ
        var activeOrder = orders.FirstOrDefault(o => o.Status != "Completed");

        // ดึงประวัติการสั่งซื้อที่ตรงกับ user หรือ OrderId ของ user
        var userOrderIds = orders.Select(o => o.OrderId).ToList();
        var historyOrders = _db.Historyorders
            .Where(h => h.UserId == userId || (h.OrderId.HasValue && userOrderIds.Contains(h.OrderId.Value)))
            .OrderByDescending(h => h.CompletedAt)
            .ThenByDescending(h => h.HistoryOrderId)
            .ToList();

        // แยก Order ที่ Completed ออกมาใช้แสดงประวัติ
        var completedOrders = orders
            .Where(o => o.Status == "Completed")
            .ToList();

        return View("~/Views/Account/Delivery.cshtml", BuildDeliveryTrackingViewModel(activeOrder, historyOrders, completedOrders));
    }

    // POST: user กดยืนยันว่าได้รับสินค้าแล้ว → เปลี่ยน Status เป็น Completed
    [HttpPost]
    public IActionResult CompleteOrder(int orderId)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdString, out var userId) || userId <= 0)
            return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อน" });

        try
        {
            // ดึง Order พร้อมข้อมูลที่เกี่ยวข้องทั้งหมด
            var order = _db.Orders
                .Include(o => o.Address)
                .Include(o => o.Promotion)
                .Include(o => o.Orderdetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
                return Json(new { success = false, message = "ไม่พบออเดอร์" });

            // ยืนยันได้เฉพาะ Order ที่อยู่ในสถานะ Shipped เท่านั้น
            if (order.Status != "Shipped")
                return Json(new { success = false, message = "ออเดอร์นี้ยังไม่อยู่ในสถานะจัดส่ง" });

            // ถ้ายังไม่มีใน History ให้สร้างใหม่
            var existingHistory = _db.Historyorders.FirstOrDefault(h => h.OrderId == order.OrderId);
            if (existingHistory == null)
            {
                // รวมที่อยู่จัดส่งเป็น string เดียว กรองส่วนที่ว่างออก
                var deliveryAddress = string.Join(", ", new[]
                {
                    order.Address?.AddressLine,
                    order.Address?.District,
                    order.Address?.Province,
                    order.Address?.PostalCode
                }.Where(part => !string.IsNullOrWhiteSpace(part)));

                // สรุปรายการสินค้าในออเดอร์ เช่น "เค้กช็อคโกแลต x2, คุกกี้ x1"
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

            // อัปเดต Status เป็น Completed แล้วบันทึก
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

    // Helper: สร้าง DeliveryTrackingViewModel จากข้อมูล Order และประวัติ
    private DeliveryTrackingViewModel BuildDeliveryTrackingViewModel(Order? order, List<Historyorder> historyOrders, List<Order>? completedOrders = null)
    {
        // แปลง Historyorder ทุกรายการเป็น DeliveryOrderHistoryItem
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
            // สร้าง Dictionary เพื่อ lookup Order ด้วย OrderId ได้เร็วขึ้น
            var completedOrderMap = completedOrders.ToDictionary(o => o.OrderId);

            // เติมข้อมูล Promotion ให้กับ History ที่มี OrderId ตรงกัน
            foreach (var item in history.Where(h => h.OrderId > 0))
            {
                if (completedOrderMap.TryGetValue(item.OrderId, out var matchedOrder))
                {
                    item.PromotionName = matchedOrder.Promotion?.PromotionName?.Trim() ?? "-";
                    item.DiscountDisplay = BuildDiscountDisplay(matchedOrder.Promotion);
                }
            }

            // หา Completed Order ที่ไม่มีใน History เพื่อเพิ่มเป็น fallback
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

            // เรียงประวัติทั้งหมดใหม่จากใหม่ไปเก่า
            history = history
                .OrderByDescending(h => h.SortDate)
                .ThenByDescending(h => h.OrderId)
                .ToList();
        }

        // ถ้าไม่มี Active Order ให้คืน ViewModel ที่มีแค่ประวัติ
        if (order == null)
        {
            return new DeliveryTrackingViewModel
            {
                OrderHistory = history
            };
        }

        var normalizedStatus = order.Status ?? "Pending";
        var normalizedPaymentStatus = order.PaymentStatus ?? "Pending";

        // แปลง Status เป็นตัวเลข Stage เพื่อแสดง Progress Bar
        var trackingStage = normalizedStatus switch
        {
            "Completed" => 5,
            "Shipped"   => 4,
            "Preparing" => 3,
            "Paid"      => 2,
            _           => 1
        };

        // หัวข้อสถานะที่แสดงบนหน้า Delivery
        var statusTitle = trackingStage switch
        {
            5 => "จัดส่งสำเร็จ",
            4 => "กำลังจัดส่ง",
            3 => "กำลังเตรียมออเดอร์",
            2 => "ชำระเงินสำเร็จ",
            _ when normalizedPaymentStatus == "PendingVerify" => "รอตรวจสอบสลิป",
            _ => "รอชำระเงิน"
        };

        // ข้อความอธิบายสถานะให้ user เข้าใจว่าตอนนี้อยู่ขั้นไหน
        var statusMessage = trackingStage switch
        {
            5 => "ผู้ใช้ยืนยันว่าได้รับออเดอร์แล้ว รายการนี้ถูกปิดและย้ายไปอยู่ในประวัติการสั่งซื้อ",
            4 => "ร้านจัดส่งออเดอร์แล้ว สามารถติดตามจากการ์ดนี้ได้ทันที",
            3 => "แอดมินรับออเดอร์แล้ว ตอนนี้ร้านกำลังเตรียมสินค้า",
            2 => "แอดมินยืนยันสลิปแล้ว รอร้านรับออเดอร์เพื่อเริ่มเตรียมสินค้า",
            _ when normalizedPaymentStatus == "PendingVerify" => "อัปโหลดสลิปแล้ว รอแอดมินตรวจสอบการชำระเงิน",
            _ => "ระบบสร้างออเดอร์แล้ว กรุณาไปหน้า Payment เพื่อชำระเงินและอัปโหลดสลิป"
        };

        // ข้อความ ETA แสดงสถานะโดยย่อ
        var etaText = trackingStage switch
        {
            5 => "เสร็จสิ้น",
            4 => "กำลังจัดส่ง",
            3 => "เตรียมส่งเร็ว ๆ นี้",
            2 => "รอร้านรับออเดอร์",
            _ when normalizedPaymentStatus == "PendingVerify" => "รอตรวจสอบ",
            _ => "รอชำระเงิน"
        };

        // รวมที่อยู่จัดส่งเป็น string เดียว กรองส่วนที่ว่างออก
        var addressParts = new[]
        {
            order.Address?.AddressLine,
            order.Address?.District,
            order.Address?.Province,
            order.Address?.PostalCode
        }
        .Where(part => !string.IsNullOrWhiteSpace(part));

        // คืน ViewModel ที่มีข้อมูลครบทั้ง Active Order และประวัติ
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
            // แสดงปุ่มชำระเงินถ้ายังไม่ได้จ่าย
            ShowPaymentAction = normalizedPaymentStatus != "Paid",
            PaymentUrl = Url.Action("Payment", "Order", new { orderId = order.OrderId }) ?? $"/Order/Payment?orderId={order.OrderId}",
            // แสดงปุ่มยืนยันรับสินค้าเฉพาะตอน Shipped
            ShowDeliveryActions = normalizedStatus == "Shipped",
            IsCompleted = normalizedStatus == "Completed",
            OrderHistory = history
        };
    }

    // Helper: รวมที่อยู่จัดส่งจาก Order เป็น string เดียว
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

    // Helper: สรุปรายการสินค้าในออเดอร์เป็น string เดียว เช่น "เค้ก x2, คุกกี้ x1"
    private string BuildItemSummary(Order order)
    {
        var itemSummary = string.Join(", ", order.Orderdetails.Select(d => $"{d.Product?.ProductName} x{d.Quantity}"));
        return string.IsNullOrWhiteSpace(itemSummary) ? "-" : itemSummary;
    }

    // Helper: แปลง Promotion เป็นข้อความส่วนลด เช่น "10%" หรือ "฿50"
    private string BuildDiscountDisplay(Promotion? promotion)
    {
        if (promotion == null)
            return "-";

        return promotion.DiscountType switch
        {
            "Percent" => $"{promotion.DiscountValue:0.##}%",
            "Baht"    => $"฿{promotion.DiscountValue:0.##}",
            _         => $"{promotion.DiscountValue:0.##}"
        };
    }
}