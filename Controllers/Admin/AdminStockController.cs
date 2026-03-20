using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

public class AdminStockController : AdminControllerBase
{
    public AdminStockController(BakerydbContext db) : base(db)
    {
    }

    public IActionResult Stock()
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var model = new AdminStockViewModel
        {
            Categories = Db.Categories
                .Include(c => c.Stocks)
                .OrderBy(c => c.CategoryName)
                .ToList(),
            Products = Db.Stocks
                .Include(s => s.Category)
                .OrderBy(s => s.ProductName)
                .ToList()
        };

        return View("~/Views/admin/Stock.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveCategory(int categoryId, string categoryName, string? description)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            TempData["StockError"] = "กรุณากรอกชื่อหมวดหมู่";
            return RedirectToAction("Stock");
        }

        if (categoryId > 0)
        {
            var category = Db.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                TempData["StockError"] = "ไม่พบหมวดหมู่ที่ต้องการแก้ไข";
                return RedirectToAction("Stock");
            }

            category.CategoryName = categoryName.Trim();
            category.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Db.SaveChanges();

            TempData["StockSuccess"] = "อัปเดตหมวดหมู่เรียบร้อย";
            return RedirectToAction("Stock");
        }

        Db.Categories.Add(new Category
        {
            CategoryName = categoryName.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        });
        Db.SaveChanges();

        TempData["StockSuccess"] = "เพิ่มหมวดหมู่ใหม่เรียบร้อย";
        return RedirectToAction("Stock");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveStock(int productId, string productName, string? description, decimal? price, int? stock1, int? categoryId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        if (string.IsNullOrWhiteSpace(productName))
        {
            TempData["StockError"] = "กรุณากรอกชื่อสินค้า";
            return RedirectToAction("Stock");
        }

        if (categoryId == null || !Db.Categories.Any(c => c.CategoryId == categoryId))
        {
            TempData["StockError"] = "กรุณาเลือกหมวดหมู่สินค้าให้ถูกต้อง";
            return RedirectToAction("Stock");
        }

        if (price == null || price < 0)
        {
            TempData["StockError"] = "กรุณากรอกราคาสินค้าให้ถูกต้อง";
            return RedirectToAction("Stock");
        }

        if (stock1 == null || stock1 < 0)
        {
            TempData["StockError"] = "กรุณากรอกจำนวนสต็อกให้ถูกต้อง";
            return RedirectToAction("Stock");
        }

        if (productId > 0)
        {
            var product = Db.Stocks.FirstOrDefault(s => s.ProductId == productId);
            if (product == null)
            {
                TempData["StockError"] = "ไม่พบสินค้าที่ต้องการแก้ไข";
                return RedirectToAction("Stock");
            }

            product.ProductName = productName.Trim();
            product.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            product.Price = price;
            product.Stock1 = stock1;
            product.CategoryId = categoryId;
            Db.SaveChanges();

            TempData["StockSuccess"] = "อัปเดตสินค้าสำเร็จ";
            return RedirectToAction("Stock");
        }

        Db.Stocks.Add(new Stock
        {
            ProductName = productName.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Price = price,
            Stock1 = stock1,
            CategoryId = categoryId
        });
        Db.SaveChanges();

        TempData["StockSuccess"] = "เพิ่มสินค้าใหม่สำเร็จ";
        return RedirectToAction("Stock");
    }
}
