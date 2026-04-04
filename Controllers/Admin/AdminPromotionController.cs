using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using _66022380.Helpers;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

public class AdminPromotionController : AdminControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private static readonly JsonSerializerOptions RewardItemJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AdminPromotionController(BakerydbContext db, IWebHostEnvironment environment) : base(db)
    {
        _environment = environment;
    }

    public IActionResult PromotionAdmin()
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var model = BuildPromotionViewModel();

        return View("~/Views/admin/PromotionAdmin.cshtml", model);
    }

    public IActionResult EventPromotion()
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        ViewBag.Title = "Event Promotion";
        ViewBag.PageTitle = "Event Promotion";

        var model = BuildPromotionViewModel();
        return View("~/Views/admin/EventPromotion.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePromotion(
        int promotionId,
        string promotionName,
        string? description,
        decimal? discountValue,
        string? discountType,
        int promoType,
        DateTime? startDate,
        DateTime? endDate,
        bool isActive,
        bool isCombinable,
        bool requiresProof,
        int? buyQuantity,
        int? rewardProductId,
        int? rewardQuantity,
        string? rewardItemsJson,
        int? maxUsePerUser,
        string? existingImagePath,
        IFormFile? imageFile)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        if (string.IsNullOrWhiteSpace(promotionName))
        {
            TempData["PromotionError"] = "กรุณากรอกชื่อโปรโมชั่น";
            return RedirectToAction("PromotionAdmin");
        }

        if (promoType is < 1 or > 3)
        {
            TempData["PromotionError"] = "กรุณาเลือกประเภทโปรโมชั่นให้ถูกต้อง";
            return RedirectToAction("PromotionAdmin");
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            TempData["PromotionError"] = "วันเริ่มโปรโมชั่นต้องไม่มากกว่าวันสิ้นสุด";
            return RedirectToAction("PromotionAdmin");
        }

        var normalizedDiscountType = NormalizeDiscountType(discountType);
        var rewardItems = ParseRewardItems(rewardItemsJson, rewardProductId, rewardQuantity);

        if (promoType == 1)
        {
            if (discountValue == null || discountValue <= 0)
            {
                TempData["PromotionError"] = "โปรโมชั่นส่วนลดต้องมีมูลค่าส่วนลดมากกว่า 0";
                return RedirectToAction("PromotionAdmin");
            }

            if (normalizedDiscountType == null)
            {
                TempData["PromotionError"] = "กรุณาเลือกประเภทส่วนลดเป็นเปอร์เซ็นต์หรือบาท";
                return RedirectToAction("PromotionAdmin");
            }
        }

        if (promoType == 2)
        {
            if (buyQuantity == null || buyQuantity <= 0 || rewardItems.Count == 0)
            {
                TempData["PromotionError"] = "โปรโมชั่นซื้อแถมต้องกำหนดจำนวนที่ซื้อ สินค้าของแถม และจำนวนของแถม";
                return RedirectToAction("PromotionAdmin");
            }
        }

        if (promoType == 3)
        {
            if (rewardItems.Count == 0)
            {
                TempData["PromotionError"] = "โปรโมชั่นอีเวนต์ต้องกำหนดขนมที่จะแถมและจำนวน";
                return RedirectToAction("PromotionAdmin");
            }
        }

        var resolvedMaxUsePerUser = Math.Max(1, maxUsePerUser ?? 1);
        var resolvedRequiresProof = promoType == 3 || requiresProof;
        var resolvedDiscountValue = promoType == 1 ? discountValue!.Value : 0m;
        var resolvedDiscountType = promoType == 1 ? normalizedDiscountType! : "Fixed";
        var resolvedImagePath = existingImagePath;
        var primaryRewardItem = rewardItems.FirstOrDefault();

        if (imageFile is { Length: > 0 })
        {
            resolvedImagePath = await SaveImageAsync(imageFile, "promotions");
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
            promotion.DiscountValue = resolvedDiscountValue;
            promotion.DiscountType = resolvedDiscountType;
            promotion.ImagePath = string.IsNullOrWhiteSpace(resolvedImagePath) ? null : resolvedImagePath;
            promotion.StartDate = startDate;
            promotion.EndDate = endDate;
            promotion.PromoType = promoType;
            promotion.IsActive = isActive;
            promotion.BuyQuantity = promoType == 2 ? buyQuantity : null;
            promotion.RewardProductId = promoType is 2 or 3 ? primaryRewardItem?.ProductId : null;
            promotion.RewardQuantity = promoType is 2 or 3 ? primaryRewardItem?.Quantity : null;
            promotion.IsCombinable = isCombinable;
            promotion.RequiresProof = resolvedRequiresProof;
            promotion.MaxUsePerUser = resolvedMaxUsePerUser;

            ReplaceRewardItems(promotion.PromotionId, promoType is 2 or 3 ? rewardItems : new List<PromotionRewardItemInput>());
            Db.SaveChanges();
            TempData["PromotionSuccess"] = "อัปเดตโปรโมชั่นเรียบร้อย";
            return RedirectToAction("PromotionAdmin");
        }

        var newPromotion = new Promotion
        {
            PromotionName = promotionName.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            DiscountValue = resolvedDiscountValue,
            DiscountType = resolvedDiscountType,
            ImagePath = string.IsNullOrWhiteSpace(resolvedImagePath) ? null : resolvedImagePath,
            StartDate = startDate,
            EndDate = endDate,
            PromoType = promoType,
            IsActive = isActive,
            BuyQuantity = promoType == 2 ? buyQuantity : null,
            RewardProductId = promoType is 2 or 3 ? primaryRewardItem?.ProductId : null,
            RewardQuantity = promoType is 2 or 3 ? primaryRewardItem?.Quantity : null,
            IsCombinable = isCombinable,
            RequiresProof = resolvedRequiresProof,
            MaxUsePerUser = resolvedMaxUsePerUser
        };

        Db.Promotions.Add(newPromotion);
        Db.SaveChanges();
        ReplaceRewardItems(newPromotion.PromotionId, promoType is 2 or 3 ? rewardItems : new List<PromotionRewardItemInput>());
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

        if (!IsPromotionAvailable(promotion))
        {
            TempData["PromotionError"] = "โปรโมชั่นนี้ยังไม่พร้อมแจก เพราะปิดใช้งานหรืออยู่นอกช่วงเวลา";
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

        NotificationHelper.AddNotification(
            Db,
            userId,
            "promotion",
            "New promotion received",
            $"A new promotion \"{promotion.PromotionName}\" was added to your account.",
            Url.Action("Promotion", "Home", new { area = "" }),
            "Promotion",
            promotionId);
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

        if (!IsPromotionAvailable(promotion))
        {
            TempData["PromotionError"] = "โปรโมชั่นนี้ยังไม่พร้อมแจก เพราะปิดใช้งานหรืออยู่นอกช่วงเวลา";
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

        NotificationHelper.AddNotifications(
            Db,
            users,
            "promotion",
            "New promotion received",
            $"A new promotion \"{promotion.PromotionName}\" was added to your account.",
            Url.Action("Promotion", "Home", new { area = "" }),
            "Promotion",
            promotionId);
        Db.SaveChanges();
        TempData["PromotionSuccess"] = "แจกโปรโมชั่นให้ผู้ใช้ทั้งหมดเรียบร้อย";
        return RedirectToAction("PromotionAdmin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApproveClaim(int claimId, string? returnTo)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var claim = Db.PromotionClaims
            .Include(c => c.User)
            .Include(c => c.Promotion)
            .FirstOrDefault(c => c.ClaimId == claimId);

        if (claim == null || claim.Status != "Pending")
        {
            TempData["PromotionError"] = "ไม่พบคำขอที่ต้องการอนุมัติ";
            return RedirectToPromotionPage(returnTo);
        }

        var existing = Db.UserPromotions.FirstOrDefault(up => up.UserId == claim.UserId && up.PromotionId == claim.PromotionId);
        if (existing == null)
        {
            Db.UserPromotions.Add(new UserPromotion
            {
                UserId = claim.UserId,
                PromotionId = claim.PromotionId,
                IsUsed = 0,
                UsedAt = null
            });
        }

        claim.Status = "Approved";
        claim.ReviewedAt = DateTime.Now;
        claim.ReviewedByUserId = GetCurrentUserId();
        claim.ReviewNote = "อนุมัติโดยพนักงาน";

        Db.SaveChanges();
        TempData["PromotionSuccess"] = $"อนุมัติคำขอของ {claim.User.Username} และมอบโปรโมชั่นเรียบร้อย";
        NotificationHelper.AddNotification(
            Db,
            claim.UserId,
            "promotion",
            "Promotion approved",
            $"Your request for \"{claim.Promotion?.PromotionName ?? "promotion"}\" was approved.",
            Url.Action("Promotion", "Home", new { area = "" }),
            "Promotion",
            claim.PromotionId);
        Db.SaveChanges();
        return RedirectToPromotionPage(returnTo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectClaim(int claimId, string? reviewNote, string? returnTo)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var claim = Db.PromotionClaims
            .Include(c => c.User)
            .Include(c => c.Promotion)
            .FirstOrDefault(c => c.ClaimId == claimId);

        if (claim == null || claim.Status != "Pending")
        {
            TempData["PromotionError"] = "ไม่พบคำขอที่ต้องการปฏิเสธ";
            return RedirectToPromotionPage(returnTo);
        }

        claim.Status = "Rejected";
        claim.ReviewedAt = DateTime.Now;
        claim.ReviewedByUserId = GetCurrentUserId();
        claim.ReviewNote = string.IsNullOrWhiteSpace(reviewNote) ? "รูปไม่ผ่านการตรวจสอบ" : reviewNote.Trim();

        Db.SaveChanges();
        TempData["PromotionSuccess"] = $"ปฏิเสธคำขอของ {claim.User.Username} เรียบร้อย";
        NotificationHelper.AddNotification(
            Db,
            claim.UserId,
            "promotion",
            "Promotion rejected",
            $"Your request for \"{claim.Promotion?.PromotionName ?? "promotion"}\" was rejected.",
            Url.Action("Promotion", "Home", new { area = "" }),
            "Promotion",
            claim.PromotionId);
        Db.SaveChanges();
        return RedirectToPromotionPage(returnTo);
    }

    private List<PromotionRewardItemInput> ParseRewardItems(string? rewardItemsJson, int? rewardProductId, int? rewardQuantity)
    {
        if (!string.IsNullOrWhiteSpace(rewardItemsJson))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<PromotionRewardItemInput>>(rewardItemsJson, RewardItemJsonOptions);
                if (parsed != null)
                {
                    return parsed
                        .Where(item => item.ProductId > 0 && item.Quantity > 0)
                        .Select((item, index) => new PromotionRewardItemInput
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            SortOrder = index
                        })
                        .ToList();
                }
            }
            catch
            {
            }
        }

        if (rewardProductId.HasValue && rewardProductId.Value > 0 && rewardQuantity.HasValue && rewardQuantity.Value > 0)
        {
            return new List<PromotionRewardItemInput>
            {
                new()
                {
                    ProductId = rewardProductId.Value,
                    Quantity = rewardQuantity.Value,
                    SortOrder = 0
                }
            };
        }

        return new List<PromotionRewardItemInput>();
    }

    private void ReplaceRewardItems(int promotionId, List<PromotionRewardItemInput> rewardItems)
    {
        var existingItems = Db.PromotionRewardItems
            .Where(item => item.PromotionId == promotionId)
            .ToList();

        if (existingItems.Count > 0)
            Db.PromotionRewardItems.RemoveRange(existingItems);

        var normalizedItems = rewardItems
            .Where(item => item.ProductId > 0 && item.Quantity > 0)
            .Select((item, index) => new PromotionRewardItem
            {
                PromotionId = promotionId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                SortOrder = index
            })
            .ToList();

        if (normalizedItems.Count > 0)
            Db.PromotionRewardItems.AddRange(normalizedItems);
    }

    private AdminPromotionViewModel BuildPromotionViewModel()
    {
        var promotions = Db.Promotions
            .OrderByDescending(p => p.PromotionId)
            .ToList();

        var users = Db.Users
            .OrderBy(u => u.UserId)
            .ToList();

        var stocks = Db.Stocks
            .OrderBy(s => s.ProductName)
            .ToList();

        var pendingClaims = Db.PromotionClaims
            .Include(c => c.User)
            .Include(c => c.Promotion)
            .Where(c => c.Status == "Pending")
            .OrderByDescending(c => c.RequestedAt)
            .ToList();

        var giftedCounts = Db.UserPromotions
            .GroupBy(up => up.PromotionId)
            .ToDictionary(g => g.Key, g => g.Count());

        var usedCounts = Db.UserPromotions
            .Where(up => up.IsUsed == 1)
            .GroupBy(up => up.PromotionId)
            .ToDictionary(g => g.Key, g => g.Count());

        var rewardItems = Db.PromotionRewardItems
            .Include(item => item.Product)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.RewardItemId)
            .AsEnumerable()
            .GroupBy(item => item.PromotionId)
            .ToDictionary(group => group.Key, group => group.ToList());

        return new AdminPromotionViewModel
        {
            Promotions = promotions,
            Users = users,
            Stocks = stocks,
            PendingClaims = pendingClaims,
            PromotionGiftCounts = giftedCounts,
            PromotionUsageCounts = usedCounts,
            PromotionRewardItems = rewardItems
        };
    }

    private IActionResult RedirectToPromotionPage(string? returnTo)
    {
        if (string.Equals(returnTo, nameof(PromotionAdmin), StringComparison.Ordinal))
            return RedirectToAction(nameof(PromotionAdmin));

        return RedirectToAction(nameof(EventPromotion));
    }

    private static string? NormalizeDiscountType(string? discountType)
    {
        if (string.Equals(discountType, "Percent", StringComparison.OrdinalIgnoreCase))
            return "Percent";

        if (string.Equals(discountType, "Fixed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(discountType, "Baht", StringComparison.OrdinalIgnoreCase))
            return "Fixed";

        return null;
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

    private async Task<string> SaveImageAsync(IFormFile file, string folderName)
    {
        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadsRoot);

        var safeExtension = Path.GetExtension(file.FileName);
        var fileName = $"{folderName}_{DateTime.Now:yyyyMMddHHmmssfff}{safeExtension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folderName}/{fileName}";
    }

    private sealed class PromotionRewardItemInput
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public int SortOrder { get; set; }
    }
}
