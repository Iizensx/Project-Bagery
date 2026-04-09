# สอนอ่าน DeliveryController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/DeliveryController.cs` แบบทีละเมธอด  
`DeliveryController` ดูแลหน้า "ติดตามออเดอร์" และ "ยืนยันรับสินค้า"

---

## DeliveryController มีหน้าที่อะไร

- แสดงสถานะออเดอร์ปัจจุบัน
- แสดงประวัติออเดอร์ที่เสร็จแล้ว
- ให้ผู้ใช้กดยืนยันรับสินค้า
- แปลงข้อมูล order ให้อยู่ในรูปที่หน้า view ใช้งานง่าย

---

## 1. Constructor

```csharp
public DeliveryController(BakerydbContext db, ILogger<DeliveryController> logger)
{
    _db = db;
    _logger = logger;
}
```

### ฟังก์ชันนี้ทำอะไร

รับฐานข้อมูลกับ logger มาเก็บไว้ใช้ทั้งไฟล์

---

## 2. ฟังก์ชัน Delivery()

```csharp
public IActionResult Delivery()
{
    var userIdString = HttpContext.Session.GetString("UserId");
    if (!int.TryParse(userIdString, out var userId) || userId <= 0)
        return View("~/Views/Account/Delivery.cshtml", new DeliveryTrackingViewModel());

    var orderBaseQuery = _db.Orders
        .AsNoTracking()
        .Where(o => o.UserId == userId && o.Status != "Cancelled");

    var activeOrder = orderBaseQuery
        .Where(o => o.Status != "Completed")
        .OrderByDescending(o => o.OrderDate)
        .ThenByDescending(o => o.OrderId)
        .Include(o => o.Address)
        .Include(o => o.Promotion)
        .Include(o => o.Orderdetails)
            .ThenInclude(d => d.Product)
        .AsSplitQuery()
        .FirstOrDefault();

    var completedOrders = orderBaseQuery
        .Where(o => o.Status == "Completed")
        .OrderByDescending(o => o.OrderDate)
        .ThenByDescending(o => o.OrderId)
        .Include(o => o.Address)
        .Include(o => o.Promotion)
        .Include(o => o.Orderdetails)
            .ThenInclude(d => d.Product)
        .AsSplitQuery()
        .ToList();

    var historyOrders = _db.Historyorders
        .AsNoTracking()
        .Where(h => h.UserId == userId)
        .OrderByDescending(h => h.CompletedAt)
        .ThenByDescending(h => h.HistoryOrderId)
        .ToList();

    return View("~/Views/Account/Delivery.cshtml", BuildDeliveryTrackingViewModel(activeOrder, historyOrders, completedOrders));
}
```

### ฟังก์ชันนี้ทำอะไร

เปิดหน้าติดตามออเดอร์ของผู้ใช้

### โครงสร้างความคิด

1. อ่าน `UserId` จาก session
2. ถ้ายังไม่ login ให้เปิดหน้าเปล่า
3. ดึงออเดอร์ที่กำลังดำเนินอยู่
4. ดึงออเดอร์ที่เสร็จแล้ว
5. ดึงประวัติจาก `Historyorders`
6. รวมทั้งหมดผ่าน `BuildDeliveryTrackingViewModel(...)`

### จุดสำคัญ

- `AsNoTracking()` ใช้ตอนอ่านอย่างเดียว ไม่ได้แก้ข้อมูล
- `Include(...)` ใช้ดึงข้อมูลที่เกี่ยวข้อง เช่น ที่อยู่ โปรโมชัน และรายการสินค้า
- `BuildDeliveryTrackingViewModel(...)` คือหัวใจของการแปลงข้อมูลก่อนส่งไปหน้า view

### สรุปสั้น ๆ

`Delivery()` = ดึงข้อมูลออเดอร์ทั้งหมดของผู้ใช้ แล้วจัดรูปแบบเพื่อแสดงหน้า tracking

---

## 3. ฟังก์ชัน CompleteOrder(int orderId)

