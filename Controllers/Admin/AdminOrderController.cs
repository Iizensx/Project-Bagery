using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

// Controller สำหรับจัดการออเดอร์ฝั่ง Admin
// ครอบคลุมการดูรายการออเดอร์, ตรวจสลิป, ยืนยันชำระเงิน และเปลี่ยนสถานะการจัดส่ง
public class AdminOrderController : AdminControllerBase
{
    private readonly ILogger<AdminOrderController> _logger;

    // รับ DbContext และ Logger มาผ่าน Dependency Injection
    public AdminOrderController(BakerydbContext db, ILogger<AdminOrderController> logger) : base(db)
    {
        _logger = logger;
    }

    // GET: แสดงรายการออเดอร์ทั้งหมด พร้อมข้อมูลผู้ใช้และรายการสินค้า
    public IActionResult Order()
    {
        // กันผู้ใช้ที่ไม่ใช่ Admin ออกจากหน้าจัดการออเดอร์
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ดึงออเดอร์ล่าสุดขึ้นก่อน เพื่อให้แอดมินเห็นงานใหม่เร็วที่สุด
        var orders = Db.Orders
            .Include(o => o.User)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View("~/Views/admin/Order.cshtml", orders);
    }

    // POST: ยืนยันการชำระเงิน และตัดสต็อกสินค้าตามจำนวนที่ลูกค้าสั่ง
    [HttpPost]
    public IActionResult ConfirmPayment(int orderId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        try
        {
            // ดึงออเดอร์พร้อมรายการสินค้า เพื่อใช้ปรับยอดสต็อก
            var order = Db.Orders
                .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            // วนลดสต็อกของสินค้าทุกรายการในออเดอร์
            // ใช้ Math.Max เพื่อป้องกันไม่ให้ค่าติดลบ
            foreach (var detail in order.Orderdetails)
            {
                var product = detail.Product;
                if (product != null)
                {
                    product.Stock1 = Math.Max(0, (product.Stock1 ?? 0) - (detail.Quantity ?? 0));
                }
            }

            // เมื่อยืนยันแล้ว ให้ถือว่าออเดอร์นี้ชำระเงินสำเร็จ
            order.PaymentStatus = "Paid";
            order.Status = "Paid";
            Db.Update(order);
            Db.SaveChanges();

            // บันทึก log เพื่อใช้ตรวจสอบย้อนหลัง
            _logger.LogInformation("Order {OrderId} payment confirmed - stock reduced", orderId);
            return Json(new { success = true, message = "ยืนยันการชำระเงินและลด Stock เรียบร้อย" });
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError("Error confirming payment for order {OrderId}: {Message}", orderId, innerMsg);
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + innerMsg });
        }
    }

    // GET: ดึงรายละเอียดออเดอร์แบบ JSON
    // ใช้สำหรับเปิด modal หรือดูข้อมูลออเดอร์แบบละเอียดในหน้าหลังบ้าน
    [HttpGet]
    public IActionResult GetOrderDetails(int orderId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ดึงทั้งข้อมูลลูกค้า รายการสินค้า และข้อมูลสลิป
        var order = Db.Orders
            .Include(o => o.User)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null)
            return Json(new { success = false, message = "ไม่พบออเดอร์" });

        // คืนข้อมูลในรูปแบบ JSON เพื่อให้ฝั่งหน้าเว็บนำไปแสดงต่อ
        return Json(new
        {
            success = true,
            orderId = order.OrderId,
            customerName = order.User?.Username,
            customerEmail = order.User?.Email,
            customerPhone = order.User?.Phone,
            orderDate = order.OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "-",
            totalAmount = order.TotalAmount,
            status = order.Status,
            paymentStatus = order.PaymentStatus,
            slipImagePath = order.SlipImagePath,
            items = order.Orderdetails.Select(d => new
            {
                productName = d.Product?.ProductName,
                quantity = d.Quantity,
                unitPrice = d.UnitPrice,
                subtotal = d.Quantity * d.UnitPrice
            }).ToList()
        });
    }

    // POST: เมื่อแอดมินรับงานแล้ว เปลี่ยนสถานะออเดอร์เป็น Preparing
    [HttpPost]
    public IActionResult AcceptOrder(int orderId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        try
        {
            // ดึงออเดอร์ที่ต้องการเปลี่ยนสถานะ
            var order = Db.Orders
                .Include(o => o.Orderdetails)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            // ถ้าออเดอร์ถูกดำเนินการไปแล้ว ไม่อนุญาตให้รับซ้ำ
            if (order.Status == "Preparing" || order.Status == "Shipped")
                return Json(new { success = false, message = "ออเดอร์นี้ประมวลผลแล้ว" });

            // เปลี่ยนสถานะเป็นกำลังเตรียมสินค้า
            order.Status = "Preparing";
            Db.Update(order);
            Db.SaveChanges();

            _logger.LogInformation("Order {OrderId} accepted - status changed to Preparing", orderId);
            return Json(new { success = true, message = "ยอมรับออเดอร์เรียบร้อย" });
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError("Error accepting order {OrderId}: {Message}", orderId, innerMsg);
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + innerMsg });
        }
    }

    // POST: เปลี่ยนสถานะออเดอร์จาก Preparing เป็น Shipped
    [HttpPost]
    public IActionResult ShipOrder(int orderId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        try
        {
            // ดึงออเดอร์ที่ต้องการจัดส่ง
            var order = Db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            // อนุญาตให้จัดส่งได้เฉพาะออเดอร์ที่ผ่านขั้น Preparing แล้ว
            if (order.Status != "Preparing")
                return Json(new { success = false, message = "ออเดอร์ต้องอยู่ในสถานะ Preparing เท่านั้น" });

            // เปลี่ยนสถานะเป็นจัดส่งแล้ว
            order.Status = "Shipped";
            Db.Update(order);
            Db.SaveChanges();

            _logger.LogInformation("Order {OrderId} shipped - status changed to Shipped", orderId);
            return Json(new { success = true, message = "จัดส่งสินค้าเรียบร้อย" });
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError("Error shipping order {OrderId}: {Message}", orderId, innerMsg);
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + innerMsg });
        }
    }
}
