using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

// Controller สำหรับจัดการโปรโมชั่น
// รองรับการสร้างโปร แก้ไขโปร และแจกโปรโมชั่นให้ผู้ใช้รายคนหรือทุกคน
public class AdminPromotionController : AdminControllerBase
{
    // รับ BakerydbContext จากคลาสแม่
    public AdminPromotionController(BakerydbContext db) : base(db)
    {
    }

    // GET: แสดงหน้าจัดการโปรโมชั่น พร้อมข้อมูลผู้ใช้และสถิติการแจก/การใช้งาน
    public IActionResult PromotionAdmin()
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ดึงโปรล่าสุดขึ้นมาก่อน เพื่อให้แอดมินเห็นโปรใหม่ได้ง่าย
        var promotions = Db.Promotions
            .OrderByDescending(p => p.PromotionId)
            .ToList();

        // ดึงรายชื่อผู้ใช้ทั้งหมดสำหรับใช้ในฟอร์มแจกโปรรายบุคคล
        var users = Db.Users
            .OrderBy(u => u.UserId)
            .ToList();

        // นับจำนวนครั้งที่โปรโมชั่นแต่ละตัวถูกแจก
        var giftedCounts = Db.UserPromotions
            .GroupBy(up => up.PromotionId)
            .ToDictionary(g => g.Key, g => g.Count());

        // นับจำนวนครั้งที่โปรโมชั่นแต่ละตัวถูกใช้งานแล้ว
        var usedCounts = Db.UserPromotions
            .Where(up => up.IsUsed == 1)
            .GroupBy(up => up.PromotionId)
            .ToDictionary(g => g.Key, g => g.Count());

        // รวมข้อมูลทั้งหมดเป็น ViewModel เพื่อส่งไปหน้า PromotionAdmin
        var model = new AdminPromotionViewModel
        {
            Promotions = promotions,
            Users = users,
            PromotionGiftCounts = giftedCounts,
            PromotionUsageCounts = usedCounts
        };

        return View("~/Views/admin/PromotionAdmin.cshtml", model);
    }

    // POST: เพิ่มโปรโมชั่นใหม่ หรือแก้ไขโปรโมชั่นเดิม
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SavePromotion(int promotionId, string promotionName, string? description, decimal? discountValue, string discountType)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ตรวจสอบข้อมูลก่อนบันทึก
        if (string.IsNullOrWhiteSpace(promotionName))
        {
            TempData["PromotionError"] = "กรุณากรอกชื่อโปรโมชั่น";
            return RedirectToAction("PromotionAdmin");
        }

        if (discountValue == null || discountValue <= 0)
        {
            TempData["PromotionError"] = "กรุณากรอกมูลค่าส่วนลดให้ถูกต้อง";
            return RedirectToAction("PromotionAdmin");
        }

        if (discountType != "Percent" && discountType != "Baht")
        {
            TempData["PromotionError"] = "กรุณาเลือกประเภทส่วนลดเป็นเปอร์เซ็นต์หรือบาท";
            return RedirectToAction("PromotionAdmin");
        }

        // ถ้ามี promotionId แสดงว่าเป็นการแก้ไขโปรโมชั่นเดิม
        if (promotionId > 0)
        {
            var promotion = Db.Promotions.FirstOrDefault(p => p.PromotionId == promotionId);
            if (promotion == null)
            {
                TempData["PromotionError"] = "ไม่พบโปรโมชั่นที่ต้องการแก้ไข";
                return RedirectToAction("PromotionAdmin");
            }

            // อัปเดตรายละเอียดโปรโมชั่นเดิม
            promotion.PromotionName = promotionName.Trim();
            promotion.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            promotion.DiscountValue = discountValue.Value;
            promotion.DiscountType = discountType;
            Db.SaveChanges();

            TempData["PromotionSuccess"] = "อัปเดตโปรโมชั่นเรียบร้อย";
            return RedirectToAction("PromotionAdmin");
        }

        // ถ้าไม่มี promotionId ให้สร้างโปรโมชั่นใหม่
        Db.Promotions.Add(new Promotion
        {
            PromotionName = promotionName.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            DiscountValue = discountValue.Value,
            DiscountType = discountType
        });
        Db.SaveChanges();

        TempData["PromotionSuccess"] = "เพิ่มโปรโมชั่นใหม่เรียบร้อย";
        return RedirectToAction("PromotionAdmin");
    }

    // POST: แจกโปรโมชั่นให้ผู้ใช้รายบุคคล
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GiftPromotion(int promotionId, int userId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ตรวจสอบว่าทั้งโปรโมชั่นและผู้ใช้มีอยู่จริง
        var promotion = Db.Promotions.FirstOrDefault(p => p.PromotionId == promotionId);
        var user = Db.Users.FirstOrDefault(u => u.UserId == userId);

        if (promotion == null || user == null)
        {
            TempData["PromotionError"] = "ไม่พบข้อมูลโปรโมชั่นหรือผู้ใช้";
            return RedirectToAction("PromotionAdmin");
        }

        // ถ้าผู้ใช้คนนี้เคยได้รับโปรนี้แล้ว ให้รีเซ็ตสถานะการใช้งาน
        // ถ้ายังไม่เคยได้รับ ให้สร้างรายการใหม่ใน UserPromotions
        var existing = Db.UserPromotions.FirstOrDefault(up => up.PromotionId == promotionId && up.UserId == userId);
        if (existing == null)
        {
            Db.UserPromotions.Add(new UserPromotion
            {
                PromotionId = promotionId,
                UserId = userId,
                IsUsed = 0,
                UsedAt = null
            });
        }
        else
        {
            existing.IsUsed = 0;
            existing.UsedAt = null;
        }

        Db.SaveChanges();
        TempData["PromotionSuccess"] = $"มอบโปรโมชั่นให้ {user.Username} เรียบร้อย";
        return RedirectToAction("PromotionAdmin");
    }

    // POST: แจกโปรโมชั่นเดียวกันให้ผู้ใช้ทุกคนในระบบ
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GiftPromotionToAll(int promotionId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ตรวจสอบก่อนว่ามีโปรโมชั่นที่ต้องการแจกจริง
        var promotion = Db.Promotions.FirstOrDefault(p => p.PromotionId == promotionId);
        if (promotion == null)
        {
            TempData["PromotionError"] = "ไม่พบโปรโมชั่นที่ต้องการแจก";
            return RedirectToAction("PromotionAdmin");
        }

        // ดึงผู้ใช้ทั้งหมด และสร้างแผนที่ของรายการโปรเดิมเพื่อ lookup ได้เร็วขึ้น
        var users = Db.Users.Select(u => u.UserId).ToList();
        var existingMap = Db.UserPromotions
            .Where(up => up.PromotionId == promotionId)
            .ToDictionary(up => up.UserId, up => up);

        // วนแจกโปรให้ครบทุกคน
        // ถ้ามีอยู่แล้วให้รีเซ็ตสิทธิ์ ถ้ายังไม่มีก็เพิ่มรายการใหม่
        foreach (var userId in users)
        {
            if (existingMap.TryGetValue(userId, out var existing))
            {
                existing.IsUsed = 0;
                existing.UsedAt = null;
            }
            else
            {
                Db.UserPromotions.Add(new UserPromotion
                {
                    PromotionId = promotionId,
                    UserId = userId,
                    IsUsed = 0,
                    UsedAt = null
                });
            }
        }

        Db.SaveChanges();
        TempData["PromotionSuccess"] = "แจกโปรโมชั่นให้ผู้ใช้ทั้งหมดเรียบร้อย";
        return RedirectToAction("PromotionAdmin");
    }
}
