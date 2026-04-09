# สอนอ่าน HomeController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/HomeController.cs` แบบดูทีละเมธอด  
จุดสำคัญของ `HomeController` คือเป็น controller ฝั่งหน้าร้าน ที่ใช้แสดงหน้าแรก เมนู และโปรโมชัน

---

## HomeController มีหน้าที่อะไร

ไฟล์นี้ดูแลหน้าที่ผู้ใช้ทั่วไปเห็นบ่อย ๆ เช่น

- หน้าแรกของร้าน
- หน้าเมนูสินค้า
- หน้าโปรโมชัน
- หน้า error

สรุปง่าย ๆ คือ `HomeController` เน้น "ดึงข้อมูลไปแสดงผล" มากกว่า "บันทึกข้อมูล"

---

## วิธีอ่านไฟล์นี้

เวลาอ่าน `HomeController` ให้โฟกัส 3 อย่าง

1. ฟังก์ชันนี้ดึงข้อมูลจากตารางไหน
2. ส่งข้อมูลให้ `ViewBag` หรือ `Model`
3. ส่งไปหน้า view ไหน

---

## 1. Constructor

```csharp
public HomeController(BakerydbContext db)
{
    _db = db;
}
```

### ฟังก์ชันนี้ทำอะไร

รับ `BakerydbContext` เข้ามาเพื่อให้ controller ใช้ฐานข้อมูลได้

### สรุปสั้น ๆ

`_db` คือทางเชื่อมไปยังตารางต่าง ๆ เช่น `Stocks`, `Promotions`, `Users`

---

## 2. ฟังก์ชัน Home()

```csharp
public IActionResult Home()
{
    ViewBag.PublicPromotions = GetPublicPromotions().Take(3).ToList();
    ViewBag.FeaturedProducts = _db.Stocks
        .Include(s => s.Category)
        .Where(s => s.IsAvailable)
        .OrderBy(s => s.ProductId)
        .Take(8)
        .ToList();
    return View("~/Views/Account/Home.cshtml");
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้เปิดหน้าแรกของร้าน

### อ่านทีละช่วง

```csharp
ViewBag.PublicPromotions = GetPublicPromotions().Take(3).ToList();
```

ดึงโปรโมชันที่เปิดใช้งานอยู่ แล้วเอาแค่ 3 รายการแรกไปแสดงบนหน้าแรก

```csharp
ViewBag.FeaturedProducts = _db.Stocks
    .Include(s => s.Category)
    .Where(s => s.IsAvailable)
    .OrderBy(s => s.ProductId)
    .Take(8)
    .ToList();
