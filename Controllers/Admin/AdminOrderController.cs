using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

public class AdminOrderController : AdminControllerBase
{
    private readonly ILogger<AdminOrderController> _logger;

    public AdminOrderController(BakerydbContext db, ILogger<AdminOrderController> logger) : base(db)
    {
        _logger = logger;
    }

    public IActionResult Order()
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var orders = Db.Orders
            .Include(o => o.User)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View("~/Views/admin/Order.cshtml", orders);
    }

    [HttpPost]
    public IActionResult ConfirmPayment(int orderId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        try
        {
            var order = Db.Orders
                .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            foreach (var detail in order.Orderdetails)
            {
                var product = detail.Product;
                if (product != null)
                {
                    product.Stock1 = Math.Max(0, (product.Stock1 ?? 0) - (detail.Quantity ?? 0));
                }
            }

            order.PaymentStatus = "Paid";
            order.Status = "Paid";
            Db.Update(order);
            Db.SaveChanges();

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

    [HttpGet]
    public IActionResult GetOrderDetails(int orderId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var order = Db.Orders
            .Include(o => o.User)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null)
            return Json(new { success = false, message = "ไม่พบออเดอร์" });

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

    [HttpPost]
    public IActionResult AcceptOrder(int orderId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        try
        {
            var order = Db.Orders
                .Include(o => o.Orderdetails)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            if (order.Status == "Preparing" || order.Status == "Shipped")
                return Json(new { success = false, message = "ออเดอร์นี้ประมวลผลแล้ว" });

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

    [HttpPost]
    public IActionResult ShipOrder(int orderId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        try
        {
            var order = Db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            if (order.Status != "Preparing")
                return Json(new { success = false, message = "ออเดอร์ต้องอยู่ในสถานะ Preparing เท่านั้น" });

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
