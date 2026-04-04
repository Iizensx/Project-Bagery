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
    private const long PromotionClaimMaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> SupportedPromotionClaimExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    public OrderController(BakerydbContext db)
    {
        _db = db;
    }

    public IActionResult Checkout()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return RedirectToAction("Login", "Account");

        ViewBag.CurrentUserId = currentUserId;
        return View("~/Views/Account/Checkout.cshtml");
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] OrderRequest model)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อนชำระเงิน" });

        if (model == null || model.Items == null || !model.Items.Any())
            return Json(new { success = false, message = "ข้อมูลออเดอร์ไม่ถูกต้อง" });

        Promotion? selectedPromotion = null;
        var rewardItems = new List<PromotionRewardItem>();
        if (model.PromotionId.HasValue && model.PromotionId > 0)
        {
            selectedPromotion = _db.Promotions.FirstOrDefault(p => p.PromotionId == model.PromotionId.Value);
            if (selectedPromotion == null || !IsPromotionAvailable(selectedPromotion))
                return Json(new { success = false, message = "โปรโมชั่นนี้ปิดใช้งานหรือหมดเวลาแล้ว" });

            rewardItems = _db.PromotionRewardItems
                .Where(item => item.PromotionId == selectedPromotion.PromotionId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.RewardItemId)
                .ToList();

            if (selectedPromotion.PromoType == 2)
            {
                var totalQuantity = model.Items.Sum(i => i.Quantity);
                if (totalQuantity < (selectedPromotion.BuyQuantity ?? 0))
                    return Json(new { success = false, message = $"โปรโมชั่นนี้ต้องซื้ออย่างน้อย {selectedPromotion.BuyQuantity} ชิ้น" });
            }
        }

        var order = new Order
        {
            UserId = currentUserId,
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
                up.UserId == currentUserId &&
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
            var product = item.ProductId.HasValue && item.ProductId.Value > 0
                ? _db.Stocks.FirstOrDefault(s => s.ProductId == item.ProductId.Value)
                : _db.Stocks.FirstOrDefault(s => s.ProductName == item.ProductName);
            if (product == null)
                continue;

            _db.Orderdetails.Add(new Orderdetail
            {
                OrderId = order.OrderId,
                ProductId = product.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            });
        }

        if (selectedPromotion?.PromoType is 2 or 3)
        {
            if (rewardItems.Count == 0 &&
                selectedPromotion.RewardProductId.HasValue &&
                (selectedPromotion.RewardQuantity ?? 0) > 0)
            {
                rewardItems.Add(new PromotionRewardItem
                {
                    PromotionId = selectedPromotion.PromotionId,
                    ProductId = selectedPromotion.RewardProductId.Value,
                    Quantity = selectedPromotion.RewardQuantity ?? 1
                });
            }

            foreach (var rewardItem in rewardItems.Where(item => item.ProductId > 0 && item.Quantity > 0))
            {
                _db.Orderdetails.Add(new Orderdetail
                {
                    OrderId = order.OrderId,
                    ProductId = rewardItem.ProductId,
                    Quantity = rewardItem.Quantity,
                    UnitPrice = 0
                });
            }
        }

        _db.SaveChanges();

        NotificationHelper.AddNotification(
            _db,
            currentUserId,
            "order",
            "Order created",
            $"Your order #{order.OrderId} was created successfully.",
            Url.Action("Payment", "Order", new { orderId = order.OrderId }),
            "Order",
            order.OrderId);

        _db.SaveChanges();

        return Json(new { success = true, orderId = order.OrderId });
    }

    [HttpGet]
    public IActionResult GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
            return Json(new { success = true, userId });

        return Json(new { success = false, userId = 0 });
    }

    [HttpGet]
    public IActionResult GetUserPromos(int userId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0 || currentUserId != userId)
            return Json(new { success = false, promos = Array.Empty<object>() });

        var promos = _db.UserPromotions
            .Include(up => up.Promotion)
            .Where(up => up.UserId == userId && up.IsUsed == 0 && up.Promotion != null)
            .AsEnumerable()
            .Where(up => IsPromotionAvailable(up.Promotion))
            .Select(up => new
            {
                up.PromotionId,
                up.Promotion.PromotionName,
                up.Promotion.Description,
                up.Promotion.DiscountValue,
                up.Promotion.DiscountType,
                up.Promotion.PromoType,
                up.Promotion.ImagePath,
                up.Promotion.IsCombinable,
                up.Promotion.BuyQuantity,
                up.Promotion.RewardProductId,
                up.Promotion.RewardQuantity
            })
            .ToList();

        return Json(new { success = true, promos });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitPromotionClaim(int promotionId, IFormFile proofImage, string? note)
    {
        try
        {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdString, out var userId) || userId <= 0)
            return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อน" });

        var promotion = _db.Promotions.FirstOrDefault(p => p.PromotionId == promotionId);
        if (promotion == null || promotion.PromoType != 3 || !promotion.RequiresProof || !IsPromotionAvailable(promotion))
            return Json(new { success = false, message = "ไม่พบโปรโมชั่นอีเวนต์ที่เปิดรับอยู่ตอนนี้" });

        if (proofImage == null || proofImage.Length == 0)
            return Json(new { success = false, message = "กรุณาแนบรูปยืนยัน" });

        var extension = Path.GetExtension(proofImage.FileName);
        if (proofImage.Length > PromotionClaimMaxFileSizeBytes)
            return Json(new { success = false, message = "รูปมีขนาดใหญ่เกินไป กรุณาใช้ไฟล์ไม่เกิน 5 MB" });

        if (string.IsNullOrWhiteSpace(extension) || !SupportedPromotionClaimExtensions.Contains(extension))
            return Json(new { success = false, message = "รองรับเฉพาะไฟล์รูป .jpg, .jpeg, .png และ .webp" });

        var hasPendingClaim = _db.PromotionClaims.Any(c =>
            c.UserId == userId &&
            c.PromotionId == promotionId &&
            c.Status == "Pending");

        if (hasPendingClaim)
            return Json(new { success = false, message = "คุณส่งคำขอนี้ไว้แล้ว กรุณารอพนักงานตรวจสอบ" });

        if (_db.UserPromotions.Any(up => up.UserId == userId && up.PromotionId == promotionId))
            return Json(new { success = false, message = "ท่านมีโปรโมชั่นนี้อยู่แล้ว ไม่สามารถส่งอีกครั้งได้" });

        var alreadyOwned = _db.UserPromotions.Any(up => up.UserId == userId && up.PromotionId == promotionId);
        if (alreadyOwned)
            return Json(new { success = false, message = "คุณได้รับโปรโมชั่นนี้แล้ว" });

        var fileName = $"claim_{userId}_{promotionId}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "promotion-claims");

        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
            await proofImage.CopyToAsync(stream);

        _db.PromotionClaims.Add(new PromotionClaim
        {
            PromotionId = promotionId,
            UserId = userId,
            ProofImagePath = "/uploads/promotion-claims/" + fileName,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            Status = "Pending",
            RequestedAt = DateTime.Now
        });

        _db.SaveChanges();
        NotificationHelper.AddNotification(
            _db,
            userId,
            "promotion",
            "Promotion claim submitted",
            "Your promotion proof was sent and is waiting for review.",
            Url.Action("Promotion", "Home"),
            "Promotion",
            promotionId);
        _db.SaveChanges();
        return Json(new { success = true, message = "ส่งรูปยืนยันเรียบร้อย กรุณารอพนักงานอนุมัติ" });
        }
        catch
        {
            return Json(new { success = false, message = "ระบบมีปัญหาระหว่างบันทึกรูป กรุณาลองใหม่อีกครั้ง" });
        }
    }

    public IActionResult Payment(int orderId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return RedirectToAction("Login", "Account");

        var order = _db.Orders
            .Include(o => o.Orderdetails)
            .ThenInclude(d => d.Product)
            .Include(o => o.Promotion)
            .FirstOrDefault(o => o.OrderId == orderId && o.UserId == currentUserId);

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
            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0)
                return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อนชำระเงิน" });

            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId && o.UserId == currentUserId);
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
            NotificationHelper.AddNotification(
                _db,
                currentUserId,
                "payment",
                "Slip uploaded",
                $"Payment slip for order #{order.OrderId} was uploaded and is waiting for verification.",
                Url.Action("Delivery", "Delivery", new { area = "" }) + "#tracking-section",
                "Order",
                order.OrderId);
            _db.SaveChanges();

            return Json(new { success = true, message = "อัปโหลดสลิปเรียบร้อย รอ Admin ยืนยัน" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
        }
    }

    private static bool IsPromotionAvailable(Promotion promotion)
    {
        if (!promotion.IsActive)
            return false;

        var now = DateTime.Now;
        if (promotion.StartDate.HasValue && promotion.StartDate.Value > now)
            return false;

        if (promotion.EndDate.HasValue && promotion.EndDate.Value < now)
            return false;

        return true;
    }

    private int GetCurrentUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        return int.TryParse(userIdString, out var userId) ? userId : 0;
    }
}
