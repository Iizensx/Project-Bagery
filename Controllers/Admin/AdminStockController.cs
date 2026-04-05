using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

public class AdminStockController : AdminControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public AdminStockController(BakerydbContext db, IWebHostEnvironment environment) : base(db)
    {
        _environment = environment;
    }

    public IActionResult Stock()
    {
        if (!IsCurrentUserAdminOrStaff())
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
    public async Task<IActionResult> UpdateStockSettings(
        int productId,
        int stockAdjustment,
        bool isAvailable,
        bool isLimitedQuantity)
    {
        if (!IsCurrentUserAdminOrStaff())
            return RedirectToAdminLogin();

        var product = await Db.Stocks.FirstOrDefaultAsync(s => s.ProductId == productId);
        if (product == null)
        {
            TempData["StockError"] = "ไม่พบสินค้าที่ต้องการแก้ไข";
            return RedirectToAction("Stock");
        }

        product.Stock1 = Math.Max(0, (product.Stock1 ?? 0) + stockAdjustment);
        product.IsAvailable = isAvailable;
        product.IsLimitedQuantity = isLimitedQuantity;

        await Db.SaveChangesAsync();

        TempData["StockSuccess"] = "อัปเดตสต็อกและสถานะสินค้าเรียบร้อย";
        return RedirectToAction("Stock");
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
    public async Task<IActionResult> SaveStock(
        int productId,
        string productName,
        string? description,
        decimal? price,
        int? stock1,
        bool isAvailable,
        bool isLimitedQuantity,
        int? categoryId,
        IFormFile? imageFile)
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

        if (imageFile != null && imageFile.Length > 0 && !IsSupportedImage(imageFile))
        {
            TempData["StockError"] = "รองรับเฉพาะไฟล์รูปภาพ .jpg, .jpeg, .png, .webp และ .gif";
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
            product.IsAvailable = isAvailable;
            product.IsLimitedQuantity = isLimitedQuantity;
            product.CategoryId = categoryId;

            if (imageFile != null && imageFile.Length > 0)
                product.ImageUrl = await SaveProductImageAsync(imageFile);

            await Db.SaveChangesAsync();

            TempData["StockSuccess"] = "อัปเดตสินค้าสำเร็จ";
            return RedirectToAction("Stock");
        }

        var newProduct = new Stock
        {
            ProductName = productName.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Price = price,
            Stock1 = stock1,
            IsAvailable = isAvailable,
            IsLimitedQuantity = isLimitedQuantity,
            CategoryId = categoryId
        };

        if (imageFile != null && imageFile.Length > 0)
            newProduct.ImageUrl = await SaveProductImageAsync(imageFile);

        Db.Stocks.Add(newProduct);
        await Db.SaveChangesAsync();

        TempData["StockSuccess"] = "เพิ่มสินค้าใหม่สำเร็จ";
        return RedirectToAction("Stock");
    }

    private static bool IsSupportedImage(IFormFile imageFile)
    {
        var extension = Path.GetExtension(imageFile.FileName);
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        return !string.IsNullOrWhiteSpace(extension) &&
               allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<string> SaveProductImageAsync(IFormFile imageFile)
    {
        var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"product_{Guid.NewGuid():N}{Path.GetExtension(imageFile.FileName)}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await imageFile.CopyToAsync(stream);

        return "/uploads/products/" + fileName;
    }
}
