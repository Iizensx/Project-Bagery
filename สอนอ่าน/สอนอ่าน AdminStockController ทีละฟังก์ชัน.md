# สอนอ่าน AdminStockController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/Admin/AdminStockController.cs`  
ไฟล์นี้ดูแลงานสินค้า สต็อก และหมวดหมู่

---

## AdminStockController มีหน้าที่อะไร

- เปิดหน้าจัดการสินค้า
- ปรับจำนวน stock และสถานะขาย
- เพิ่มหรือแก้ไขหมวดหมู่
- เพิ่มหรือแก้ไขสินค้า
- อัปโหลดรูปสินค้า

---

## 1. Constructor

```csharp
public AdminStockController(BakerydbContext db, IWebHostEnvironment environment) : base(db)
{
    _environment = environment;
}
```

### ฟังก์ชันนี้ทำอะไร

รับ `DbContext` กับ environment สำหรับใช้บันทึกไฟล์รูป

---

## 2. ฟังก์ชัน Stock()

```csharp
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
```

### ฟังก์ชันนี้ทำอะไร

เปิดหน้าจัดการสินค้าและหมวดหมู่

### สรุปสั้น ๆ

ดึงทั้ง `Categories` และ `Products` ไปแสดงในหน้าเดียว

---

## 3. ฟังก์ชัน UpdateStockSettings(...)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateStockSettings(int productId, int stockAdjustment, bool isAvailable, bool isLimitedQuantity)
{
    if (!IsCurrentUserAdminOrStaff())
        return RedirectToAdminLogin();

    var product = await Db.Stocks.FirstOrDefaultAsync(s => s.ProductId == productId);
    if (product == null)
        return RedirectToAction("Stock");

    product.Stock1 = Math.Max(0, (product.Stock1 ?? 0) + stockAdjustment);
    product.IsAvailable = isAvailable;
    product.IsLimitedQuantity = isLimitedQuantity;

    await Db.SaveChangesAsync();
    return RedirectToAction("Stock");
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้ปรับ stock และสถานะขายของสินค้า

### จุดสำคัญ

- admin และ staff ทำได้
- `Math.Max(0, ...)` กัน stock ติดลบ

---

## 4. ฟังก์ชัน SaveCategory(...)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult SaveCategory(int categoryId, string categoryName, string? description)
{
    if (!IsCurrentUserAdmin())
        return RedirectToAdminLogin();

    if (categoryId > 0)
    {
        var category = Db.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
        if (category == null)
            return RedirectToAction("Stock");

        category.CategoryName = categoryName.Trim();
        category.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Db.SaveChanges();
        return RedirectToAction("Stock");
    }

    Db.Categories.Add(new Category
    {
        CategoryName = categoryName.Trim(),
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
    });
    Db.SaveChanges();

    return RedirectToAction("Stock");
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้เพิ่มหรือแก้ไขหมวดหมู่สินค้า

### วิธีอ่าน

- `categoryId > 0` = แก้ไขหมวดหมู่เดิม
- ไม่มี `categoryId` = เพิ่มหมวดหมู่ใหม่

---

## 5. ฟังก์ชัน SaveStock(...)

```csharp
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

    if (productId > 0)
    {
        var product = Db.Stocks.FirstOrDefault(s => s.ProductId == productId);
        if (product == null)
            return RedirectToAction("Stock");

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

    return RedirectToAction("Stock");
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้เพิ่มสินค้าใหม่ หรือแก้ไขสินค้าเดิม

### โครงสร้างความคิด

1. เช็กสิทธิ์ admin
2. ตรวจข้อมูลสินค้า
3. ถ้า `productId > 0` ให้แก้สินค้าเดิม
4. ถ้าไม่ใช่ ให้สร้างสินค้าใหม่
5. ถ้ามีรูป ให้เรียก `SaveProductImageAsync(...)`

---

## 6. ฟังก์ชัน IsSupportedImage(...)

```csharp
private static bool IsSupportedImage(IFormFile imageFile)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

ตรวจว่านามสกุลไฟล์รูปอยู่ในชนิดที่ระบบรองรับหรือไม่

---

## 7. ฟังก์ชัน SaveProductImageAsync(...)

```csharp
private async Task<string> SaveProductImageAsync(IFormFile imageFile)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

บันทึกรูปสินค้าลงโฟลเดอร์ `wwwroot/uploads/products` แล้วคืน path กลับมา

---

## สรุปภาพรวมของ AdminStockController

- `Stock()` เปิดหน้าจัดการสินค้า
- `UpdateStockSettings()` ปรับ stock และสถานะขาย
- `SaveCategory()` เพิ่มหรือแก้ไขหมวดหมู่
- `SaveStock()` เพิ่มหรือแก้ไขสินค้า
- ฟังก์ชัน private ช่วยเช็กรูปและบันทึกรูปสินค้า
