# สอนอ่าน AccountController ทีละฟังก์ชัน

ไฟล์นี้ทำไว้สำหรับฝึกอ่าน `Controllers/AccountController.cs` แบบค่อย ๆ ดูทีละเมธอด  
แนวคิดคือ **ไม่ต้องรีบจำทุกบรรทัด** แต่ให้จับว่าแต่ละฟังก์ชัน

- รับอะไรเข้ามา
- ตรวจอะไรบ้าง
- ไปยุ่งกับฐานข้อมูลตรงไหน
- ตั้งค่า `Session` ตรงไหน
- สุดท้ายส่งอะไรกลับ

---

## AccountController มีหน้าที่อะไร

`AccountController` เป็น controller ที่ดูแลเรื่องบัญชีผู้ใช้ เช่น

- เปิดหน้า `Login`
- ตรวจการเข้าสู่ระบบ
- เปิดหน้า `Signup`
- สมัครสมาชิก
- ออกจากระบบ
- ลืมรหัสผ่าน
- ขอ OTP
- ตรวจ OTP
- ตั้งรหัสผ่านใหม่

สรุปง่าย ๆ คือไฟล์นี้เป็นจุดเริ่มของระบบฝั่งผู้ใช้ เพราะเกี่ยวกับการเข้าใช้งานและตัวตนของผู้ใช้โดยตรง

---

## วิธีอ่านไฟล์นี้ให้เข้าใจเร็ว

เวลาอ่านแต่ละฟังก์ชัน ให้ถามตัวเอง 4 ข้อ

1. ฟังก์ชันนี้เปิดหน้า หรือบันทึกข้อมูล
2. ฟังก์ชันนี้รับค่าอะไรเข้ามา
3. ฟังก์ชันนี้เช็กเงื่อนไขอะไรบ้าง
4. ฟังก์ชันนี้จบด้วย `View`, `Redirect`, หรือ `Json`

---

## 1. Constructor

```csharp
public AccountController(BakerydbContext db, ILogger<AccountController> logger)
{
    _db = db;
    _logger = logger;
}
```

### ฟังก์ชันนี้ทำอะไร

เป็น constructor ของ `AccountController`  
ใช้รับของสำคัญที่ controller ต้องใช้ตั้งแต่ต้น

- `_db` ใช้คุยกับฐานข้อมูล
- `_logger` ใช้บันทึก log

### อ่านทีละส่วน

```csharp
BakerydbContext db
```

คือฐานข้อมูลของระบบ ส่งเข้ามาผ่าน Dependency Injection

```csharp
ILogger<AccountController> logger
```

คือตัวช่วยเขียน log เช่น warning หรือ error

```csharp
_db = db;
_logger = logger;
```

เอาค่าที่รับมาเก็บไว้ในตัวแปรของคลาส เพื่อให้เมธอดอื่นใช้ต่อได้

### สรุปสั้น ๆ

ถ้าจะใช้ `AccountController` ต้องมีฐานข้อมูลกับ logger ส่งเข้ามาก่อน

---

## 2. ฟังก์ชัน Login() แบบ GET

```csharp
public IActionResult Login() => View(new AuthUserViewModel());
```

### ฟังก์ชันนี้ทำอะไร

ใช้เปิดหน้า `Login`

### อ่านทีละส่วน

```csharp
public IActionResult Login()
```

เป็น action ชื่อ `Login` และคืนค่ากลับเป็นผลลัพธ์ของหน้าเว็บ

```csharp
View(new AuthUserViewModel())
```

ส่ง `AuthUserViewModel` เปล่า ๆ ไปให้หน้า View ใช้งาน

### สรุปสั้น ๆ

ฟังก์ชันนี้ยังไม่ตรวจฐานข้อมูล  
แค่เปิดหน้า login พร้อม model ว่าง

---

## 3. ฟังก์ชัน Login(AuthUserViewModel model) แบบ POST

