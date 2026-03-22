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

    // รับ BakerydbContext มาผ่าน Dependency Injection
    public OrderController(BakerydbContext db)
    {
        _db = db;
    }

    // GET: แสดงหน้า Checkout
    public IActionResult Checkout() => View("~/Views/Account/Checkout.cshtml");

    // POST: รับข้อมูลออเดอร์จาก JSON แล้วสร้าง Order ใหม่ลง database
    [HttpPost]
    public IActionResult CreateOrder([FromBody] OrderRequest model)
    {
        // ตรวจสอบว่ามีข้อมูลสินค้าส่งมาไหม
        if (model == null || model.Items == null || !model.Items.Any())
            return Json(new { success = false, message = "ข้อมูลออเดอร์ไม่ถูกต้อง" });

        // สร้าง Order ใหม่ โดยสถานะเริ่มต้นเป็น Pending ทั้งคู่
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
        _db.SaveChanges(); // บันทึกก่อนเพื่อให้ได้ OrderId มาใช้ต่อ

        // ถ้ามีการใช้โปรโมชั่น ให้ mark ว่าใช้แล้ว
        if (model.PromotionId.HasValue && model.PromotionId > 0)
        {
            var userPromo = _db.UserPromotions.FirstOrDefault(up =>
                up.UserId == model.UserId &&
                up.PromotionId == model.PromotionId &&
                up.IsUsed == 0); // หาโปรที่ยังไม่ได้ใช้

            if (userPromo != null)
            {
                userPromo.IsUsed = 1;        // mark ว่าใช้แล้ว
                userPromo.UsedAt = DateTime.Now;
            }
        }

        // วนลูปสร้าง OrderDetail ทีละสินค้า
        foreach (var item in model.Items)
        {
            // หาสินค้าจาก Stock ด้วยชื่อสินค้า
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

        _db.SaveChanges(); // บันทึก OrderDetail ทั้งหมด

        return Json(new { success = true, orderId = order.OrderId });
    }

    // GET: ดึง UserId ของคนที่ login อยู่จาก Session
    [HttpGet]
    public IActionResult GetCurrentUser()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdString, out int userId) && userId > 0)
            return Json(new { success = true, userId });

        return Json(new { success = false, userId = 0 });
    }

    // GET: ดึงโปรโมชั่นที่ user คนนี้มีอยู่และยังไม่ได้ใช้
    [HttpGet]
    public IActionResult GetUserPromos(int userId)
    {
        var promos = _db.UserPromotions
            .Include(up => up.Promotion)          // JOIN กับตาราง Promotion
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

    // GET: แสดงหน้า Payment พร้อม QR Code PromptPay
    public IActionResult Payment(int orderId)
    {
        // ดึงข้อมูล Order พร้อม OrderDetail และข้อมูลสินค้า
        var order = _db.Orders
            .Include(o => o.Orderdetails)
            .ThenInclude(d => d.Product)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null)
            return RedirectToAction("Home", "Home");

        // สร้าง PromptPay payload จากเบอร์โทรและยอดเงิน
        var payload = PromptPayHelper.GeneratePayload("0943253900", order.TotalAmount ?? 0);

        // แปลง payload เป็น QR Code รูปแบบ PNG
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(10); // ขนาด pixel ต่อ block

        // แปลงเป็น Base64 เพื่อฝังใน HTML ได้เลย
        ViewBag.QrBase64 = Convert.ToBase64String(qrBytes);
        ViewBag.OrderId = orderId;
        ViewBag.TotalAmount = order.TotalAmount;

        return View("~/Views/Account/Payment.cshtml", order);
    }

    // POST: รับไฟล์สลิปโอนเงินจาก user แล้วบันทึกลงเซิร์ฟเวอร์
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

            // ตั้งชื่อไฟล์ให้ไม่ซ้ำกันด้วย orderId + timestamp
            var fileName = $"slip_{orderId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(slipImage.FileName)}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "slips");

            // สร้าง folder ถ้ายังไม่มี
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            // เขียนไฟล์ลง disk แบบ async
            await using (var stream = new FileStream(filePath, FileMode.Create))
                await slipImage.CopyToAsync(stream);

            // บันทึก path ของสลิปและเปลี่ยน PaymentStatus รอ Admin ยืนยัน
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