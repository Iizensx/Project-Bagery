# สอนอ่าน AdminOrderController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/Admin/AdminOrderController.cs`  
ไฟล์นี้เป็น controller หลักของฝั่งหลังบ้านที่ดูแลงานออเดอร์

---

## AdminOrderController มีหน้าที่อะไร

- แสดงรายการออเดอร์ทั้งหมด
- ตรวจรายละเอียดออเดอร์
- ยืนยันการชำระเงิน
- รับออเดอร์เข้าครัวหรือเตรียมสินค้า
- เปลี่ยนสถานะเป็นจัดส่ง

---

## 1. Constructor

```csharp
public AdminOrderController(BakerydbContext db, ILogger<AdminOrderController> logger) : base(db)
{
    _logger = logger;
}
```

### ฟังก์ชันนี้ทำอะไร

รับ `DbContext` ไปให้คลาสแม่ และเก็บ `logger` ไว้ใช้เขียน log

---

## 2. ฟังก์ชัน Order()

```csharp
public IActionResult Order()
{
    if (!IsCurrentUserAdminOrStaff())
        return RedirectToAdminLogin();

    var orders = Db.Orders
        .Include(o => o.User)
        .Include(o => o.Promotion)
        .Include(o => o.Orderdetails)
            .ThenInclude(d => d.Product)
        .OrderByDescending(o => o.OrderDate)
        .ToList();

    return View("~/Views/admin/Order.cshtml", orders);
}
```

### ฟังก์ชันนี้ทำอะไร

เปิดหน้ารายการออเดอร์ทั้งหมดให้ admin หรือ staff ดู

### จุดสำคัญ

- เช็กสิทธิ์ด้วย `IsCurrentUserAdminOrStaff()`
- ดึงข้อมูล user, promotion, และรายการสินค้าไปพร้อมกัน

---

## 3. ฟังก์ชัน ConfirmPayment(int orderId)

```csharp
[HttpPost]
public IActionResult ConfirmPayment(int orderId)
{
    if (!IsCurrentUserAdminOrStaff())
        return RedirectToAdminLogin();

    try
    {
        var order = Db.Orders
            .Include(o => o.Orderdetails)
            .ThenInclude(d => d.Product)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null)
            return Json(new { success = false, message = "ไม่พบ Order" });

        foreach (var detail in order.Orderdetails)
        {
            var product = detail.Product;
            if (product != null)
                product.Stock1 = Math.Max(0, (product.Stock1 ?? 0) - (detail.Quantity ?? 0));
        }

        order.PaymentStatus = "Paid";
        order.Status = "Paid";
        Db.Update(order);
        Db.SaveChanges();

        return Json(new { success = true, message = "ยืนยันการชำระเงินเรียบร้อย" });
    }
    catch (Exception ex)
    {
        _logger.LogError("Error confirming payment for order {OrderId}: {Message}", orderId, ex.Message);
        return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
    }
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้ตอนแอดมินหรือสตาฟตรวจสลิปแล้วกดยืนยันการชำระเงิน

### โครงสร้างความคิด

1. เช็กสิทธิ์
2. หา order
3. ลด stock ตามจำนวนที่ลูกค้าสั่ง
4. เปลี่ยนสถานะการชำระเงินเป็น `Paid`
5. บันทึกลงฐานข้อมูล

### จุดสำคัญ

การตัด stock เกิดในฟังก์ชันนี้ ไม่ได้ตัดตั้งแต่ `CreateOrder()`

---

## 4. ฟังก์ชัน GetOrderDetails(int orderId)

```csharp
[HttpGet]
public IActionResult GetOrderDetails(int orderId)
{
    if (!IsCurrentUserAdminOrStaff())
        return RedirectToAdminLogin();

    var order = Db.Orders
        .Include(o => o.User)
        .Include(o => o.Promotion)
        .Include(o => o.Orderdetails)
            .ThenInclude(d => d.Product)
        .FirstOrDefault(o => o.OrderId == orderId);

    if (order == null)
        return Json(new { success = false, message = "ไม่พบออเดอร์" });

    return Json(new { success = true, orderId = order.OrderId, customerName = order.User?.Username });
}
```

### ฟังก์ชันนี้ทำอะไร

ดึงรายละเอียดออเดอร์กลับไปเป็น JSON  
มักใช้เปิด modal หรือดูรายละเอียดออเดอร์แบบไม่เปลี่ยนหน้า

---

## 5. ฟังก์ชัน BuildPromotionLabel(Promotion? promotion)

```csharp
private static string? BuildPromotionLabel(Promotion? promotion)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

แปลงข้อมูลโปรโมชันเป็นข้อความที่อ่านง่าย เช่น

- ลด 10%
- ลด 50 บาท
- ซื้อ 2 แถม 1

---

## 6. ฟังก์ชัน AcceptOrder(int orderId)

```csharp
[HttpPost]
public IActionResult AcceptOrder(int orderId)
{
    if (!IsCurrentUserAdminOrStaff())
        return RedirectToAdminLogin();

    try
    {
        var order = Db.Orders
            .Include(o => o.Orderdetails)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null)
            return Json(new { success = false, message = "ไม่พบ Order" });

        if (order.Status == "Preparing" || order.Status == "Shipped")
            return Json(new { success = false, message = "ออเดอร์นี้ประมวลผลแล้ว" });

        order.Status = "Preparing";
        Db.Update(order);
        Db.SaveChanges();

        return Json(new { success = true, message = "ยอมรับออเดอร์เรียบร้อย" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
    }
}
```

### ฟังก์ชันนี้ทำอะไร

เปลี่ยนสถานะออเดอร์เป็น `Preparing`

### สรุปสั้น ๆ

ใช้เมื่อร้านเริ่มรับงานและเตรียมสินค้า

---

## 7. ฟังก์ชัน ShipOrder(int orderId)

```csharp
[HttpPost]
public IActionResult ShipOrder(int orderId)
{
    if (!IsCurrentUserAdminOrStaff())
        return RedirectToAdminLogin();

    try
    {
        var order = Db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
            return Json(new { success = false, message = "ไม่พบ Order" });

        if (order.Status != "Preparing")
            return Json(new { success = false, message = "ออเดอร์ต้องอยู่ในสถานะ Preparing เท่านั้น" });

        order.Status = "Shipped";
        Db.Update(order);
        Db.SaveChanges();

        return Json(new { success = true, message = "จัดส่งสินค้าเรียบร้อย" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
    }
}
```

### ฟังก์ชันนี้ทำอะไร

เปลี่ยนสถานะจาก `Preparing` ไปเป็น `Shipped`

---

## สรุปภาพรวมของ AdminOrderController

- `Order()` เปิดหน้ารายการออเดอร์
- `ConfirmPayment()` ยืนยันสลิปและตัด stock
- `GetOrderDetails()` ส่งรายละเอียดออเดอร์เป็น JSON
- `AcceptOrder()` เปลี่ยนสถานะเป็น `Preparing`
- `ShipOrder()` เปลี่ยนสถานะเป็น `Shipped`
