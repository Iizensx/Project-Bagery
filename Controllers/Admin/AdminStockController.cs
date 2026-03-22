using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

// Controller สำหรับจัดการสินค้าและหมวดหมู่
// ดูข้อมูลสต็อก เพิ่มหมวดหมู่ใหม่ แก้ไขสินค้าเดิม หรือเพิ่มสินค้าใหม่
public class AdminStockController : AdminControllerBase
{
    // รับ BakerydbContext จากคลาสแม่
    public AdminStockController(BakerydbContext db) : base(db)
    {
    }

    // GET: แสดงหน้าจัดการสต็อก พร้อมข้อมูลสินค้าและหมวดหมู่ทั้งหมด
    public IActionResult Stock()
    {
        // จำกัดสิทธิ์เฉพาะ Admin
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // สร้าง ViewModel เพื่อส่งทั้งหมวดหมู่และสินค้าไปที่หน้า Stock
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

    // POST: เพิ่มหมวดหมู่ใหม่ หรือแก้ไขหมวดหมู่เดิม
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveCategory(int categoryId, string categoryName, string? description)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ชื่อหมวดหมู่ห้ามว่าง
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            TempData["StockError"] = "กรุณากรอกชื่อหมวดหมู่";
            return RedirectToAction("Stock");
        }

        // ถ้ามี categoryId แสดงว่าเป็นการแก้ไขหมวดหมู่เดิม
        if (categoryId > 0)
        {
            var category = Db.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                TempData["StockError"] = "ไม่พบหมวดหมู่ที่ต้องการแก้ไข";
                return RedirectToAction("Stock");
            }

            // อัปเดตข้อมูลหมวดหมู่แล้วบันทึก
            category.CategoryName = categoryName.Trim();
            category.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Db.SaveChanges();

            TempData["StockSuccess"] = "อัปเดตหมวดหมู่เรียบร้อย";
            return RedirectToAction("Stock");
        }

        // ถ้าไม่มี categoryId ให้สร้างหมวดหมู่ใหม่
        Db.Categories.Add(new Category
        {
            CategoryName = categoryName.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        });
        Db.SaveChanges();

        TempData["StockSuccess"] = "เพิ่มหมวดหมู่ใหม่เรียบร้อย";
        return RedirectToAction("Stock");
    }

    // POST: เพิ่มสินค้าใหม่ หรือแก้ไขสินค้าเดิม
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveStock(int productId, string productName, string? description, decimal? price, int? stock1, int? categoryId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ตรวจสอบข้อมูลสำคัญก่อนบันทึก
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

        // ถ้ามี productId แสดงว่าเป็นการแก้ไขสินค้าเดิม
        if (productId > 0)
        {
            var product = Db.Stocks.FirstOrDefault(s => s.ProductId == productId);
            if (product == null)
            {
                TempData["StockError"] = "ไม่พบสินค้าที่ต้องการแก้ไข";
                return RedirectToAction("Stock");
            }

            // อัปเดตข้อมูลสินค้าแล้วบันทึกลงฐานข้อมูล
            product.ProductName = productName.Trim();
            product.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            product.Price = price;
            product.Stock1 = stock1;
            product.CategoryId = categoryId;
            Db.SaveChanges();

            TempData["StockSuccess"] = "อัปเดตสินค้าสำเร็จ";
            return RedirectToAction("Stock");
        }

        // ถ้าไม่มี productId ให้สร้างสินค้าใหม่
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