```csharp
[HttpPost]
public IActionResult CompleteOrder(int orderId)
{
    var userIdString = HttpContext.Session.GetString("UserId");
    if (!int.TryParse(userIdString, out var userId) || userId <= 0)
        return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อน" });

    try
    {
        var order = _db.Orders
            .Include(o => o.Address)
            .Include(o => o.Promotion)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

        if (order == null)
            return Json(new { success = false, message = "ไม่พบออเดอร์" });

        if (order.Status != "Shipped")
            return Json(new { success = false, message = "ออเดอร์นี้ยังไม่อยู่ในสถานะจัดส่ง" });

        var existingHistory = _db.Historyorders.FirstOrDefault(h => h.OrderId == order.OrderId);
        if (existingHistory == null)
        {
            _db.Historyorders.Add(new Historyorder
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                CompletedAt = DateTime.Now,
                TotalAmount = order.TotalAmount,
                Status = "Completed",
                PaymentStatus = order.PaymentStatus
            });
        }

        order.Status = "Completed";
        _db.Update(order);
        _db.SaveChanges();

        return Json(new { success = true, message = "ปิดออเดอร์เรียบร้อย" });
    }
    catch (Exception ex)
    {
        _logger.LogError("Error completing order {OrderId}: {Message}", orderId, ex.Message);
        return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
    }
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้ตอนผู้ใช้กดยืนยันว่าได้รับสินค้าแล้ว

### โครงสร้างความคิด

1. เช็ก login
2. หา order ของ user คนนี้
3. อนุญาตให้ปิดออเดอร์ได้เฉพาะสถานะ `Shipped`
4. ถ้ายังไม่มีประวัติใน `Historyorders` ให้สร้างใหม่
5. เปลี่ยนสถานะ order เป็น `Completed`

### สรุปสั้น ๆ

`CompleteOrder()` = ยืนยันรับสินค้า แล้วย้ายข้อมูลไปอยู่ฝั่งประวัติ

---

## 4. ฟังก์ชัน BuildDeliveryTrackingViewModel(...)

```csharp
private DeliveryTrackingViewModel BuildDeliveryTrackingViewModel(Order? order, List<Historyorder> historyOrders, List<Order>? completedOrders = null)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

เป็นเมธอดช่วยตัวใหญ่ของไฟล์นี้  
หน้าที่คือแปลงข้อมูลจากหลายตารางให้กลายเป็น `DeliveryTrackingViewModel`

### แนวคิดเวลาอ่าน

ให้แบ่งอ่านเป็น 3 ส่วน

1. แปลง `Historyorders` ให้เป็นรายการประวัติที่หน้า view ใช้งานได้
2. ถ้ามี `completedOrders` ให้เอาข้อมูลจริงมาเติมรายละเอียดสินค้าและโปรโมชัน
3. ถ้ามี `active order` ให้คำนวณสถานะปัจจุบัน เช่น `Pending`, `Paid`, `Preparing`, `Shipped`, `Completed`

### จุดสำคัญ

- คำนวณ `trackingStage`
- สร้าง `statusTitle`, `statusMessage`, `etaText`
- รวมที่อยู่ รายการสินค้า และประวัติออเดอร์ไว้ใน model เดียว

### สรุปสั้น ๆ

ฟังก์ชันนี้คือ "ตัวแปลข้อมูล" ก่อนส่งให้หน้า `Delivery.cshtml`

---

## 5. ฟังก์ชัน BuildDeliveryAddress(Order order)

```csharp
private string BuildDeliveryAddress(Order order)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

รวมที่อยู่จากหลายช่องให้กลายเป็นข้อความเดียว

---

## 6. ฟังก์ชัน BuildItemSummary(Order order)

```csharp
private string BuildItemSummary(Order order)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

รวมชื่อสินค้าและจำนวนเป็นข้อความสรุป เช่น `เค้ก x2`

---

## 7. ฟังก์ชัน BuildHistoryLineItems(Order order)

```csharp
private List<DeliveryOrderHistoryLineItem> BuildHistoryLineItems(Order order)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

แปลง `Orderdetails` ให้เป็นรายการสินค้าสำหรับประวัติออเดอร์

---

## 8. ฟังก์ชัน ParseHistoryLineItems(string? itemSummary)

```csharp
private List<DeliveryOrderHistoryLineItem> ParseHistoryLineItems(string? itemSummary)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

แปลงข้อความสรุปรายการสินค้าให้กลับมาเป็น list ของสินค้าอีกครั้ง

### จุดสำคัญ

ใช้ `Regex` เพื่อจับรูปแบบเช่น `ชื่อสินค้า x2`

---

## 9. ฟังก์ชัน BuildDiscountDisplay(Promotion? promotion)

```csharp
private string BuildDiscountDisplay(Promotion? promotion)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

แปลงข้อมูลโปรโมชันให้เป็นข้อความที่อ่านง่าย เช่น

- ลด 10%
- ซื้อ 2 แถม 1
- รับของแถมตามกิจกรรม

---

## สรุปภาพรวมของ DeliveryController

- `Delivery()` ดึงข้อมูลออเดอร์ปัจจุบันและประวัติ
- `CompleteOrder()` ใช้ยืนยันรับสินค้า
- ฟังก์ชัน private หลายตัวมีหน้าที่จัดรูปแบบข้อมูลก่อนส่งให้หน้า view
