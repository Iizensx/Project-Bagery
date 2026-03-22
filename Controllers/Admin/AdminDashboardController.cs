using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

// Controller สำหรับหน้า Dashboard ฝั่ง Admin
// ใช้รวบรวมข้อมูลภาพรวมของระบบ เช่น รายได้ ออเดอร์ สต็อก และสถิติแยกตามหมวดหมู่
public class AdminDashboardController : AdminControllerBase
{
    // รับ BakerydbContext จากคลาสแม่
    public AdminDashboardController(BakerydbContext db) : base(db)
    {
    }

    // GET: แสดงหน้า Dashboard สำหรับผู้ดูแลระบบ
    public IActionResult Dashbordadmin()
    {
        // ถ้าไม่ใช่ Admin ไม่อนุญาตให้เข้าหน้านี้
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // กำหนดช่วงเวลาที่ใช้คำนวณ เช่น วันนี้ และวันแรกของเดือนปัจจุบัน
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // ดึงออเดอร์ทั้งหมดพร้อมข้อมูลผู้ใช้ เพื่อใช้คำนวณ KPI ต่าง ๆ
        var orders = Db.Orders
            .Include(o => o.User)
            .ToList();

        // ดึงรายการสินค้าและหมวดหมู่ เพื่อใช้คำนวณข้อมูลสต็อก
        var stocks = Db.Stocks
            .Include(s => s.Category)
            .ToList();

        // สถานะที่ถือว่า "รับรู้รายได้แล้ว"
        var paidStatuses = new[] { "Paid", "Preparing", "Shipped", "Completed" };

        // สร้าง ViewModel สำหรับส่งไปหน้า Dashboard
        var model = new AdminDashboardViewModel
        {
            // รายได้ของวันนี้ นับเฉพาะออเดอร์ที่อยู่ในสถานะที่ถือว่าชำระแล้ว
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
            // สรุปรายได้ย้อนหลัง 6 เดือน เพื่อใช้แสดงกราฟ
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
            // เรียงออเดอร์ล่าสุดจากใหม่ไปเก่า แล้วดึงมาแสดงเพียง 6 รายการ
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
            // เลือกสินค้าใกล้หมดโดยเรียงจากสต็อกน้อยที่สุดขึ้นมาก่อน
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
            // สรุปจำนวนสินค้าและยอดรวมสต็อกในแต่ละหมวดหมู่
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

        // ส่งข้อมูลทั้งหมดไปแสดงที่หน้า Dashboard
        return View("~/Views/admin/Dashbordadmin.cshtml", model);
    }
}