```csharp
[HttpPost]
public IActionResult Login(AuthUserViewModel model)
{
    model.Username = model.Username?.Trim();
    model.Email = model.Email?.Trim();
    model.Password = model.Password?.Trim();

    if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
    {
        ViewBag.Error = "กรุณากรอกชื่อผู้ใช้/อีเมล และรหัสผ่าน";
        return View(model);
    }

    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
    var user = _db.Users.FirstOrDefault(u =>
        u.Username == model.Username || u.Email == model.Username);

    if (user == null || user.Password != model.Password)
    {
        ViewBag.Error = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
        return View(model);
    }

    HttpContext.Session.SetInt32("UserId", user.UserId);
    HttpContext.Session.SetString("Username", user.Username ?? string.Empty);
    HttpContext.Session.SetInt32("RoleId", user.RoleId ?? 0);
    HttpContext.Session.SetString("RoleName", user.Role?.RoleName ?? string.Empty);

    LogUserLogin(user.UserId, user.Username ?? "-", ipAddress, "Success");

    if (user.RoleId == 1)
        return RedirectToAction("Dashbordadmin", "AdminDashboard");

    if (user.RoleId == 2)
        return RedirectToAction("Order", "AdminOrder");

    return RedirectToAction("Home", "Home");
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้รับค่าจากฟอร์ม login แล้วตรวจว่าผู้ใช้เข้าสู่ระบบได้หรือไม่

### โครงสร้างความคิดของฟังก์ชันนี้

1. จัดรูปแบบข้อมูลที่รับมา
2. เช็กว่ากรอกครบไหม
3. หา user ในฐานข้อมูล
4. ตรวจรหัสผ่าน
5. ถ้าถูกต้องให้เก็บข้อมูลลง `Session`
6. พาไปหน้าตามสิทธิ์ของผู้ใช้

### อ่านทีละช่วง

```csharp
model.Username = model.Username?.Trim();
model.Email = model.Email?.Trim();
model.Password = model.Password?.Trim();
```

ลบช่องว่างหน้าหลังออกก่อน เพื่อกันปัญหาผู้ใช้กรอกเว้นวรรคเกินมา

```csharp
if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
```

เช็กว่าผู้ใช้กรอกชื่อผู้ใช้กับรหัสผ่านมาหรือยัง

```csharp
ViewBag.Error = "กรุณากรอกชื่อผู้ใช้/อีเมล และรหัสผ่าน";
return View(model);
```

ถ้ากรอกไม่ครบ ให้กลับไปหน้าเดิมพร้อมข้อความ error

```csharp
var user = _db.Users.FirstOrDefault(u =>
    u.Username == model.Username || u.Email == model.Username);
```

ค้นหาผู้ใช้จากฐานข้อมูล โดยยอมให้กรอกได้ทั้ง `Username` หรือ `Email`

```csharp
if (user == null || user.Password != model.Password)
```

ถ้าไม่เจอ user หรือรหัสผ่านไม่ตรง ให้ถือว่า login ไม่สำเร็จ

```csharp
HttpContext.Session.SetInt32("UserId", user.UserId);
HttpContext.Session.SetString("Username", user.Username ?? string.Empty);
HttpContext.Session.SetInt32("RoleId", user.RoleId ?? 0);
HttpContext.Session.SetString("RoleName", user.Role?.RoleName ?? string.Empty);
```

ตรงนี้สำคัญมาก เพราะเป็นการสร้าง `Session` หลัง login สำเร็จ  
ระบบจะจำได้ว่าใครเป็นคนที่กำลังใช้งานอยู่

```csharp
if (user.RoleId == 1)
    return RedirectToAction("Dashbordadmin", "AdminDashboard");
```

ถ้าเป็น admin ให้ไป dashboard ฝั่งแอดมิน

```csharp
if (user.RoleId == 2)
    return RedirectToAction("Order", "AdminOrder");
