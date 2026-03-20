using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using _66022380.Helpers;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers;

public class OrderController : Controller
{
    private readonly BakerydbContext _db;

    public OrderController(BakerydbContext db)
    {
        _db = db;
    }

    public IActionResult Checkout() => View("~/Views/Account/Checkout.cshtml");

    [HttpPost]
    public IActionResult CreateOrder([FromBody] OrderRequest model)
    {
        if (model == null || model.Items == null || !model.Items.Any())
            return Json(new { success = false, message = "ข้อมูลออเดอร์ไม่ถูกต้อง" });

        var order = new Order
        {
            UserId = model.UserId,
            AddressId = model.AddressId,
            PromotionId = model.PromotionId,
            TotalAmount = model.TotalAmount,
            OrderDate = DateTime.Now,
            Status = "Pending",
            PaymentStatus = "Pending"
        };

        _db.Orders.Add(order);
        _db.SaveChanges();

        if (model.PromotionId.HasValue && model.PromotionId > 0)
        {
            var userPromo = _db.UserPromotions.FirstOrDefault(up =>
                up.UserId == model.UserId &&
                up.PromotionId == model.PromotionId &&
                up.IsUsed == 0);

            if (userPromo != null)
            {
                userPromo.IsUsed = 1;
                userPromo.UsedAt = DateTime.Now;
            }
        }

        foreach (var item in model.Items)
        {
            var product = _db.Stocks.FirstOrDefault(s => s.ProductName == item.ProductName);
            if (product != null)
            {
                _db.Orderdetails.Add(new Orderdetail
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }
        }

        _db.SaveChanges();

        return Json(new { success = true, orderId = order.OrderId });
    }

    [HttpGet]
    public IActionResult GetCurrentUser()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdString, out int userId) && userId > 0)
            return Json(new { success = true, userId });

        return Json(new { success = false, userId = 0 });
    }

    [HttpGet]
    public IActionResult GetUserPromos(int userId)
    {
        var promos = _db.UserPromotions
            .Include(up => up.Promotion)
            .Where(up => up.UserId == userId && up.IsUsed == 0 && up.Promotion != null)
            .Select(up => new
            {
                up.PromotionId,
                up.Promotion.PromotionName,
                up.Promotion.Description,
                up.Promotion.DiscountValue,
                up.Promotion.DiscountType
            })
            .ToList();

        return Json(new { success = true, promos });
    }

    public IActionResult Payment(int orderId)
    {
        var order = _db.Orders
            .Include(o => o.Orderdetails)
            .ThenInclude(d => d.Product)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null)
            return RedirectToAction("Home", "Home");

        var payload = PromptPayHelper.GeneratePayload("0943253900", order.TotalAmount ?? 0);

        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(10);

        ViewBag.QrBase64 = Convert.ToBase64String(qrBytes);
        ViewBag.OrderId = orderId;
        ViewBag.TotalAmount = order.TotalAmount;

        return View("~/Views/Account/Payment.cshtml", order);
    }

    [HttpPost]
    public async Task<IActionResult> UploadSlip(int orderId, IFormFile slipImage)
    {
        try
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            if (slipImage == null || slipImage.Length == 0)
                return Json(new { success = false, message = "กรุณาเลือกไฟล์" });

            var fileName = $"slip_{orderId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(slipImage.FileName)}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "slips");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
                await slipImage.CopyToAsync(stream);

            order.SlipImagePath = "/uploads/slips/" + fileName;
            order.PaymentStatus = "PendingVerify";
            _db.SaveChanges();

            return Json(new { success = true, message = "อัปโหลดสลิปเรียบร้อย รอ Admin ยืนยัน" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
        }
    }
}
