using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace _66022380.Controllers;

public class HomeController : Controller
{
    private readonly BakerydbContext _db;

    public HomeController(BakerydbContext db)
    {
        _db = db;
    }

    public IActionResult Home()
    {
        ViewBag.PublicPromotions = GetPublicPromotions().Take(3).ToList();
        ViewBag.FeaturedProducts = _db.Stocks
            .Include(s => s.Category)
            .OrderBy(s => s.ProductId)
            .Take(8)
            .ToList();
        return View("~/Views/Account/Home.cshtml");
    }

    public IActionResult Menu()
    {
        var products = _db.Stocks
            .Include(s => s.Category)
            .OrderBy(s => s.Price ?? decimal.MaxValue)
            .ThenBy(s => s.ProductName)
            .ToList();

        return View("~/Views/Account/Menu.cshtml", products);
    }

    public IActionResult Promotion()
    {
        var promotions = GetPublicPromotions();

        var rewardProductIds = promotions
            .Where(p => p.RewardProductId.HasValue)
            .Select(p => p.RewardProductId!.Value)
            .Distinct()
            .ToList();

        var rewardMap = _db.Stocks
            .Where(s => rewardProductIds.Contains(s.ProductId))
            .ToDictionary(s => s.ProductId, s => s.ProductName);

        var model = new UserPromotionPageViewModel
        {
            Promotions = promotions,
            EventPromotions = promotions.Where(p => p.PromoType == 3).ToList(),
            RewardProductNames = rewardMap
        };

        return View("~/Views/Account/Promotion.cshtml", model);
    }

    public IActionResult Contact()
    {
        return RedirectToAction("Home");
    }

    public IActionResult lab8()
    {
        var users = (from u in _db.Users select u).ToList();
        return View(users);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private List<Promotion> GetPublicPromotions()
    {
        var now = DateTime.Now;

        return _db.Promotions
            .Where(p => p.IsActive &&
                        !string.IsNullOrEmpty(p.ImagePath) &&
                        (!p.StartDate.HasValue || p.StartDate <= now) &&
                        (!p.EndDate.HasValue || p.EndDate >= now))
            .OrderByDescending(p => p.PromoType)
            .ThenByDescending(p => p.StartDate)
            .ThenByDescending(p => p.PromotionId)
            .ToList();
    }
}
