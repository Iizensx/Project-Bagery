using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

public class AdminPromotionController : AdminControllerBase
{
    public AdminPromotionController(BakerydbContext db) : base(db)
    {
    }

    public IActionResult PromotionAdmin()
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var promotions = Db.Promotions
            .OrderByDescending(p => p.PromotionId)
            .ToList();

        var users = Db.Users
            .OrderBy(u => u.UserId)
            .ToList();

        var giftedCounts = Db.UserPromotions
            .GroupBy(up => up.PromotionId)
            .ToDictionary(g => g.Key, g => g.Count());

        var usedCounts = Db.UserPromotions
            .Where(up => up.IsUsed == 1)
            .GroupBy(up => up.PromotionId)
            .ToDictionary(g => g.Key, g => g.Count());

        var model = new AdminPromotionViewModel
        {
            Promotions = promotions,
            Users = users,
            PromotionGiftCounts = giftedCounts,
            PromotionUsageCounts = usedCounts
        };

        return View("~/Views/admin/PromotionAdmin.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SavePromotion(int promotionId, string promotionName, string? description, decimal? discountValue, string discountType)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

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

        if (promotionId > 0)
        {
            var promotion = Db.Promotions.FirstOrDefault(p => p.PromotionId == promotionId);
            if (promotion == null)
            {
                TempData["PromotionError"] = "ไม่พบโปรโมชั่นที่ต้องการแก้ไข";
                return RedirectToAction("PromotionAdmin");
            }

            promotion.PromotionName = promotionName.Trim();
            promotion.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            promotion.DiscountValue = discountValue.Value;
            promotion.DiscountType = discountType;
            Db.SaveChanges();

            TempData["PromotionSuccess"] = "อัปเดตโปรโมชั่นเรียบร้อย";
            return RedirectToAction("PromotionAdmin");
        }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GiftPromotion(int promotionId, int userId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var promotion = Db.Promotions.FirstOrDefault(p => p.PromotionId == promotionId);
        var user = Db.Users.FirstOrDefault(u => u.UserId == userId);

        if (promotion == null || user == null)
        {
            TempData["PromotionError"] = "ไม่พบข้อมูลโปรโมชั่นหรือผู้ใช้";
            return RedirectToAction("PromotionAdmin");
        }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GiftPromotionToAll(int promotionId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var promotion = Db.Promotions.FirstOrDefault(p => p.PromotionId == promotionId);
        if (promotion == null)
        {
            TempData["PromotionError"] = "ไม่พบโปรโมชั่นที่ต้องการแจก";
            return RedirectToAction("PromotionAdmin");
        }

        var users = Db.Users.Select(u => u.UserId).ToList();
        var existingMap = Db.UserPromotions
            .Where(up => up.PromotionId == promotionId)
            .ToDictionary(up => up.UserId, up => up);

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
