# สอนอ่าน AdminMemberController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/Admin/AdminMemberController.cs`  
ไฟล์นี้ดูแลงานสมาชิกและบทบาทผู้ใช้

---

## AdminMemberController มีหน้าที่อะไร

- เปิดหน้าจัดการสมาชิก
- เพิ่มผู้ใช้ใหม่
- แก้ไขข้อมูลผู้ใช้เดิม
- ลบผู้ใช้

---

## 1. Constructor

```csharp
public AdminMemberController(BakerydbContext db) : base(db)
{
}
```

### ฟังก์ชันนี้ทำอะไร

รับ `DbContext` ไปให้คลาสแม่ใช้งาน

---

## 2. ฟังก์ชัน Member()

```csharp
public IActionResult Member()
{
    if (!IsCurrentUserAdmin())
        return RedirectToAdminLogin();

    ViewBag.Roles = Db.Roles.OrderBy(r => r.RoleId).ToList();

    var users = Db.Users
        .Include(u => u.Role)
        .OrderBy(u => u.UserId)
        .ToList();

    return View("~/Views/admin/Member.cshtml", users);
}
```

### ฟังก์ชันนี้ทำอะไร

เปิดหน้าจัดการสมาชิก

### จุดสำคัญ

- ใช้ได้เฉพาะ admin
- ดึง `Roles` ไปใส่ใน dropdown
- ดึง `Users` พร้อม role ไปแสดงในตาราง

---

## 3. ฟังก์ชัน SaveMember(...)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult SaveMember(int userId, string username, string? email, string? phone, string? password, int? roleId)
{
    if (!IsCurrentUserAdmin())
        return RedirectToAdminLogin();

    if (userId > 0)
    {
        var user = Db.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            return RedirectToAction("Member");

        user.Username = username.Trim();
        user.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        user.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        user.RoleId = roleId;

        if (!string.IsNullOrWhiteSpace(password))
            user.Password = password.Trim();

        Db.SaveChanges();
        return RedirectToAction("Member");
    }

    Db.Users.Add(new User
    {
        Username = username.Trim(),
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
        Password = password!.Trim(),
        RoleId = roleId
    });
    Db.SaveChanges();

    return RedirectToAction("Member");
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้ทั้งเพิ่มสมาชิกใหม่ และแก้ไขสมาชิกเดิม

### วิธีอ่าน

- `userId > 0` = แก้ไขสมาชิกเดิม
- `userId == 0` = เพิ่มสมาชิกใหม่

### จุดสำคัญ

ถ้าแก้ไขสมาชิกเดิม ระบบจะเปลี่ยนรหัสผ่านก็ต่อเมื่อมีการกรอก `password` ใหม่เข้ามา

---

## 4. ฟังก์ชัน DeleteMember(int userId)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult DeleteMember(int userId)
{
    if (!IsCurrentUserAdmin())
        return RedirectToAdminLogin();

    var currentAdminId = GetCurrentUserId();
    if (currentAdminId == userId)
        return RedirectToAction("Member");

    var user = Db.Users.FirstOrDefault(u => u.UserId == userId);
    if (user == null)
        return RedirectToAction("Member");

    Db.Users.Remove(user);
    Db.SaveChanges();

    return RedirectToAction("Member");
}
```

### ฟังก์ชันนี้ทำอะไร

ลบผู้ใช้ออกจากระบบ

### จุดสำคัญ

```csharp
if (currentAdminId == userId)
```

กันไม่ให้ admin ลบบัญชีของตัวเองตอนกำลังใช้งานอยู่

---

## สรุปภาพรวมของ AdminMemberController

- `Member()` เปิดหน้าจัดการสมาชิก
- `SaveMember()` ใช้ทั้งเพิ่มและแก้ไขผู้ใช้
- `DeleteMember()` ลบผู้ใช้ แต่กันไม่ให้ลบบัญชีตัวเอง
