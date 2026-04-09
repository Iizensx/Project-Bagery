# สอนอ่าน ProfileController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/ProfileController.cs` แบบทีละเมธอด  
`ProfileController` เน้นจัดการข้อมูลส่วนตัวและที่อยู่ของผู้ใช้

---

## ProfileController มีหน้าที่อะไร

ไฟล์นี้ดูแลเรื่อง

- เปิดหน้าโปรไฟล์
- แก้ไขข้อมูลผู้ใช้
- เพิ่มหรือแก้ไขที่อยู่
- ลบที่อยู่
- ดึงรายการที่อยู่ของผู้ใช้

สรุปง่าย ๆ คือเป็น controller ที่ดูแล "ข้อมูลส่วนตัวของสมาชิก"

---

## วิธีอ่านไฟล์นี้

เวลาอ่าน `ProfileController` ให้ดู 3 อย่าง

1. ใช้ `Session` ตรงไหนเพื่อรู้ว่าใครเป็นผู้ใช้ปัจจุบัน
2. แก้ข้อมูลลงตาราง `Users` หรือ `Addresses` ตรงไหน
3. คืนค่าเป็น `View` หรือ `Json`

---

## 1. Constructor

```csharp
public ProfileController(BakerydbContext db)
{
    _db = db;
}
```

### ฟังก์ชันนี้ทำอะไร

รับฐานข้อมูลเข้ามาเก็บไว้ใน `_db`

---

## 2. ฟังก์ชัน Profile(int id = 0) แบบ GET