```

ดึงสินค้าแนะนำจากตาราง `Stocks`

- `Include(s => s.Category)` ดึงข้อมูลหมวดหมู่มาด้วย
- `Where(s => s.IsAvailable)` เอาเฉพาะสินค้าที่ขายได้
- `Take(8)` แสดงแค่ 8 ชิ้น

```csharp
return View("~/Views/Account/Home.cshtml");
```

ส่งไปแสดงที่หน้า `Home.cshtml`

### สรุปสั้น ๆ

`Home()` = ดึงโปรโมชันกับสินค้าแนะนำไปโชว์บนหน้าแรก

---

## 3. ฟังก์ชัน Menu()

```csharp
public IActionResult Menu()
{
    var products = _db.Stocks
        .Include(s => s.Category)
        .Where(s => s.IsAvailable)
        .OrderBy(s => s.Price ?? decimal.MaxValue)
        .ThenBy(s => s.ProductName)
        .ToList();

    return View("~/Views/Account/Menu.cshtml", products);
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้เปิดหน้าเมนูสินค้า

### อ่านทีละช่วง

```csharp
var products = _db.Stocks
```

เริ่มดึงข้อมูลสินค้าจากตาราง `Stocks`

```csharp
.Include(s => s.Category)
```

ดึงข้อมูลหมวดหมู่ของสินค้าไปพร้อมกัน

```csharp
.Where(s => s.IsAvailable)
```

เอาเฉพาะสินค้าที่เปิดขายอยู่

```csharp
.OrderBy(s => s.Price ?? decimal.MaxValue)
.ThenBy(s => s.ProductName)
```

เรียงสินค้าตามราคา แล้วเรียงตามชื่ออีกชั้นหนึ่ง

```csharp
return View("~/Views/Account/Menu.cshtml", products);
```

ส่งรายการสินค้าไปให้หน้า `Menu.cshtml`

### สรุปสั้น ๆ

`Menu()` = ดึงรายการสินค้าที่ขายได้ทั้งหมด แล้วส่งไปแสดงหน้าเมนู

---

## 4. ฟังก์ชัน Promotion()

```csharp
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
```

### ฟังก์ชันนี้ทำอะไร

ใช้เปิดหน้าโปรโมชันของร้าน

### โครงสร้างความคิด

1. ดึงโปรโมชันที่เปิดใช้งาน
2. หา `RewardProductId` ของโปรโมชันที่มีของแถม
3. ดึงชื่อสินค้าที่เป็นของรางวัล
4. รวมข้อมูลใส่ `ViewModel`
5. ส่งไปที่หน้า `Promotion.cshtml`

### จุดสำคัญ

```csharp
var promotions = GetPublicPromotions();
```

เรียกเมธอดช่วยเพื่อดึงเฉพาะโปรโมชันที่ใช้ได้จริง

```csharp
EventPromotions = promotions.Where(p => p.PromoType == 3).ToList()
```

แยกโปรโมชันประเภท event ออกมาอีกชุด

```csharp
RewardProductNames = rewardMap
```

เก็บชื่อสินค้าของรางวัลไว้ใน model เพื่อให้หน้า view เอาไปแสดงได้ง่าย

### สรุปสั้น ๆ

`Promotion()` = รวมข้อมูลโปรโมชันและข้อมูลของรางวัล แล้วส่งไปหน้าโปรโมชัน

---

## 5. ฟังก์ชัน Contact()

```csharp
public IActionResult Contact()
{
    return RedirectToAction("Home");
}
```

### ฟังก์ชันนี้ทำอะไร

ไม่ได้มีหน้า contact แยกจริง  
ถ้าเข้ามาจะ redirect กลับหน้า `Home`

---

## 6. ฟังก์ชัน lab8()

```csharp
public IActionResult lab8()
{
    var users = (from u in _db.Users select u).ToList();
    return View(users);
}
```

### ฟังก์ชันนี้ทำอะไร

ดึงข้อมูลผู้ใช้ทั้งหมดแล้วส่งไปหน้า view

### จุดที่ควรสังเกต

```csharp
from u in _db.Users select u
```

เป็นการเขียน query แบบ LINQ syntax

---

## 7. ฟังก์ชัน Privacy()

```csharp
public IActionResult Privacy()
{
    return View();
}
```

### ฟังก์ชันนี้ทำอะไร

เปิดหน้า `Privacy`

---

## 8. ฟังก์ชัน Error()

```csharp
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public IActionResult Error()
{
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้เปิดหน้า error เมื่อระบบเกิดปัญหา

### จุดสำคัญ

```csharp
new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
```

ส่ง `RequestId` ไปให้หน้า error ใช้อ้างอิง

---

## 9. ฟังก์ชัน GetPublicPromotions()

```csharp
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
```

### ฟังก์ชันนี้ทำอะไร

เป็นเมธอดช่วยภายในสำหรับดึงโปรโมชันที่แสดงสู่สาธารณะได้

### สรุปสั้น ๆ

โปรโมชันต้องเปิดใช้งาน มีรูป และอยู่ในช่วงเวลาใช้งาน

---

## สรุปภาพรวมของ HomeController

- `Home()` ดึงโปรโมชันและสินค้าแนะนำ
- `Menu()` ดึงสินค้าที่ขายได้ทั้งหมด
- `Promotion()` รวมข้อมูลโปรโมชันและของรางวัล
- `GetPublicPromotions()` เป็น helper กลางของหน้าโปรโมชัน
