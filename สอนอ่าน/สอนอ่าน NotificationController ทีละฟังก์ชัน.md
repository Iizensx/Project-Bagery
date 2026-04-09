# สอนอ่าน NotificationController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/NotificationController.cs` แบบทีละเมธอด  
`NotificationController` ดูแลงานแจ้งเตือนของผู้ใช้

---

## NotificationController มีหน้าที่อะไร

- ดึงการแจ้งเตือนของผู้ใช้ปัจจุบัน
- กดอ่านทีละรายการ
- กดอ่านทั้งหมด
- ลบการแจ้งเตือนทั้งหมด

---

## 1. Constructor

```csharp
public NotificationController(BakerydbContext db)
{
    _db = db;
}
```

### ฟังก์ชันนี้ทำอะไร

รับฐานข้อมูลเข้ามาเก็บไว้ใน `_db`

---

## 2. ฟังก์ชัน GetMyNotifications()

```csharp
[HttpGet]
public IActionResult GetMyNotifications()
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId <= 0)
        return Unauthorized(new { success = false, notifications = Array.Empty<object>() });

    var notifications = _db.Notifications
        .Where(n => n.UserId == currentUserId)
        .OrderByDescending(n => n.CreatedAt)
        .Take(30)
        .Select(n => new
        {
            id = n.NotificationId,
            type = n.Type,
            title = n.Title,
            message = n.Message,
            href = n.LinkUrl,
            isRead = n.IsRead,
            createdAt = n.CreatedAt
        })
        .ToList();

    return Json(new { success = true, notifications });
}
```

### ฟังก์ชันนี้ทำอะไร

ดึงการแจ้งเตือนล่าสุดของผู้ใช้ที่ login อยู่

### จุดสำคัญ

- เช็ก `UserId` จาก session ก่อน
- ดึงเฉพาะ notification ของผู้ใช้คนนั้น
- จำกัดไว้ 30 รายการล่าสุด
- ส่งกลับเป็น JSON ให้หน้าเว็บเอาไปแสดง

### สรุปสั้น ๆ

`GetMyNotifications()` = โหลดกล่องแจ้งเตือนของ user ปัจจุบัน

---

## 3. ฟังก์ชัน MarkAsRead(int id)

```csharp
[HttpPost]
public IActionResult MarkAsRead(int id)
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId <= 0)
        return Unauthorized(new { success = false });

    var notification = _db.Notifications.FirstOrDefault(n => n.NotificationId == id && n.UserId == currentUserId);
    if (notification == null)
        return Json(new { success = false, message = "Notification not found." });

    notification.IsRead = true;
    _db.SaveChanges();
    return Json(new { success = true });
}
```

### ฟังก์ชันนี้ทำอะไร

ทำเครื่องหมายว่า notification รายการนั้นถูกอ่านแล้ว

### จุดสำคัญ

เช็กทั้ง `NotificationId` และ `UserId` เพื่อกันไม่ให้ไปแก้ของคนอื่น

---

## 4. ฟังก์ชัน MarkAllAsRead()

```csharp
[HttpPost]
public IActionResult MarkAllAsRead()
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId <= 0)
        return Unauthorized(new { success = false });

    var notifications = _db.Notifications
        .Where(n => n.UserId == currentUserId && !n.IsRead)
        .ToList();

    foreach (var notification in notifications)
        notification.IsRead = true;

    _db.SaveChanges();
    return Json(new { success = true });
}
```

### ฟังก์ชันนี้ทำอะไร

ทำเครื่องหมาย notification ที่ยังไม่อ่านทั้งหมดให้เป็นอ่านแล้ว

### สรุปสั้น ๆ

ดึงของ user คนปัจจุบันทั้งหมดที่ `IsRead = false` แล้ววนเปลี่ยนเป็น `true`

---

## 5. ฟังก์ชัน ClearMyNotifications()

```csharp
[HttpPost]
public IActionResult ClearMyNotifications()
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId <= 0)
        return Unauthorized(new { success = false });

    var notifications = _db.Notifications
        .Where(n => n.UserId == currentUserId)
        .ToList();

    if (notifications.Count > 0)
    {
        _db.Notifications.RemoveRange(notifications);
        _db.SaveChanges();
    }

    return Json(new { success = true });
}
```

### ฟังก์ชันนี้ทำอะไร

ลบ notification ทั้งหมดของผู้ใช้ที่ login อยู่

---

## 6. ฟังก์ชัน GetCurrentUserId()

```csharp
private int GetCurrentUserId()
{
    var userIdString = HttpContext.Session.GetString("UserId");
    return int.TryParse(userIdString, out var userId) ? userId : 0;
}
```

### ฟังก์ชันนี้ทำอะไร

อ่าน `UserId` จาก session

---

## สรุปภาพรวมของ NotificationController

- `GetMyNotifications()` ดึงรายการแจ้งเตือน
- `MarkAsRead()` อ่านทีละรายการ
- `MarkAllAsRead()` อ่านทั้งหมด
- `ClearMyNotifications()` ลบทั้งหมด
- `GetCurrentUserId()` เป็น helper กลางของไฟล์นี้