```csharp
public IActionResult Profile(int id = 0)
{
    if (id <= 0)
    {
        id = int.TryParse(HttpContext.Session.GetString("UserId"), out var userId) ? userId : 0;
    }

    if (id <= 0)
        return RedirectToAction("Login", "Account");

    var user = _db.Users.FirstOrDefault(u => u.UserId == id);
    if (user == null)
        return RedirectToAction("Login", "Account");

    var addresses = _db.Addresses.Where(a => a.UserId == id).ToList();

    var profileModel = new ProfileViewModel
    {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email ?? "",
        Phone = user.Phone ?? "",
        Addresses = addresses
    };

    return View("~/Views/Account/Profile.cshtml", profileModel);
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้เปิดหน้าโปรไฟล์ของผู้ใช้

### โครงสร้างความคิด

1. ถ้าไม่มี `id` เข้ามา ให้ดึง `UserId` จาก session
2. ถ้ายังไม่ได้ login ให้ส่งไปหน้า login
3. หา user จากฐานข้อมูล
4. ดึงที่อยู่ทั้งหมดของ user
5. รวมข้อมูลใส่ `ProfileViewModel`
6. ส่งไปหน้า `Profile.cshtml`

### จุดสำคัญ

```csharp
id = int.TryParse(HttpContext.Session.GetString("UserId"), out var userId) ? userId : 0;
```

ถ้าไม่ได้ส่ง `id` มา จะใช้ `UserId` ของคนที่ login อยู่แทน

```csharp
var addresses = _db.Addresses.Where(a => a.UserId == id).ToList();
```

ดึงที่อยู่ทั้งหมดของ user คนนี้

### สรุปสั้น ๆ

`Profile GET` = ดึงข้อมูลผู้ใช้และที่อยู่ แล้วเปิดหน้าโปรไฟล์

---

## 3. ฟังก์ชัน Profile(ProfileViewModel model) แบบ POST

```csharp
[HttpPost]
public IActionResult Profile([FromBody] ProfileViewModel model)
{
    if (model.UserId <= 0)
        return Json(new { success = false, message = "ข้อมูลผู้ใช้ไม่ถูกต้อง" });

    var user = _db.Users.Find(model.UserId);
    if (user == null)
        return Json(new { success = false, message = "ไม่พบข้อมูลผู้ใช้" });

    if (!string.IsNullOrWhiteSpace(model.Username))
        user.Username = model.Username;
    if (!string.IsNullOrWhiteSpace(model.Email))
        user.Email = model.Email;
    if (!string.IsNullOrWhiteSpace(model.Phone))
        user.Phone = model.Phone;

    if (!string.IsNullOrEmpty(model.NewPassword))
    {
        if (string.IsNullOrEmpty(model.CurrentPassword))
            return Json(new { success = false, message = "กรุณากรอกรหัสผ่านปัจจุบัน" });

        if (!string.Equals(user.Password, model.CurrentPassword, StringComparison.Ordinal))
            return Json(new { success = false, message = "รหัสผ่านปัจจุบันไม่ถูกต้อง" });

        if (model.NewPassword != model.ConfirmPassword)
            return Json(new { success = false, message = "รหัสผ่านใหม่ไม่ตรงกัน" });

        if (model.NewPassword.Length < 6)
            return Json(new { success = false, message = "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร" });

        user.Password = model.NewPassword;
    }

    _db.SaveChanges();

    return Json(new { success = true, message = "บันทึกข้อมูลโปรไฟล์เรียบร้อยแล้ว!" });
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้บันทึกการแก้ไขข้อมูลโปรไฟล์

### โครงสร้างความคิด

1. ตรวจว่า `UserId` ถูกต้องไหม
2. หา user จากฐานข้อมูล
3. อัปเดตชื่อ อีเมล เบอร์โทร
4. ถ้ามีการเปลี่ยนรหัสผ่าน ให้เช็กหลายชั้น
5. บันทึกลงฐานข้อมูล
6. ส่งผลลัพธ์กลับแบบ JSON

### จุดสำคัญ

```csharp
if (!string.IsNullOrEmpty(model.NewPassword))
```

ถ้าผู้ใช้กรอกรหัสผ่านใหม่เข้ามา จึงค่อยเข้าส่วนเปลี่ยนรหัสผ่าน

```csharp
if (!string.Equals(user.Password, model.CurrentPassword, StringComparison.Ordinal))
```

ต้องตรวจรหัสผ่านเดิมก่อน เพื่อป้องกันการเปลี่ยนรหัสผ่านโดยไม่ยืนยันตัวตน

```csharp
_db.SaveChanges();
```

เป็นจุดที่บันทึกข้อมูลที่แก้ไขจริง

### สรุปสั้น ๆ

`Profile POST` = แก้ข้อมูลผู้ใช้ และอาจเปลี่ยนรหัสผ่านด้วย

---

## 4. ฟังก์ชัน SaveAddress(...)

```csharp
[HttpPost]
public IActionResult SaveAddress(int userId, int addressId, string addressLine, string district, string province, string postalCode)
{
    if (addressId > 0)
    {
        var address = _db.Addresses.Find(addressId);
        if (address != null && address.UserId == userId)
        {
            address.AddressLine = addressLine;
            address.District = district;
            address.Province = province;
            address.PostalCode = postalCode;
            _db.SaveChanges();
        }
    }
    else
    {
        var newAddress = new Address
        {
            UserId = userId,
            AddressLine = addressLine,
            District = district,
            Province = province,
            PostalCode = postalCode
        };
        _db.Addresses.Add(newAddress);
        _db.SaveChanges();
    }

    return Json(new { success = true, message = "บันทึกที่อยู่เรียบร้อยแล้ว!" });
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้ทั้งเพิ่มที่อยู่ใหม่ และแก้ไขที่อยู่เดิม

### วิธีอ่านให้ง่าย

แยกเป็น 2 กรณี

- `addressId > 0` = แก้ไขที่อยู่เดิม
- `addressId == 0` = เพิ่มที่อยู่ใหม่

### จุดสำคัญ

```csharp
if (address != null && address.UserId == userId)
```

แก้ได้ก็ต่อเมื่อที่อยู่นั้นเป็นของ user คนนี้จริง

### สรุปสั้น ๆ

`SaveAddress()` = ถ้ามี `addressId` คือแก้ไข ถ้าไม่มีคือเพิ่มใหม่

---

## 5. ฟังก์ชัน DeleteAddress(...)

```csharp
[HttpPost]
public IActionResult DeleteAddress(int addressId, int userId)
{
    var address = _db.Addresses.Find(addressId);
    if (address != null && address.UserId == userId)
    {
        _db.Addresses.Remove(address);
        _db.SaveChanges();
        return Json(new { success = true, message = "ลบที่อยู่เรียบร้อยแล้ว!" });
    }

    return Json(new { success = false, message = "ไม่พบที่อยู่หรือคุณไม่มีสิทธิ์ลบ" });
}
```

### ฟังก์ชันนี้ทำอะไร

ลบที่อยู่ของผู้ใช้

### สรุปสั้น ๆ

ลบได้ก็ต่อเมื่อที่อยู่นั้นเป็นของผู้ใช้คนนั้นจริง

---

## 6. ฟังก์ชัน GetUserAddresses(int userId)

```csharp
[HttpGet]
public IActionResult GetUserAddresses(int userId)
{
    var addresses = _db.Addresses.Where(a => a.UserId == userId).ToList();
    return Json(new { success = true, addresses });
}
```

### ฟังก์ชันนี้ทำอะไร

ดึงรายการที่อยู่ทั้งหมดของผู้ใช้แล้วส่งกลับเป็น JSON

### สรุปสั้น ๆ

มักใช้ตอนหน้าเว็บต้องรีเฟรชรายการที่อยู่โดยไม่ reload ทั้งหน้า

---

## สรุปภาพรวมของ ProfileController

- `Profile GET` ดึงข้อมูลผู้ใช้และที่อยู่
- `Profile POST` ใช้แก้ข้อมูลส่วนตัว
- `SaveAddress` ใช้ได้ทั้งเพิ่มและแก้ไข
- `DeleteAddress` ลบได้เฉพาะที่อยู่ของเจ้าของข้อมูล
- `GetUserAddresses` ส่งรายการที่อยู่กลับเป็น JSON
