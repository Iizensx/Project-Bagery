# สอนอ่าน AdminDashboardController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/Admin/AdminDashboardController.cs`  
ไฟล์นี้เป็นหน้าสรุปภาพรวมของระบบฝั่ง admin

---

## AdminDashboardController มีหน้าที่อะไร

- แสดง dashboard ของแอดมิน
- คำนวณ KPI สำคัญของระบบ
- สรุปรายได้ ออเดอร์ สต็อก และหมวดหมู่

---

## 1. Constructor

```csharp
public AdminDashboardController(BakerydbContext db) : base(db)
{
}
```

### ฟังก์ชันนี้ทำอะไร

รับ `DbContext` ไปให้คลาสแม่

---

## 2. ฟังก์ชัน Dashbordadmin()

```csharp
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
        MonthlyRevenue = ...,
        RecentOrders = ...,
        LowStockItems = ...,
        CategorySummaries = ...
    };

    return View("~/Views/admin/Dashbordadmin.cshtml", model);
}
```

### ฟังก์ชันนี้ทำอะไร

ดึงข้อมูลจำนวนมากจากระบบ แล้วสรุปเป็นตัวเลขและรายการสำคัญสำหรับหน้า dashboard

### โครงสร้างความคิด

1. เช็กสิทธิ์ admin
2. กำหนดช่วงเวลาที่ใช้คำนวณ เช่น วันนี้ และต้นเดือน
3. ดึงออเดอร์ทั้งหมด
4. ดึงสินค้าทั้งหมด
5. สร้าง `AdminDashboardViewModel`
6. ส่ง model ไปหน้า dashboard

### จุดสำคัญ

```csharp
var paidStatuses = new[] { "Paid", "Preparing", "Shipped", "Completed" };
```

ใช้กำหนดว่า status ไหนนับเป็นรายได้แล้ว

```csharp
TodayRevenue = ...
TodayOrders = ...
PendingVerificationOrders = ...
```

เป็น KPI หลักของ dashboard

```csharp
MonthlyRevenue = ...
```

ใช้สรุปรายได้ย้อนหลัง 6 เดือน

```csharp
RecentOrders = ...
LowStockItems = ...
CategorySummaries = ...
```

ใช้แสดง widget หรือ section ย่อยบน dashboard

### สรุปสั้น ๆ

`Dashbordadmin()` = รวบรวมข้อมูลจากหลายตาราง แล้วทำให้เป็น dashboard ที่อ่านง่าย

---

## สรุปภาพรวมของ AdminDashboardController

- มี action หลักจริง ๆ คือ `Dashbordadmin()`
- จุดสำคัญไม่ใช่การบันทึกข้อมูล แต่เป็นการคำนวณและสรุปข้อมูล
- ถ้าอ่านไฟล์นี้เข้าใจ จะเห็นภาพรวมธุรกิจของระบบทั้งหมด