```

ถ้าเป็น staff ให้ไปหน้าจัดการออเดอร์

```csharp
return RedirectToAction("Home", "Home");
```

ถ้าเป็นผู้ใช้ทั่วไป ให้ไปหน้าแรกของร้าน

### จุดที่ควรจำ

- `POST Login` เป็นฟังก์ชันที่เริ่มต้น session
- `RoleId` มีผลกับการ redirect หลัง login
- ถ้าอ่านฟังก์ชันนี้ออก จะเริ่มเข้าใจ `UserId`, `RoleId`, `Session` ทันที

---

## 4. ฟังก์ชัน Signup() แบบ GET

```csharp
public IActionResult Signup() => View(new AuthUserViewModel());
```

### ฟังก์ชันนี้ทำอะไร

ใช้เปิดหน้าสมัครสมาชิก

### สรุปสั้น ๆ

เหมือน `Login()` แบบ GET คือส่ง model ว่างไปให้หน้า view แสดงฟอร์ม

---

## 5. ฟังก์ชัน Signup(AuthUserViewModel model) แบบ POST

```csharp
[HttpPost]
public IActionResult Signup(AuthUserViewModel model)
{
    model.Username = model.Username?.Trim();
    model.Email = model.Email?.Trim();
    model.Phone = model.Phone?.Trim();
    model.Password = model.Password?.Trim();
    model.ConfirmPassword = model.ConfirmPassword?.Trim();

    if (model.Password != model.ConfirmPassword)
    {
        ViewBag.Error = "รหัสผ่านไม่ตรงกัน";
        return View(model);
    }

    if (string.IsNullOrWhiteSpace(model.Username) ||
        string.IsNullOrWhiteSpace(model.Email) ||
        string.IsNullOrWhiteSpace(model.Password))
    {
        ViewBag.Error = "กรุณากรอกข้อมูลให้ครบ";
        return View(model);
    }

    bool usernameExists = _db.Users.Any(u => u.Username == model.Username);
    bool emailExists = _db.Users.Any(u => u.Email == model.Email);

    if (usernameExists)
    {
        ViewBag.Error = "ชื่อผู้ใช้นี้ถูกใช้แล้ว";
        return View(model);
    }

    if (emailExists)
    {
        ViewBag.Error = "อีเมลนี้ถูกใช้แล้ว";
        return View(model);
    }

    var newUser = new User
    {
        Username = model.Username,
        Email = model.Email,
        Phone = model.Phone,
        Password = model.Password,
        RoleId = 3
    };

    _db.Users.Add(newUser);
    _db.SaveChanges();

    GrantWelcomePromotion(newUser.UserId);

    TempData["Success"] = "สมัครสมาชิกสำเร็จ";
    return RedirectToAction("Login");
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้รับข้อมูลจากหน้าสมัครสมาชิก แล้วสร้างผู้ใช้ใหม่ในฐานข้อมูล

### โครงสร้างความคิดของฟังก์ชันนี้

1. ล้างช่องว่าง
2. ตรวจว่ารหัสผ่านตรงกันไหม
3. ตรวจว่ากรอกข้อมูลครบไหม
4. เช็กว่าชื่อผู้ใช้หรืออีเมลซ้ำหรือเปล่า
5. สร้าง `User` ใหม่
6. บันทึกลงฐานข้อมูล
7. มอบโปรโมชันต้อนรับ
8. พาไปหน้า login

### อ่านทีละช่วง

```csharp
if (model.Password != model.ConfirmPassword)
```

ใช้เช็กว่าผู้ใช้พิมพ์ยืนยันรหัสผ่านตรงกันหรือไม่

```csharp
bool usernameExists = _db.Users.Any(u => u.Username == model.Username);
bool emailExists = _db.Users.Any(u => u.Email == model.Email);
```

ตรวจในฐานข้อมูลก่อนว่าข้อมูลซ้ำไหม

```csharp
var newUser = new User
{
    Username = model.Username,
    Email = model.Email,
    Phone = model.Phone,
    Password = model.Password,
    RoleId = 3
};
```

สร้าง object ผู้ใช้ใหม่  
จุดสำคัญคือ `RoleId = 3` หมายถึงผู้ใช้ทั่วไป

```csharp
_db.Users.Add(newUser);
_db.SaveChanges();
```

เพิ่มผู้ใช้ลงฐานข้อมูลและบันทึกจริง

```csharp
GrantWelcomePromotion(newUser.UserId);
```

หลังสมัครสำเร็จ จะมอบโปรโมชันต้อนรับให้ผู้ใช้อัตโนมัติ

### จุดที่ควรจำ

- `POST Signup` คือจุดที่สร้างข้อมูลลงตาราง `Users`
- role ของสมาชิกทั่วไปถูกกำหนดตอนสมัคร
- สมัครเสร็จไม่ได้ login ทันที แต่ redirect ไปหน้า `Login`

---

## 6. ฟังก์ชัน Logout()

```csharp
[HttpGet]
public IActionResult Logout()
{
    HttpContext.Session.Clear();
    return RedirectToAction("Home", "Home");
}
```

### ฟังก์ชันนี้ทำอะไร

ออกจากระบบ

### อ่านทีละส่วน

```csharp
HttpContext.Session.Clear();
```

ลบข้อมูล session ทั้งหมดของผู้ใช้คนปัจจุบัน

```csharp
return RedirectToAction("Home", "Home");
```

หลังออกจากระบบแล้ว ให้กลับไปหน้าแรก

### สรุปสั้น ๆ

logout = ล้าง session แล้วพากลับหน้า home

---

## 7. ฟังก์ชัน ForgotPassword() แบบ GET

```csharp
[HttpGet]
public IActionResult ForgotPassword()
{
    return View();
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้เปิดหน้าลืมรหัสผ่าน

### สรุปสั้น ๆ

ไม่มีการตรวจฐานข้อมูล  
แค่เปิดหน้าให้ผู้ใช้กรอกอีเมลเพื่อขอ OTP

---

## 8. ฟังก์ชัน RequestPasswordReset(...)

```csharp
[HttpPost]
public IActionResult RequestPasswordReset([FromBody] ForgotPasswordRequestViewModel model)
{
    if (model == null || string.IsNullOrWhiteSpace(model.Email))
    {
        return Json(new { success = false, message = "กรุณากรอกอีเมล" });
    }

    var email = model.Email.Trim();
    var user = _db.Users.FirstOrDefault(u => u.Email == email);

    if (user == null)
    {
        return Json(new { success = false, message = "ไม่พบบัญชีผู้ใช้นี้" });
    }

    var otp = RandomNumberGenerator.GetInt32(1000, 10000).ToString();
    user.PasswordResetOtp = otp;
    user.PasswordResetOtpExpireAt = DateTime.Now.AddMinutes(5);

    _db.SaveChanges();

    HttpContext.Session.Remove("PasswordResetVerifiedEmail");
    HttpContext.Session.Remove("PasswordResetVerifiedOtp");
    HttpContext.Session.Remove("PasswordResetVerifiedUserId");

    return Json(new
    {
        success = true,
        message = "ส่งรหัส OTP สำเร็จ",
        otp,
        expireAt = user.PasswordResetOtpExpireAt
    });
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้รับอีเมลจากหน้าลืมรหัสผ่าน แล้วสร้าง OTP สำหรับรีเซ็ตรหัสผ่าน

### อ่านทีละช่วง

```csharp
[FromBody] ForgotPasswordRequestViewModel model
```

หมายถึงรับข้อมูลจาก body ของ request มักใช้คู่กับ `fetch` หรือ AJAX

```csharp
var user = _db.Users.FirstOrDefault(u => u.Email == email);
```

หา user จากอีเมล

```csharp
var otp = RandomNumberGenerator.GetInt32(1000, 10000).ToString();
```

สร้าง OTP แบบสุ่ม 4 หลัก

```csharp
user.PasswordResetOtp = otp;
user.PasswordResetOtpExpireAt = DateTime.Now.AddMinutes(5);
```

เก็บ OTP และเวลาหมดอายุลงในข้อมูลผู้ใช้

```csharp
return Json(new { ... });
```

ตอบกลับเป็น JSON เพราะฟังก์ชันนี้ไม่ได้เปลี่ยนหน้า แต่ส่งผลลัพธ์ให้ JavaScript ฝั่งหน้าเว็บ

### จุดที่ควรจำ

- ฟังก์ชันนี้ยังไม่เปลี่ยนรหัสผ่าน
- แค่ “ขอ OTP”
- OTP ถูกเก็บในฐานข้อมูลชั่วคราว

---

## 9. ฟังก์ชัน VerifyPasswordResetOtp(...)

```csharp
[HttpPost]
public IActionResult VerifyPasswordResetOtp([FromBody] ForgotPasswordVerifyOtpViewModel model)
{
    if (model == null ||
        string.IsNullOrWhiteSpace(model.Email) ||
        string.IsNullOrWhiteSpace(model.Otp))
    {
        return Json(new { success = false, message = "ข้อมูลไม่ครบ" });
    }

    var email = model.Email.Trim();
    var otp = model.Otp.Trim();

    var user = _db.Users.FirstOrDefault(u => u.Email == email);
    if (user == null)
    {
        return Json(new { success = false, message = "ไม่พบบัญชีผู้ใช้" });
    }

    if (string.IsNullOrWhiteSpace(user.PasswordResetOtp) ||
        user.PasswordResetOtpExpireAt == null ||
        user.PasswordResetOtpExpireAt < DateTime.Now)
    {
        return Json(new { success = false, message = "OTP หมดอายุหรือไม่ถูกต้อง" });
    }

    if (user.PasswordResetOtp != otp)
    {
        return Json(new { success = false, message = "OTP ไม่ถูกต้อง" });
    }

    HttpContext.Session.SetString("PasswordResetVerifiedEmail", email);
    HttpContext.Session.SetString("PasswordResetVerifiedOtp", otp);
    HttpContext.Session.SetInt32("PasswordResetVerifiedUserId", user.UserId);

    return Json(new { success = true, message = "ยืนยัน OTP สำเร็จ" });
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้ตรวจว่า OTP ที่ผู้ใช้กรอกมาตรงกับของในระบบหรือไม่

### โครงสร้างการคิด

1. เช็กว่าข้อมูลมาครบไหม
2. หา user จากอีเมล
3. เช็กว่า OTP ยังไม่หมดอายุ
4. เช็กว่า OTP ตรงไหม
5. ถ้าถูกต้อง เก็บสถานะยืนยันผ่านไว้ใน session

### จุดสำคัญ

```csharp
HttpContext.Session.SetString("PasswordResetVerifiedEmail", email);
HttpContext.Session.SetString("PasswordResetVerifiedOtp", otp);
HttpContext.Session.SetInt32("PasswordResetVerifiedUserId", user.UserId);
```

ตรงนี้เป็นการบอกระบบว่า  
“ผู้ใช้นี้ยืนยัน OTP ผ่านแล้ว”  
เพื่อให้ไปเปลี่ยนรหัสผ่านในขั้นตอนถัดไปได้

### สรุปสั้น ๆ

ฟังก์ชันนี้ยังไม่ reset password  
แต่เป็นด่านตรวจ OTP ก่อน

---

## 10. ฟังก์ชัน ResetPassword(...)

```csharp
[HttpPost]
public IActionResult ResetPassword([FromBody] ForgotPasswordResetViewModel model)
{
    if (model == null ||
        string.IsNullOrWhiteSpace(model.Email) ||
        string.IsNullOrWhiteSpace(model.NewPassword) ||
        string.IsNullOrWhiteSpace(model.ConfirmPassword))
    {
        return Json(new { success = false, message = "กรุณากรอกข้อมูลให้ครบ" });
    }

    var verifiedEmail = HttpContext.Session.GetString("PasswordResetVerifiedEmail");
    var verifiedOtp = HttpContext.Session.GetString("PasswordResetVerifiedOtp");
    var verifiedUserId = HttpContext.Session.GetInt32("PasswordResetVerifiedUserId");

    if (string.IsNullOrWhiteSpace(verifiedEmail) ||
        string.IsNullOrWhiteSpace(verifiedOtp) ||
        !verifiedUserId.HasValue)
    {
        return Json(new { success = false, message = "กรุณายืนยัน OTP ก่อน" });
    }

    if (!string.Equals(model.Email.Trim(), verifiedEmail, StringComparison.OrdinalIgnoreCase))
    {
        return Json(new { success = false, message = "ข้อมูลไม่ตรงกับบัญชีที่ยืนยัน OTP" });
    }

    if (model.NewPassword != model.ConfirmPassword)
    {
        return Json(new { success = false, message = "รหัสผ่านใหม่ไม่ตรงกัน" });
    }

    if (model.NewPassword.Trim().Length < 6)
    {
        return Json(new { success = false, message = "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร" });
    }

    var user = _db.Users.FirstOrDefault(u => u.Email == verifiedEmail && u.UserId == verifiedUserId.Value);
    if (user == null)
    {
        return Json(new { success = false, message = "ไม่พบบัญชีผู้ใช้" });
    }

    if (string.IsNullOrWhiteSpace(user.PasswordResetOtp) ||
        user.PasswordResetOtpExpireAt == null ||
        user.PasswordResetOtpExpireAt < DateTime.Now ||
        !string.Equals(user.PasswordResetOtp, verifiedOtp, StringComparison.Ordinal))
    {
        return Json(new { success = false, message = "OTP หมดอายุ กรุณาขอใหม่อีกครั้ง" });
    }

    user.Password = model.NewPassword.Trim();
    user.PasswordResetOtp = null;
    user.PasswordResetOtpExpireAt = null;

    _db.SaveChanges();

    HttpContext.Session.Remove("PasswordResetVerifiedEmail");
    HttpContext.Session.Remove("PasswordResetVerifiedOtp");
    HttpContext.Session.Remove("PasswordResetVerifiedUserId");

    return Json(new
    {
        success = true,
        message = "เปลี่ยนรหัสผ่านสำเร็จ",
        redirectUrl = Url.Action("Login", "Account")
    });
}
```

### ฟังก์ชันนี้ทำอะไร

เป็นขั้นสุดท้ายของการลืมรหัสผ่าน  
ใช้ตั้งรหัสผ่านใหม่หลังจากยืนยัน OTP ผ่านแล้ว

### โครงสร้างความคิด

1. ตรวจว่ากรอกข้อมูลครบไหม
2. ตรวจว่า session ยืนยัน OTP ยังอยู่ไหม
3. ตรวจว่าอีเมลตรงกับคนที่ยืนยัน OTP ไหม
4. ตรวจว่ารหัสผ่านใหม่ตรงกันไหม
5. ตรวจความยาวรหัสผ่าน
6. หา user ตัวจริงในฐานข้อมูล
7. เช็ก OTP ซ้ำอีกครั้งเพื่อความปลอดภัย
8. เปลี่ยนรหัสผ่าน
9. ล้าง OTP และล้าง session ที่เกี่ยวข้อง

### จุดที่ควรดูเป็นพิเศษ

```csharp
var verifiedEmail = HttpContext.Session.GetString("PasswordResetVerifiedEmail");
var verifiedOtp = HttpContext.Session.GetString("PasswordResetVerifiedOtp");
var verifiedUserId = HttpContext.Session.GetInt32("PasswordResetVerifiedUserId");
```

ดึงข้อมูลที่ผ่านการยืนยัน OTP แล้วจาก session มาใช้

```csharp
user.Password = model.NewPassword.Trim();
```

บรรทัดนี้คือจุดที่เปลี่ยนรหัสผ่านจริง

```csharp
user.PasswordResetOtp = null;
user.PasswordResetOtpExpireAt = null;
```

ล้าง OTP ทิ้งหลังใช้งานเสร็จ เพื่อไม่ให้ใช้ซ้ำได้

### สรุปสั้น ๆ

ฟังก์ชันนี้คือ “เปลี่ยนรหัสผ่านจริง”  
และมีการเช็กหลายชั้นเพื่อกันการเปลี่ยนรหัสผ่านผิดคน

---

## 11. ฟังก์ชัน LogUserLogin(...)

```csharp
private void LogUserLogin(int userId, string username, string? ipAddress, string status)
{
    try
    {
        var logDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        var logFile = Path.Combine(logDir, $"login_{DateTime.Now:yyyy-MM-dd}.log");
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UserId={userId}, Username={username}, IP={ipAddress}, Status={status}";

        System.IO.File.AppendAllText(logFile, line + Environment.NewLine);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "เกิดข้อผิดพลาดในการบันทึก login log");
    }
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้บันทึก log การ login ลงไฟล์

### อ่านทีละช่วง

```csharp
var logDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
```

กำหนดตำแหน่งโฟลเดอร์เก็บ log

```csharp
if (!Directory.Exists(logDir))
{
    Directory.CreateDirectory(logDir);
}
```

ถ้ายังไม่มีโฟลเดอร์ log ก็สร้างขึ้นมาก่อน

```csharp
var logFile = Path.Combine(logDir, $"login_{DateTime.Now:yyyy-MM-dd}.log");
```

ตั้งชื่อไฟล์ log แยกตามวัน

```csharp
System.IO.File.AppendAllText(logFile, line + Environment.NewLine);
```

เพิ่มข้อความ log ลงท้ายไฟล์

### สรุปสั้น ๆ

ฟังก์ชันนี้เป็น helper ภายใน  
ไม่ได้เกี่ยวกับหน้าเว็บโดยตรง แต่ช่วยเก็บประวัติการ login

---

## 12. ฟังก์ชัน GrantWelcomePromotion(...)

```csharp
private void GrantWelcomePromotion(int userId)
{
    try
    {
        var promotion = _db.Promotions.FirstOrDefault(p => p.PromotionId == WelcomePromotionId);
        if (promotion == null)
        {
            _logger.LogWarning("ไม่พบโปรโมชันต้อนรับที่กำหนดไว้");
            return;
        }

        var alreadyGranted = _db.UserPromotions.Any(up =>
            up.UserId == userId && up.PromotionId == promotion.PromotionId);

        if (alreadyGranted)
        {
            return;
        }

        _db.UserPromotions.Add(new UserPromotion
        {
            UserId = userId,
            PromotionId = promotion.PromotionId,
            IsUsed = false,
            AssignedDate = DateTime.Now
        });

        _db.SaveChanges();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "เกิดข้อผิดพลาดในการมอบโปรโมชันต้อนรับ");
    }
}
```

### ฟังก์ชันนี้ทำอะไร

มอบโปรโมชันต้อนรับให้ผู้ใช้ใหม่หลังสมัครสมาชิก

### โครงสร้างความคิด

1. หาโปรโมชันต้อนรับจากฐานข้อมูล
2. ถ้าไม่เจอ ให้จบและเขียน log
3. เช็กว่าผู้ใช้คนนี้เคยได้โปรนี้แล้วหรือยัง
4. ถ้ายังไม่เคยได้ ให้เพิ่มข้อมูลลง `UserPromotions`

### จุดสำคัญ

```csharp
var promotion = _db.Promotions.FirstOrDefault(p => p.PromotionId == WelcomePromotionId);
```

หาโปรโมชันจากเลข id คงที่ที่ระบบกำหนดไว้

```csharp
var alreadyGranted = _db.UserPromotions.Any(up =>
    up.UserId == userId && up.PromotionId == promotion.PromotionId);
```

กันการแจกซ้ำ

```csharp
_db.UserPromotions.Add(new UserPromotion
{
    UserId = userId,
    PromotionId = promotion.PromotionId,
    IsUsed = false,
    AssignedDate = DateTime.Now
});
```

เพิ่มสิทธิ์โปรโมชันให้ผู้ใช้ในตารางเชื่อม

### สรุปสั้น ๆ

ฟังก์ชันนี้เป็น helper ที่ถูกเรียกจาก `Signup`  
ทำให้ผู้ใช้ใหม่ได้รับโปรโมชันต้อนรับอัตโนมัติ

---

## สรุปภาพรวมของ AccountController

ถ้าจะจำทั้งไฟล์นี้แบบสั้น ๆ ให้จำเป็น 4 กลุ่ม

### กลุ่ม 1 เปิดหน้า

- `Login()`
- `Signup()`
- `ForgotPassword()`

หน้าที่คือเปิดหน้า view

### กลุ่ม 2 จัดการบัญชีผู้ใช้

- `Login(AuthUserViewModel model)`
- `Signup(AuthUserViewModel model)`
- `Logout()`

หน้าที่คือเข้าใช้ระบบ สมัครสมาชิก และออกจากระบบ

### กลุ่ม 3 ลืมรหัสผ่าน

- `RequestPasswordReset(...)`
- `VerifyPasswordResetOtp(...)`
- `ResetPassword(...)`

หน้าที่คือขอ OTP ตรวจ OTP และตั้งรหัสผ่านใหม่

### กลุ่ม 4 ฟังก์ชันช่วย

- `LogUserLogin(...)`
- `GrantWelcomePromotion(...)`

หน้าที่คือช่วยงานเบื้องหลัง ไม่ได้เป็นหน้าเว็บโดยตรง

---

## ถ้าจะอ่านให้เข้าใจจริง ควรจำอะไร

- `Login POST` เป็นจุดเริ่มของ `Session`
- `Signup POST` เป็นจุดสร้าง `User`
- `RoleId` ใช้ตัดสินว่าจะพาไปหน้าไหนหลัง login
- `Forgot Password` มี 3 ขั้น คือ ขอ OTP -> ตรวจ OTP -> เปลี่ยนรหัสผ่าน
- ฟังก์ชัน helper มักถูกเรียกจากฟังก์ชันหลักอีกที

---

## คำแนะนำก่อนไปไฟล์ถัดไป

ถ้าคุณอ่านไฟล์นี้แล้วเริ่มเข้าใจ ให้ลองสรุปเอง 1 บรรทัดต่อฟังก์ชัน เช่น

- `Login GET` = เปิดหน้า login
- `Login POST` = ตรวจผู้ใช้และสร้าง session
- `Signup POST` = สมัครสมาชิกและเพิ่ม user ลงฐานข้อมูล
- `ResetPassword` = เปลี่ยนรหัสผ่านจริงหลังยืนยัน OTP ผ่านแล้ว

ถ้าสรุปเองได้ แปลว่าคุณเริ่มอ่าน controller เป็นแล้ว
