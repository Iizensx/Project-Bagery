# สอนอ่าน AdminControllerBase ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/Admin/AdminControllerBase.cs`  
ไฟล์นี้ไม่ได้เป็นหน้าใช้งานตรง ๆ แต่เป็น "คลาสแม่" ของ controller ฝั่ง admin/staff

---

## AdminControllerBase มีหน้าที่อะไร

- เก็บ `DbContext` ไว้ให้คลาสลูกใช้
- อ่าน `UserId` จาก session
- เช็กสิทธิ์ว่าเป็น admin หรือ staff
- ช่วย redirect ไปหน้าที่เหมาะสมเมื่อสิทธิ์ไม่ผ่าน

---

## 1. การประกาศคลาส

```csharp
public abstract class AdminControllerBase : Controller
```

### ตรงนี้แปลว่าอะไร

- `abstract` = เป็นคลาสแม่ ใช้ให้คลาสอื่นสืบทอด
- `: Controller` = ใช้ความสามารถของ ASP.NET MVC controller

---

## 2. ตัวแปรระดับคลาส

```csharp
protected readonly BakerydbContext Db;
protected const int AdminRoleId = 1;
protected const int StaffRoleId = 2;
```

### ตรงนี้ทำอะไร

- `Db` ใช้คุยกับฐานข้อมูล
- `AdminRoleId = 1` ใช้แทน role ของ admin
- `StaffRoleId = 2` ใช้แทน role ของ staff

---

## 3. Constructor

```csharp
protected AdminControllerBase(BakerydbContext db)
{
    Db = db;
}
```

### ฟังก์ชันนี้ทำอะไร

รับฐานข้อมูลเข้ามา แล้วเก็บไว้ใน `Db`

---

## 4. ฟังก์ชัน IsCurrentUserAdmin()

```csharp
protected bool IsCurrentUserAdmin()
{
    var userId = GetCurrentUserId();
    if (userId <= 0)
        return false;

    return Db.Users.Any(u => u.UserId == userId && u.RoleId == AdminRoleId);
}
```

### ฟังก์ชันนี้ทำอะไร

เช็กว่าคนที่ login อยู่เป็น admin หรือไม่

### วิธีคิด

1. อ่าน `UserId` จาก session
2. ถ้าไม่มี user ให้ `false`
3. ไปเช็กในตาราง `Users` ว่า role เป็น admin ไหม

---

## 5. ฟังก์ชัน IsCurrentUserStaff()

```csharp
protected bool IsCurrentUserStaff()
{
    var userId = GetCurrentUserId();
    if (userId <= 0)
        return false;

    return Db.Users.Any(u => u.UserId == userId && u.RoleId == StaffRoleId);
}
```

### ฟังก์ชันนี้ทำอะไร

เช็กว่าผู้ใช้ปัจจุบันเป็น staff หรือไม่

---

## 6. ฟังก์ชัน IsCurrentUserAdminOrStaff()

```csharp
protected bool IsCurrentUserAdminOrStaff()
{
    var userId = GetCurrentUserId();
    if (userId <= 0)
        return false;

    return Db.Users.Any(u => u.UserId == userId && (u.RoleId == AdminRoleId || u.RoleId == StaffRoleId));
}
```

### ฟังก์ชันนี้ทำอะไร

เช็กว่าคนปัจจุบันเป็น admin หรือ staff อย่างใดอย่างหนึ่ง

### ใช้เมื่อไหร่

ใช้กับหน้าที่ admin และ staff เข้าร่วมกันได้ เช่น order หรือ stock บางส่วน

---

## 7. ฟังก์ชัน GetCurrentUserId()

```csharp
protected int GetCurrentUserId()
{
    var userIdString = HttpContext.Session.GetString("UserId");
    return int.TryParse(userIdString, out var userId) ? userId : 0;
}
```

### ฟังก์ชันนี้ทำอะไร

ดึง `UserId` จาก session แล้วแปลงเป็นเลข

### สรุปสั้น ๆ

ถ้าอ่านไม่ได้หรือไม่มีค่า จะคืน `0`

---

## 8. ฟังก์ชัน RedirectToAdminLogin()

```csharp
protected IActionResult RedirectToAdminLogin()
{
    if (IsCurrentUserStaff())
        return RedirectToAction("Order", "AdminOrder", new { area = "" });

    if (IsCurrentUserAdmin())
        return RedirectToAction("Dashbordadmin", "AdminDashboard", new { area = "" });

    return RedirectToAction("Login", "Account", new { area = "" });
}
```

### ฟังก์ชันนี้ทำอะไร

ช่วย redirect ผู้ใช้ไปหน้าที่เหมาะสมตามสิทธิ์

### วิธีคิด

- ถ้าเป็น staff ให้ไปหน้า `AdminOrder`
- ถ้าเป็น admin ให้ไปหน้า dashboard
- ถ้าไม่ใช่ทั้งคู่ ให้กลับหน้า login

---

## สรุปภาพรวมของ AdminControllerBase

- เป็นคลาสแม่ของ controller ฝั่งหลังบ้าน
- ทำให้ controller ลูกใช้ `Db` ได้
- มีฟังก์ชันเช็กสิทธิ์ซ้ำ ๆ ให้พร้อมใช้
- ลดการเขียนโค้ดซ้ำในไฟล์ admin อื่น ๆ
