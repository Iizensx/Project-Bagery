using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

public class AdminDashboardController : AdminControllerBase
{
    public AdminDashboardController(BakerydbContext db) : base(db)
    {
    }

    public IActionResult Dashbordadmin()
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var orders = Db.Orders
            .Include(o => o.User)
            .ToList();

        var stocks = Db.Stocks
            .Include(s => s.Category)
            .ToList();

        var paidStatuses = new[] { "Paid", "Preparing", "Shipped", "Completed" };

        var model = new AdminDashboardViewModel
        {
            TodayRevenue = orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == today && paidStatuses.Contains(o.Status ?? ""))
                .Sum(o => o.TotalAmount ?? 0),
            TodayOrders = orders.Count(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == today),
            PendingVerificationOrders = orders.Count(o => o.PaymentStatus == "PendingVerify"),
            TotalUsers = Db.Users.Count(),
            LowStockProducts = stocks.Count(s => (s.Stock1 ?? 0) > 0 && (s.Stock1 ?? 0) <= 10),
            OutOfStockProducts = stocks.Count(s => (s.Stock1 ?? 0) <= 0),
            ActivePromotions = Db.Promotions.Count(),
            CompletedOrders = orders.Count(o => o.Status == "Completed"),
            PreparingOrders = orders.Count(o => o.Status == "Preparing"),
            ShippedOrders = orders.Count(o => o.Status == "Shipped"),
            MonthlyRevenue = Enumerable.Range(0, 6)
                .Select(offset =>
                {
                    var targetMonth = monthStart.AddMonths(-(5 - offset));
                    var monthOrders = orders.Where(o =>
                        o.OrderDate.HasValue &&
                        o.OrderDate.Value.Year == targetMonth.Year &&
                        o.OrderDate.Value.Month == targetMonth.Month &&
                        paidStatuses.Contains(o.Status ?? ""));

                    return new AdminDashboardMonthlyRevenueItem
                    {
                        Label = targetMonth.ToString("MMM"),
                        Revenue = monthOrders.Sum(o => o.TotalAmount ?? 0),
                        Orders = monthOrders.Count()
                    };
                })
                .ToList(),
            RecentOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderId)
                .Take(6)
                .Select(o => new AdminDashboardRecentOrderItem
                {
                    OrderId = o.OrderId,
                    CustomerName = string.IsNullOrWhiteSpace(o.User?.Username) ? "-" : o.User.Username,
                    TotalAmount = o.TotalAmount ?? 0,
                    Status = string.IsNullOrWhiteSpace(o.Status) ? "-" : o.Status,
                    PaymentStatus = string.IsNullOrWhiteSpace(o.PaymentStatus) ? "-" : o.PaymentStatus,
                    OrderDateText = o.OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "-"
                })
                .ToList(),
            LowStockItems = stocks
                .Where(s => (s.Stock1 ?? 0) <= 10)
                .OrderBy(s => s.Stock1 ?? 0)
                .ThenBy(s => s.ProductName)
                .Take(6)
                .Select(s => new AdminDashboardLowStockItem
                {
                    ProductId = s.ProductId,
                    ProductName = s.ProductName,
                    CategoryName = s.Category?.CategoryName ?? "-",
                    Stock = s.Stock1 ?? 0
                })
                .ToList(),
            CategorySummaries = stocks
                .GroupBy(s => s.Category?.CategoryName ?? "ไม่ระบุหมวดหมู่")
                .Select(g => new AdminDashboardCategoryItem
                {
                    CategoryName = g.Key,
                    ProductCount = g.Count(),
                    TotalStock = g.Sum(x => x.Stock1 ?? 0)
                })
                .OrderByDescending(x => x.ProductCount)
                .ToList()
        };

        return View("~/Views/admin/Dashbordadmin.cshtml", model);
    }
}
