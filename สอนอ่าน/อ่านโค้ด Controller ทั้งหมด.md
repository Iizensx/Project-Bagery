# อ่านโค้ด Controller ทั้งหมด

ไฟล์นี้ทำขึ้นเพื่อใช้เป็นแนวทางอ่านโค้ด `Controller` ของโปรเจกต์แบบค่อย ๆ ไล่จากง่ายไปยาก โดยเน้นให้อ่านแล้วเข้าใจว่าแต่ละไฟล์ทำหน้าที่อะไร ไม่ใช่แค่อ่านผ่าน ๆ แล้วจำชื่อฟังก์ชันอย่างเดียว

แนวคิดหลักของการอ่าน `Controller` คือ
-
- ไม่ต้องเริ่มจากจำตัวแปรทั้งหมด
- ให้เริ่มจากดูว่าไฟล์นั้น “ดูแลเรื่องอะไร”
- จากนั้นดูชื่อฟังก์ชันว่า “ฟังก์ชันนี้ทำอะไร”
- ค่อยไล่ดู parameter, การเช็กเงื่อนไข, การคุยกับฐานข้อมูล และผลลัพธ์สุดท้าย

---

## ลำดับการอ่านที่แนะนำ

### 1. ฝั่งผู้ใช้ทั่วไป

#### `AccountController`
เริ่มจาก `Login`, `Signup`, `ForgotPassword`

เหตุผล:

- เป็นจุดเริ่มของระบบ
- ทำให้เข้าใจ `Session`
- ทำให้เข้าใจ `UserId`, `RoleId`
- ทำให้เห็นว่าหลัง login แล้วระบบแยกสิทธิ์ยังไง

#### `HomeController`
อ่าน `Home`, `Menu`, `Promotion`

เหตุผล:

- จะเห็นว่าหลัง login แล้วผู้ใช้เข้าหน้าไหน
- จะเห็นว่าข้อมูลหน้าร้าน เช่น สินค้าและโปรโมชัน ดึงจากไหน

#### `ProfileController`
อ่าน `Profile`, `SaveAddress`, `DeleteAddress`, `GetUserAddresses`

เหตุผล:

- ช่วยให้เข้าใจข้อมูลผู้ใช้
- ช่วยให้เข้าใจที่อยู่และการใช้งานก่อน checkout

#### `OrderController`
อ่าน `Checkout`, `CreateOrder`, `GetUserPromos`, `Payment`, `UploadSlip`

เหตุผล:

- เป็น flow หลักของระบบขาย
- เป็นส่วนที่เชื่อมสินค้า โปรโมชัน ออเดอร์ และการชำระเงินเข้าด้วยกัน

#### `DeliveryController`
อ่าน `Delivery`, `CompleteOrder`

เหตุผล:

- จะเห็นปลายทางของออเดอร์หลังสั่งซื้อ
- เข้าใจว่าระบบปิดงานออเดอร์ยังไง

#### `NotificationController` หรือส่วนแจ้งเตือนที่เกี่ยวข้อง

เหตุผล:

- เอาไว้เชื่อมว่าแต่ละ action แจ้งอะไรกลับผู้ใช้
- ช่วยให้เห็นภาพการทำงานต่อเนื่องของทั้งระบบ

### 2. ฝั่งหลังบ้าน

#### `AdminControllerBase`

เหตุผล:

- ควรอ่านก่อน controller ฝั่ง admin ตัวอื่น
- เพราะเป็นฐานร่วมของ `admin/staff`
- จะช่วยให้เข้าใจการเช็กสิทธิ์และการ redirect

#### `AdminOrderController`

เหตุผล:

- เชื่อมต่อจาก flow ออเดอร์โดยตรง
- มีการตรวจสลิป ยืนยันชำระ รับออเดอร์ และจัดส่ง

#### `AdminStockController`

เหตุผล:

- ใช้อ่านเรื่องสินค้า สต็อก และหมวดหมู่

#### `AdminPromotionController`

เหตุผล:

- มีหลายกรณี เช่น สร้างโปรโมชัน แจกโปรโมชัน และอนุมัติ claim

#### `AdminMemberController`

เหตุผล:

- เป็นระบบจัดการผู้ใช้
- อ่านทีหลังได้ เพราะไม่กระทบ flow สั่งซื้อโดยตรง

#### `AdminDashboardController`

เหตุผล:

- อ่านท้าย ๆ ได้
- เพราะ dashboard เป็นหน้าสรุปจากข้อมูลที่ควรเข้าใจมาก่อนแล้ว

---

## วิธีอ่าน Controller ให้เข้าใจเร็ว

เวลาเปิดไฟล์ controller ขึ้นมา ให้ถามตัวเองตามนี้

1. ไฟล์นี้ดูแลเรื่องอะไร
2. ฟังก์ชันนี้เปิดหน้า หรือบันทึกข้อมูล
3. ฟังก์ชันนี้รับอะไรเข้ามา
4. ฟังก์ชันนี้เช็กเงื่อนไขอะไรบ้าง
5. ฟังก์ชันนี้ไปยุ่งกับตารางไหนในฐานข้อมูล
6. สุดท้ายมันคืนอะไรกลับมา เช่น `View`, `Redirect`, `Json`

จำง่าย ๆ:

- ชื่อไฟล์ บอก “ขอบเขตงาน”
- ชื่อฟังก์ชัน บอก “หน้าที่”
- parameter บอก “ข้อมูลที่รับ”
- `Db.xxx` บอก “ตารางที่เกี่ยวข้อง”
- `return` บอก “ผลลัพธ์สุดท้าย”

---

## หน้าแรก: สอนอ่าน `AdminControllerBase.cs`

ไฟล์นี้คือ

- คลาสฐานของ controller ฝั่ง `Admin`
- รวมเมธอดที่ใช้ซ้ำบ่อย
- ทำให้ controller ลูกไม่ต้องเขียนโค้ดซ้ำ

โค้ดตัวอย่าง

```csharp
using Microsoft.AspNetCore.Mvc;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

public abstract class AdminControllerBase : Controller
{
    protected readonly BakerydbContext Db;
    protected const int AdminRoleId = 1;
    protected const int StaffRoleId = 2;

    protected AdminControllerBase(BakerydbContext db)
    {
        Db = db;
    }

    protected bool IsCurrentUserAdmin()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return false;

        return Db.Users.Any(u => u.UserId == userId && u.RoleId == AdminRoleId);
    }

    protected bool IsCurrentUserStaff()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return false;

        return Db.Users.Any(u => u.UserId == userId && u.RoleId == StaffRoleId);
    }

    protected bool IsCurrentUserAdminOrStaff()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return false;

        return Db.Users.Any(u => u.UserId == userId && (u.RoleId == AdminRoleId || u.RoleId == StaffRoleId));
    }

    protected int GetCurrentUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        return int.TryParse(userIdString, out var userId) ? userId : 0;
    }

    protected IActionResult RedirectToAdminLogin()
    {
        if (IsCurrentUserStaff())
            return RedirectToAction("Order", "AdminOrder", new { area = "" });

        if (IsCurrentUserAdmin())
            return RedirectToAction("Dashbordadmin", "AdminDashboard", new { area = "" });

        return RedirectToAction("Login", "Account", new { area = "" });
    }
}
```

---

## 1. บรรทัดบนสุด

```csharp
using Microsoft.AspNetCore.Mvc;
using _66022380.Models.Db;
```

ตรงนี้คือการ import namespace

### `Microsoft.AspNetCore.Mvc`

ใช้พวก

- `Controller`
- `IActionResult`
- `RedirectToAction()`

ถ้าไม่มี จะใช้ของ ASP.NET MVC ไม่ได้

### `_66022380.Models.Db`

ใช้ `BakerydbContext` และ model ตารางต่าง ๆ ในฐานข้อมูล

จำง่าย ๆ:

`using` = ดึงเครื่องมือจากที่อื่นมาใช้

---

## 2. namespace

```csharp
namespace _66022380.Controllers.Admin;
```

บอกว่าไฟล์นี้อยู่ในกลุ่ม `Controllers.Admin`

ทำให้รู้ว่า

- เป็นไฟล์ฝั่ง `Controller`
- อยู่ในหมวด `Admin`

---

## 3. คอมเมนต์อธิบายคลาส

```csharp
// Base Controller สำหรับฝั่ง Admin
// รวมเมธอดที่ใช้ซ้ำร่วมกัน เช่น การเข้าถึง DbContext, การอ่าน UserId จาก Session
// และการตรวจสอบว่าผู้ใช้ปัจจุบันมีสิทธิ์เป็น Admin หรือไม่
```

คอมเมนต์นี้สำคัญมาก เพราะบอก “หน้าที่ของไฟล์ทั้งไฟล์”

สรุปจากคอมเมนต์:

- เข้าถึงฐานข้อมูล
- อ่าน `UserId` จาก session
- เช็กสิทธิ์ `admin/staff`
- redirect ไปหน้าที่เหมาะสม

---

## 4. ประกาศคลาส

```csharp
public abstract class AdminControllerBase : Controller
```

บรรทัดนี้สำคัญมาก

### `public`

คลาสนี้ถูกมองเห็นจากที่อื่นได้

### `abstract`

หมายถึง “คลาสแม่” ใช้สำหรับให้คลาสลูกสืบทอดต่อ  
ปกติจะไม่สร้าง object จากคลาสนี้โดยตรง

### `AdminControllerBase`

ชื่อคลาส

### `: Controller`

หมายถึงคลาสนี้สืบทอดจาก `Controller` ของ ASP.NET MVC

สรุป:

`AdminControllerBase` เป็น controller พื้นฐาน ที่ controller อื่นเอาไป inherit ต่อ

เช่นถ้าไฟล์อื่นเขียนแบบนี้

```csharp
public class AdminOrderController : AdminControllerBase
```

แปลว่า `AdminOrderController` จะใช้เมธอดพวก `GetCurrentUserId()` และ `IsCurrentUserAdmin()` ได้เลย

---

## 5. ตัวแปรระดับคลาส

```csharp
protected readonly BakerydbContext Db;
protected const int AdminRoleId = 1;
protected const int StaffRoleId = 2;
```

ตรงนี้คือข้อมูลที่ทั้งคลาสใช้ร่วมกัน

### `protected readonly BakerydbContext Db;`

- `protected` = ใช้ได้ในคลาสนี้ และคลาสลูก
- `readonly` = กำหนดค่าได้ตอนสร้าง object แล้วเปลี่ยนทีหลังไม่ได้
- `BakerydbContext` = ตัวเชื่อมฐานข้อมูล
- `Db` = ชื่อตัวแปร

สรุป:

`Db` คือทางเข้าไป query ฐานข้อมูล เช่น `Db.Users`

### `protected const int AdminRoleId = 1;`
### `protected const int StaffRoleId = 2;`

คือค่าคงที่

- `Admin = role id 1`
- `Staff = role id 2`

ข้อดีคือไม่ต้องเขียนเลข `1`, `2` กระจายเต็มโค้ด ทำให้อ่านง่ายขึ้น

---

## 6. Constructor

```csharp
protected AdminControllerBase(BakerydbContext db)
{
    Db = db;
}
```

ตรงนี้คือ constructor

อ่านแบบง่าย:

เวลา ASP.NET สร้าง controller ตัวนี้ มันจะส่ง `BakerydbContext` เข้ามาให้  
แล้วโค้ดนี้เอาค่านั้นมาเก็บไว้ใน `Db`

### ทำไมต้องมี constructor

เพราะคลาสนี้ต้องใช้ฐานข้อมูล  
และ ASP.NET Core ใช้ Dependency Injection ส่ง `db` มาให้

มองแบบภาษาคน:

“ถ้าจะใช้คลาสนี้ ต้องเอาฐานข้อมูลมาให้ก่อน”

---

## 7. เมธอด `IsCurrentUserAdmin()`

```csharp
protected bool IsCurrentUserAdmin()
{
    var userId = GetCurrentUserId();
    if (userId <= 0)
        return false;

    return Db.Users.Any(u => u.UserId == userId && u.RoleId == AdminRoleId);
}
```

อันนี้เป็นเมธอดเช็กว่า “คนที่ login อยู่ตอนนี้เป็น admin ไหม”

### อ่านทีละบรรทัด

```csharp
var userId = GetCurrentUserId();
```

เรียกเมธอด `GetCurrentUserId()` เพื่อเอา `UserId` จาก session

```csharp
if (userId <= 0)
    return false;
```

ถ้า `userId` เป็น `0` หรือไม่มีค่า  
แปลว่ายังไม่ได้ login หรือ session ผิด  
ก็ให้ตอบเลยว่า “ไม่ใช่ admin”

```csharp
return Db.Users.Any(u => u.UserId == userId && u.RoleId == AdminRoleId);
```

เช็กในตาราง `Users` ว่ามีผู้ใช้คนนี้ไหม และ role เป็น admin ไหม

### `Any(...)` คืออะไร

แปลว่า “มีข้อมูลที่ตรงเงื่อนไขนี้อย่างน้อย 1 แถวไหม”

- ถ้ามี -> `true`
- ถ้าไม่มี -> `false`

### สรุปเมธอดนี้

1. เอา `UserId` จาก session
2. ถ้าไม่มี user -> `false`
3. ถ้ามี user -> ไปเช็กในฐานข้อมูลว่า role เป็น admin ไหม

---

## 8. เมธอด `IsCurrentUserStaff()`

```csharp
protected bool IsCurrentUserStaff()
{
    var userId = GetCurrentUserId();
    if (userId <= 0)
        return false;

    return Db.Users.Any(u => u.UserId == userId && u.RoleId == StaffRoleId);
}
```

ตัวนี้เหมือน `IsCurrentUserAdmin()` แทบทั้งหมด  
ต่างกันแค่เช็ก role เป็น `StaffRoleId`

สรุป:

ใช้ตรวจว่าผู้ใช้ปัจจุบันเป็น staff หรือไม่

---

## 9. เมธอด `IsCurrentUserAdminOrStaff()`

```csharp
protected bool IsCurrentUserAdminOrStaff()
{
    var userId = GetCurrentUserId();
    if (userId <= 0)
        return false;

    return Db.Users.Any(u => u.UserId == userId && (u.RoleId == AdminRoleId || u.RoleId == StaffRoleId));
}
```

อันนี้ไว้เช็กว่าเป็น “admin หรือ staff ก็ได้”

### จุดสำคัญคือเงื่อนไขนี้

```csharp
(u.RoleId == AdminRoleId || u.RoleId == StaffRoleId)
```

- `||` = หรือ
- ถ้า role เป็น admin หรือ staff อย่างใดอย่างหนึ่ง ก็ผ่าน

### ใช้เมื่อไหร่

ใช้กับหน้าที่ทั้ง admin และ staff เข้าถึงได้  
เช่นหน้า order หรือหน้า stock บางส่วน

---

## 10. เมธอด `GetCurrentUserId()`

```csharp
protected int GetCurrentUserId()
{
    var userIdString = HttpContext.Session.GetString("UserId");
    return int.TryParse(userIdString, out var userId) ? userId : 0;
}
```

อันนี้สำคัญมาก เพราะเป็นฐานของเมธอดเช็กสิทธิ์ทั้งหมด

### บรรทัดแรก

```csharp
var userIdString = HttpContext.Session.GetString("UserId");
```

ไปดึงค่า `"UserId"` จาก session

ค่าที่ดึงมาจะเป็น string เช่น `"5"`

### บรรทัดที่สอง

```csharp
return int.TryParse(userIdString, out var userId) ? userId : 0;
```

ลองแปลง string ให้เป็น int

- ถ้าแปลงได้ -> คืนค่า `userId`
- ถ้าแปลงไม่ได้ -> คืน `0`

### ทำไมใช้ `0`

เพราะในระบบนี้ใช้ `0` เป็นความหมายว่า

- ไม่มีผู้ใช้ที่ login อยู่
- หรืออ่านค่าไม่ได้

### สรุปเมธอดนี้

อ่าน `UserId` จาก session แล้วแปลงเป็นเลขให้พร้อมใช้งาน

---

## 11. เมธอด `RedirectToAdminLogin()`

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

ชื่อดูเหมือน “ส่งไปหน้า login อย่างเดียว”  
แต่จริง ๆ มันฉลาดกว่านั้น

### ทำงานยังไง

#### กรณี 1: ถ้าเป็น staff

```csharp
if (IsCurrentUserStaff())
    return RedirectToAction("Order", "AdminOrder", new { area = "" });
```

จะพาไปหน้า `Order` ของ `AdminOrderController`

#### กรณี 2: ถ้าเป็น admin

```csharp
if (IsCurrentUserAdmin())
    return RedirectToAction("Dashbordadmin", "AdminDashboard", new { area = "" });
```

จะพาไปหน้า dashboard

#### กรณี 3: ถ้าไม่ใช่ทั้งสอง

```csharp
return RedirectToAction("Login", "Account", new { area = "" });
```

ส่งกลับไปหน้า login

---

## 12. ทำไมต้องมีเมธอดนี้

เพราะเวลาใน controller ลูกเขียนแบบนี้

```csharp
if (!IsCurrentUserAdmin())
    return RedirectToAdminLogin();
```

มันจะไม่ต้องเขียนโค้ด redirect ซ้ำทุกไฟล์

และยังช่วยแยกเคสได้ว่า

- ถ้าเป็น staff แล้วเผลอเข้าหน้า admin ล้วน ๆ -> ส่งกลับหน้าที่ staff ควรอยู่
- ถ้าเป็น admin -> ส่งกลับ dashboard
- ถ้ายังไม่ login -> ส่งไป login

---

## 13. ภาพรวมทั้งไฟล์แบบสั้นมาก

ถ้าจะสรุปไฟล์นี้ในหัวเดียว:

`AdminControllerBase` คือคลาสแม่ที่รวมเครื่องมือกลางของฝั่งแอดมินสำหรับ

- ใช้ `Db` คุยกับฐานข้อมูล
- อ่าน `UserId` จาก session
- เช็กว่า user เป็น `admin / staff / ทั้งสอง`
- redirect ไปหน้าที่เหมาะสมเมื่อไม่มีสิทธิ์

---

## 14. วิธีอ่านไฟล์นี้ให้เข้าใจจริง

ผมแนะนำให้มองเป็น 4 กลุ่ม

### กลุ่ม 1: ข้อมูลพื้นฐาน

- `Db`
- `AdminRoleId`
- `StaffRoleId`

### กลุ่ม 2: อ่านข้อมูลคนปัจจุบัน

- `GetCurrentUserId()`

### กลุ่ม 3: เช็กสิทธิ์

- `IsCurrentUserAdmin()`
- `IsCurrentUserStaff()`
- `IsCurrentUserAdminOrStaff()`

### กลุ่ม 4: จัดทางไปหน้าที่เหมาะสม

- `RedirectToAdminLogin()`

ถ้าแยกได้ 4 กลุ่มนี้ จะอ่านคลาสนี้ออกทันที

---

## 15. จุดที่ควรจำจริง ๆ

ไม่ต้องจำทุกตัว  
จำแค่นี้พอ

- `Db` = ตัวคุยฐานข้อมูล
- `GetCurrentUserId()` = อ่าน user จาก session
- `IsCurrentUserAdmin()` = เช็ก admin
- `IsCurrentUserStaff()` = เช็ก staff
- `IsCurrentUserAdminOrStaff()` = เช็กรวม
- `RedirectToAdminLogin()` = ส่งกลับหน้าที่ควรไป

---

## 16. ตัวอย่างเวลาคลาสลูกเอาไปใช้

เช่นใน controller อื่นจะเขียนประมาณนี้

```csharp
public IActionResult Member()
{
    if (!IsCurrentUserAdmin())
        return RedirectToAdminLogin();

    var users = Db.Users.ToList();
    return View(users);
}
```

อ่านได้ว่า

1. เช็กก่อนว่าเป็น admin ไหม
2. ถ้าไม่ใช่ก็ redirect
3. ถ้าใช่ค่อยดึง `users`
4. ส่งไป `view`

จะเห็นเลยว่า `AdminControllerBase` ทำให้โค้ด controller ลูกสั้นลงมาก

---

## 17. สรุปสุดท้ายของหน้าแรก

ถ้าจะอ่านไฟล์นี้ให้เข้าใจ ไม่ต้องเริ่มจากจำตัวแปร  
ให้เริ่มจากถามว่า

- คลาสนี้มีไว้ทำอะไร
- เมธอดแต่ละตัวตอบคำถามอะไร
- ใช้ session ตรงไหน
- ใช้ฐานข้อมูลตรงไหน
- คืนค่าอะไรกลับ

สำหรับไฟล์นี้ เมธอดแต่ละตัวตอบคำถามแบบนี้

- `GetCurrentUserId()` = ตอนนี้คือ user คนไหน
- `IsCurrentUserAdmin()` = user นี้เป็น admin ไหม
- `IsCurrentUserStaff()` = user นี้เป็น staff ไหม
- `IsCurrentUserAdminOrStaff()` = user นี้มีสิทธิ์หลังบ้านไหม
- `RedirectToAdminLogin()` = ถ้าไม่ควรอยู่หน้านี้ ควรส่งไปไหน

ถ้าอ่านถึงตรงนี้แล้วเข้าใจ แปลว่าพร้อมไปต่อไฟล์ถัดไปแล้ว

---

## หน้าถัดไปที่ควรอ่านต่อ

ถ้าหน้านี้เข้าใจแล้ว ไฟล์ถัดไปที่ควรอ่านคือ

- `AccountController`
หรือถ้าจะต่อฝั่งหลังบ้านเลย
- `AdminOrderController`

เพราะสองไฟล์นี้จะทำให้เห็นการเอาแนวคิดจาก `base controller` ไปใช้จริง

---

## 18. สอนอ่าน `AccountController.cs`

ไฟล์นี้เป็นจุดเริ่มของระบบฝั่งผู้ใช้ เพราะรวมเรื่อง

- `Login`
- `Signup`
- `Logout`
- `ForgotPassword`

ถ้าอ่านไฟล์นี้เข้าใจ จะเริ่มเห็นภาพว่า

- ระบบเก็บ session ยังไง
- ใช้ `UserId` และ `RoleId` ยังไง
- หลัง login แล้วระบบพาไปหน้าไหน

### ก่อนอ่านแต่ละเมธอด ให้มองภาพรวมก่อน

ตัวแปรระดับคลาสของไฟล์นี้คือ

- `_db` = ใช้คุยกับฐานข้อมูล
- `_logger` = ใช้บันทึก log
- `WelcomePromotionId = 1` = โปรโมชันต้อนรับสมาชิกใหม่

แปลว่าไฟล์นี้ไม่ได้มีแค่ login อย่างเดียว แต่ยังเชื่อมกับ

- ตาราง `Users`
- ตาราง `Promotions`
- ตาราง `UserPromotions`
- ระบบ `Session`

### เมธอดที่ควรอ่านก่อน

#### 1. `Login()`

มี 2 แบบ

- `GET Login()` = เปิดหน้า login
- `POST Login(AuthUserViewModel model)` = รับค่าจากฟอร์มแล้วตรวจสอบ

เวลาอ่าน `POST Login(...)` ให้ดูตามนี้

1. มีการ trim ค่า `Username` และเตรียม `Password`
2. ตรวจว่ากรอกข้อมูลครบไหม
3. ค้นหา user จาก `Username` หรือ `Email`
4. เช็กรหัสผ่าน
5. ถ้าถูกต้อง ให้เซ็ต session ได้แก่
   - `UserId`
   - `Username`
   - `RoleId`
   - `RoleName`
6. redirect ตาม role
   - Admin -> `AdminDashboard`
   - Staff -> `AdminOrder`
   - User -> `Home`

สรุปสั้น ๆ:

`Login` คือเมธอดที่ทำให้เราเข้าใจ `Session`, `UserId`, `RoleId` มากที่สุดในโปรเจกต์

#### 2. `Signup()`

มี 2 แบบ

- `GET Signup()` = เปิดหน้าสมัครสมาชิก
- `POST Signup(AuthUserViewModel model)` = รับข้อมูลสมัคร

เวลาอ่านให้ดูว่า

1. ระบบ trim ค่า input
2. เช็กว่ารหัสผ่านตรงกับ `ConfirmPassword` ไหม
3. เช็กว่ากรอกข้อมูลสำคัญครบไหม
4. เช็กว่า `Username` หรือ `Email` ซ้ำไหม
5. ถ้าไม่ซ้ำ -> สร้าง `User` ใหม่
6. กำหนด `RoleId = 3`
7. เรียก `GrantWelcomePromotion(newUser.UserId)`

สรุป:

`Signup` คือเมธอดสร้างบัญชีผู้ใช้ใหม่ และต่อให้เห็นว่าระบบแจกโปรต้อนรับอัตโนมัติ

#### 3. `ForgotPassword()` และชุดรีเซ็ตรหัสผ่าน

อ่านเป็น 3 ช่วง

- `RequestPasswordReset(...)`
- `VerifyPasswordResetOtp(...)`
- `ResetPassword(...)`

เวลาอ่านให้มองเป็น flow เดียว

1. ผู้ใช้กรอกอีเมล
2. ระบบสร้าง OTP และเวลาหมดอายุ
3. ผู้ใช้กรอก OTP
4. ระบบเช็ก OTP และเก็บสถานะลง session
5. ผู้ใช้ตั้งรหัสผ่านใหม่
6. ระบบอัปเดตรหัสผ่านและล้างค่า OTP

### เมธอดเสริมที่ควรอ่าน

#### `LogUserLogin(...)`

ใช้บันทึก log การ login ลงไฟล์ใน `wwwroot/logs`

#### `GrantWelcomePromotion(int userId)`

ใช้แจกโปรโมชันต้อนรับให้ผู้ใช้ใหม่หลังสมัครสมาชิกสำเร็จ

### สรุปไฟล์ `AccountController`

ไฟล์นี้ตอบคำถามสำคัญของระบบว่า

- ผู้ใช้เข้าใช้งานยังไง
- ระบบจำผู้ใช้ยังไง
- ระบบแยกสิทธิ์ยังไง
- ถ้าลืมรหัสผ่านจะกู้คืนยังไง

ถ้าไฟล์นี้เข้าใจแล้ว จะอ่าน controller ตัวอื่นง่ายขึ้นมาก

---

## 19. สอนอ่าน `HomeController.cs`

ไฟล์นี้ใช้แสดงข้อมูลหน้าร้านเป็นหลัก

เมธอดที่ควรโฟกัสคือ

- `Home()`
- `Menu()`
- `Promotion()`

### 1. `Home()`

อ่านแล้วให้มองว่า

- ระบบเอาโปรโมชันสาธารณะมาใส่ `ViewBag.PublicPromotions`
- เอาสินค้าเด่นมาใส่ `ViewBag.FeaturedProducts`
- แล้วส่งไปหน้า `Home.cshtml`

สรุป:

`Home()` คือเมธอดแสดงหน้าแรกของร้าน

### 2. `Menu()`

ดูว่า

- ดึงสินค้าจาก `Stocks`
- `Include(s => s.Category)` เพื่อเอาหมวดหมู่มาด้วย
- กรองเฉพาะสินค้าที่ `IsAvailable`
- เรียงลำดับก่อนส่งไปหน้า view

สรุป:

`Menu()` คือเมธอดแสดงรายการสินค้าที่พร้อมขาย

### 3. `Promotion()`

ดูว่า

1. เรียก `GetPublicPromotions()`
2. หา `RewardProductId` ของโปรโมชันที่มีของแถม
3. ดึงชื่อสินค้าของรางวัลจาก `Stocks`
4. รวมข้อมูลทั้งหมดเป็น `UserPromotionPageViewModel`

สรุป:

`Promotion()` คือเมธอดแสดงหน้าโปรโมชัน โดยรวมทั้งโปรทั่วไปและโปรกิจกรรมไว้ด้วยกัน

### เมธอดเสริมที่ควรอ่าน

#### `GetPublicPromotions()`

เมธอดนี้สำคัญ เพราะเป็นตัวกลางที่ใช้คัดว่าโปรโมชันไหน “ควรแสดงให้ผู้ใช้เห็น”

เงื่อนไขหลักคือ

- `IsActive`
- มีรูปภาพ
- อยู่ในช่วงวันที่ใช้งานได้

### สรุปไฟล์ `HomeController`

ไฟล์นี้คือประตูสู่ข้อมูลหน้าร้าน

- `Home` = หน้าแรก
- `Menu` = รายการสินค้า
- `Promotion` = รายการโปรโมชัน

อ่านไฟล์นี้แล้วจะเข้าใจว่า “ข้อมูลหน้าร้านมาจากไหน”

---

## 20. สอนอ่าน `ProfileController.cs`

ไฟล์นี้เกี่ยวกับข้อมูลส่วนตัวของผู้ใช้

เมธอดหลักคือ

- `Profile(int id = 0)`
- `Profile([FromBody] ProfileViewModel model)`
- `SaveAddress(...)`
- `DeleteAddress(...)`
- `GetUserAddresses(...)`

### 1. `GET Profile(int id = 0)`

เวลาอ่านให้ดู flow นี้

1. ถ้าไม่ได้ส่ง `id` มา ให้ไปอ่าน `UserId` จาก session
2. ถ้าไม่มี user -> redirect ไป `Login`
3. ดึงข้อมูล `Users`
4. ดึงข้อมูล `Addresses`
5. รวมเป็น `ProfileViewModel`

สรุป:

ใช้เปิดหน้าโปรไฟล์ของผู้ใช้ปัจจุบัน

### 2. `POST Profile(...)`

เวลาอ่านให้ดูว่า

- อัปเดตข้อมูลพื้นฐาน เช่น `Username`, `Email`, `Phone`
- ถ้าจะเปลี่ยนรหัสผ่าน ต้องผ่านเงื่อนไข
  - กรอกรหัสเดิม
  - รหัสเดิมต้องถูก
  - รหัสใหม่ตรงกับยืนยันรหัส
  - รหัสใหม่ยาวพอ

สรุป:

เมธอดนี้ใช้บันทึกข้อมูลโปรไฟล์ และรองรับการเปลี่ยนรหัสผ่าน

### 3. `SaveAddress(...)`

ให้ดูว่า

- ถ้ามี `addressId` -> แก้ไขที่อยู่เดิม
- ถ้าไม่มี -> เพิ่มที่อยู่ใหม่

### 4. `DeleteAddress(...)`

ลบที่อยู่ โดยเช็กก่อนว่าที่อยู่นั้นเป็นของ user คนนี้จริงไหม

### 5. `GetUserAddresses(...)`

ดึงรายการที่อยู่ของ user กลับไปเป็น `Json`

### สรุปไฟล์ `ProfileController`

ไฟล์นี้ตอบคำถามว่า

- ผู้ใช้แก้ข้อมูลตัวเองยังไง
- ผู้ใช้เปลี่ยนรหัสผ่านยังไง
- ผู้ใช้จัดการที่อยู่ยังไง

---

## 21. สอนอ่าน `OrderController.cs`

ไฟล์นี้เป็นแกนหลักของระบบขาย

เมธอดสำคัญคือ

- `Checkout()`
- `CreateOrder(...)`
- `GetCurrentUser()`
- `GetUserPromos(int userId)`
- `SubmitPromotionClaim(...)`
- `Payment(int orderId)`
- `UploadSlip(...)`

### 1. `Checkout()`

โค้ดส่วนนี้ไม่ซับซ้อน

- เช็กก่อนว่า login หรือยัง
- ถ้า login แล้วค่อยเปิดหน้า checkout
- ส่ง `CurrentUserId` ไปให้หน้า view ผ่าน `ViewBag`

### 2. `CreateOrder(...)`

เมธอดนี้สำคัญมาก ให้แบ่งอ่านเป็น 5 ช่วง

#### ช่วงที่ 1: เช็กสิทธิ์และเช็กข้อมูล

- login หรือยัง
- model มีสินค้าไหม

#### ช่วงที่ 2: เช็กโปรโมชัน

- ถ้ามี `PromotionId` ให้ดึงโปรโมชัน
- เช็กว่าโปรยังใช้งานได้ไหม
- ถ้าเป็นโปรซื้อแถม ให้เช็กจำนวนสินค้าที่ซื้อ

#### ช่วงที่ 3: สร้างหัวออเดอร์

สร้าง `Order` ใหม่ แล้วบันทึกก่อนเพื่อให้ได้ `OrderId`

#### ช่วงที่ 4: สร้างรายการสินค้า

วน `model.Items`

- หา product
- เช็กว่าสินค้าพร้อมขายไหม
- เพิ่มรายการลง `Orderdetails`

ถ้าเป็นโปรของแถม ก็เพิ่มรายการของแถมลง `Orderdetails` ด้วย

#### ช่วงที่ 5: ปิดงานหลังสร้างออเดอร์

- อัปเดต `UserPromotions` ว่าใช้ไปแล้ว
- สร้าง notification
- ส่ง `Json` กลับพร้อม `orderId`

สรุป:

`CreateOrder` คือหัวใจของการสั่งซื้อทั้งหมด

### 3. `GetCurrentUser()`

เมธอดสั้น ๆ เอาไว้ให้หน้าเว็บถามว่า “ตอนนี้ user ที่ login อยู่คือใคร”

### 4. `GetUserPromos(int userId)`

ใช้ดึงสิทธิ์โปรโมชันของ user ที่ยังไม่ได้ใช้

จุดสำคัญคือ

- เช็กว่า user ที่ขอมาตรงกับ session จริงไหม
- ดึงจาก `UserPromotions`
- รวมข้อมูลกับ `Promotion`
- กรองเฉพาะโปรที่ยังใช้งานได้

### 5. `SubmitPromotionClaim(...)`

ใช้สำหรับโปรกิจกรรมที่ต้องแนบหลักฐาน

เวลาอ่านให้ดูเป็นลำดับ

1. เช็ก login
2. เช็กว่าโปรนี้เป็นโปรกิจกรรมจริงไหม
3. เช็กว่ามีรูปไหม
4. เช็กขนาดไฟล์และนามสกุล
5. เช็กว่ามี claim ค้างอยู่ไหม
6. เช็กว่า user เคยได้โปรนี้ไปแล้วหรือยัง
7. บันทึกรูปลงโฟลเดอร์
8. สร้าง `PromotionClaim`
9. สร้าง notification

### 6. `Payment(int orderId)`

อ่านแล้วให้มองว่า

- หา order ของ user คนปัจจุบัน
- สร้าง PromptPay payload
- สร้าง QR Code
- ส่งข้อมูลไปหน้า payment

### 7. `UploadSlip(...)`

ใช้รับสลิปการโอนเงิน

อ่านเป็นลำดับ

1. เช็ก login
2. หา order ของ user คนนั้น
3. เช็กว่ามีไฟล์ไหม
4. เซฟไฟล์ลง `wwwroot/uploads/slips`
5. อัปเดต `SlipImagePath`
6. เปลี่ยน `PaymentStatus = PendingVerify`
7. สร้าง notification

### เมธอดช่วย

- `IsPromotionAvailable(...)`
- `GetCurrentUserId()`

สองตัวนี้ช่วยให้โค้ดหลักอ่านง่ายขึ้น

### สรุปไฟล์ `OrderController`

ไฟล์นี้ตอบคำถามว่า

- ผู้ใช้เริ่มสั่งซื้อยังไง
- ระบบสร้าง order ยังไง
- ระบบดึงโปรโมชันไปใช้ยังไง
- ระบบจ่ายเงินและอัปโหลดสลิปยังไง

ถ้าเข้าใจไฟล์นี้ จะเข้าใจ flow ธุรกิจหลักของโปรเจกต์

---

## 22. สอนอ่าน `DeliveryController.cs`

ไฟล์นี้คือปลายทางของออเดอร์

เมธอดหลักคือ

- `Delivery()`
- `CompleteOrder(int orderId)`

### 1. `Delivery()`

เมธอดนี้ยาว แต่ให้แบ่งอ่านเป็น 4 ส่วน

#### ส่วนที่ 1: หา user จาก session

ถ้าไม่มี user -> ส่ง `DeliveryTrackingViewModel` ว่างกลับไป

#### ส่วนที่ 2: หา active order

หาออเดอร์ที่ยังไม่ `Completed`

พร้อมดึง

- `Address`
- `Promotion`
- `Orderdetails`
- `Product`

#### ส่วนที่ 3: หาประวัติ completed orders

ดึงออเดอร์ที่ completed แล้ว และดึง `Historyorders`

#### ส่วนที่ 4: สร้าง ViewModel

เรียก `BuildDeliveryTrackingViewModel(...)`

เมธอดนี้คือจุดสำคัญ เพราะทำหน้าที่แปลงข้อมูลจากฐานข้อมูลให้อยู่ในรูปที่หน้าเว็บใช้ได้ทันที

### 2. `CompleteOrder(int orderId)`

อ่านแล้วให้มองแบบนี้

1. เช็ก user จาก session
2. หา order ของ user คนนั้น
3. เช็กว่า order อยู่สถานะ `Shipped` หรือยัง
4. ถ้ายังไม่มีประวัติ -> สร้าง `Historyorder`
5. เปลี่ยน `order.Status = Completed`
6. บันทึกผล

### เมธอดช่วยที่ควรอ่านผ่าน ๆ

- `BuildDeliveryTrackingViewModel(...)`
- `BuildDeliveryAddress(...)`
- `BuildItemSummary(...)`
- `BuildHistoryLineItems(...)`
- `ParseHistoryLineItems(...)`
- `BuildDiscountDisplay(...)`

ไม่ต้องจำทั้งหมด แต่ให้รู้ว่าพวกนี้คือ “เมธอดแปลงข้อมูล” เพื่อให้หน้า Delivery แสดงผลสวยและครบ

### สรุปไฟล์ `DeliveryController`

ไฟล์นี้ตอบคำถามว่า

- ผู้ใช้ติดตามสถานะออเดอร์ยังไง
- ระบบแยกออเดอร์ปัจจุบันกับประวัติยังไง
- ออเดอร์ถูกปิดงานและย้ายเข้าประวัติยังไง

---

## 23. สอนอ่าน `NotificationController.cs`

ไฟล์นี้สั้น แต่สำคัญ เพราะเชื่อมการแจ้งเตือนของทั้งระบบ

เมธอดหลักคือ

- `GetMyNotifications()`
- `MarkAsRead(int id)`
- `MarkAllAsRead()`
- `ClearMyNotifications()`

### 1. `GetMyNotifications()`

ใช้ดึงรายการแจ้งเตือนของ user ปัจจุบัน

ให้ดูว่า

- เช็ก user จาก session
- ดึง `Notifications` ของ user คนนั้น
- เรียงจากใหม่ไปเก่า
- เอาแค่ 30 รายการ
- ส่งกลับเป็น `Json`

### 2. `MarkAsRead(int id)`

ใช้เปลี่ยนแจ้งเตือน 1 รายการเป็นอ่านแล้ว

### 3. `MarkAllAsRead()`

ใช้เปลี่ยนแจ้งเตือนที่ยังไม่อ่านทั้งหมดเป็นอ่านแล้ว

### 4. `ClearMyNotifications()`

ใช้ลบแจ้งเตือนทั้งหมดของ user คนนั้น

### เมธอดช่วย

- `GetCurrentUserId()`

ตัวนี้เหมือนแนวคิดเดียวกับหลายไฟล์ คืออ่าน `UserId` จาก session

### สรุปไฟล์ `NotificationController`

ไฟล์นี้ไม่ได้ซับซ้อน แต่สำคัญเพราะเป็นตัวเชื่อมผลลัพธ์ของหลายระบบ เช่น

- สร้างออเดอร์
- อัปโหลดสลิป
- ยืนยันชำระเงิน
- อนุมัติโปรโมชัน

---

## 24. สอนอ่าน `AdminOrderController.cs`

ไฟล์นี้เป็นหลังบ้านที่เชื่อมต่อจาก flow ออเดอร์โดยตรง

เมธอดหลักคือ

- `Order()`
- `ConfirmPayment(int orderId)`
- `GetOrderDetails(int orderId)`
- `AcceptOrder(int orderId)`
- `ShipOrder(int orderId)`

### 1. `Order()`

ใช้เปิดหน้ารายการออเดอร์ทั้งหมด

ให้ดูว่า

- เช็กสิทธิ์ด้วย `IsCurrentUserAdminOrStaff()`
- ดึง `Orders`
- `Include` ข้อมูล `User`, `Promotion`, `Orderdetails`, `Product`
- เรียงจากใหม่ไปเก่า

### 2. `ConfirmPayment(int orderId)`

นี่คือเมธอดสำคัญมากของหลังบ้าน

อ่านเป็นลำดับ

1. เช็กสิทธิ์
2. หา order พร้อมรายการสินค้า
3. วนลด stock ของสินค้าแต่ละตัว
4. เปลี่ยน `PaymentStatus = Paid`
5. เปลี่ยน `Status = Paid`
6. สร้าง notification ให้ลูกค้า

สรุป:

เมธอดนี้คือจุดที่การชำระเงิน “ถูกยืนยันจริง” และระบบเริ่มตัด stock

### 3. `GetOrderDetails(int orderId)`

ใช้ดึงรายละเอียดออเดอร์แบบ `Json`

เวลาอ่านให้ดูว่า

- ดึงข้อมูลลูกค้า
- ดึงสินค้าแต่ละรายการ
- ดึงข้อมูลโปรโมชัน
- คำนวณจำนวนสินค้าปกติกับของแถม

เมธอดนี้สำคัญเพราะใช้กับ modal หรือรายละเอียดเพิ่มเติมบนหน้า order

### 4. `AcceptOrder(int orderId)`

ใช้เปลี่ยนสถานะออเดอร์เป็น `Preparing`

ดูว่า

- ต้องเป็น admin/staff
- ถ้า order ถูกทำไปแล้วจะไม่ให้รับซ้ำ
- ถ้าผ่านก็เปลี่ยนสถานะและแจ้งลูกค้า

### 5. `ShipOrder(int orderId)`

ใช้เปลี่ยนสถานะจาก `Preparing` เป็น `Shipped`

ดูว่า

- order ต้องอยู่สถานะ `Preparing` ก่อน
- ถ้าผ่านก็เปลี่ยนสถานะและแจ้งลูกค้า

### สรุปไฟล์ `AdminOrderController`

ไฟล์นี้ตอบคำถามว่า

- หลังบ้านดูออเดอร์ยังไง
- หลังบ้านยืนยันการชำระเงินยังไง
- หลังบ้านรับงานและจัดส่งยังไง

---

## 25. สอนอ่าน `AdminStockController.cs`

ไฟล์นี้ใช้จัดการสินค้า สต็อก และหมวดหมู่

เมธอดหลักคือ

- `Stock()`
- `UpdateStockSettings(...)`
- `SaveCategory(...)`
- `SaveStock(...)`

### 1. `Stock()`

ใช้เปิดหน้าจัดการสินค้าและสต็อก

ดูว่า

- admin/staff เข้าได้
- ดึง `Categories` พร้อมสินค้าในหมวด
- ดึง `Products` พร้อม `Category`
- รวมเป็น `AdminStockViewModel`

### 2. `UpdateStockSettings(...)`

ใช้ปรับจำนวนสต็อกและสถานะการขาย

ดูว่า

- admin/staff ใช้ได้
- หา product ตาม `productId`
- ปรับ `Stock1`
- อัปเดต `IsAvailable`
- อัปเดต `IsLimitedQuantity`

### 3. `SaveCategory(...)`

ใช้เพิ่มหรือแก้ไขหมวดหมู่

จุดสำคัญคือ

- มีสิทธิ์เฉพาะ admin
- ถ้ามี `categoryId` = แก้ไข
- ถ้าไม่มี = เพิ่มใหม่

### 4. `SaveStock(...)`

ใช้เพิ่มหรือแก้ไขสินค้า

เวลาอ่านให้แบ่งเป็นช่วง

1. เช็กสิทธิ์ admin
2. ตรวจข้อมูลสินค้า
   - ชื่อ
   - หมวดหมู่
   - ราคา
   - จำนวน
   - ประเภทไฟล์รูป
3. ถ้ามี `productId` = แก้ไขของเดิม
4. ถ้าไม่มี = เพิ่มสินค้าใหม่
5. ถ้ามีรูป -> บันทึกรูปด้วย `SaveProductImageAsync(...)`

### เมธอดช่วย

- `IsSupportedImage(...)`
- `SaveProductImageAsync(...)`

สองตัวนี้ช่วยเรื่องตรวจไฟล์และเซฟไฟล์รูปสินค้า

### สรุปไฟล์ `AdminStockController`

ไฟล์นี้ตอบคำถามว่า

- หลังบ้านดู stock ยังไง
- staff ปรับ stock ยังไง
- admin เพิ่มหมวดหมู่และสินค้าใหม่ยังไง

---

## 26. สอนอ่าน `AdminPromotionController.cs`

ไฟล์นี้เป็นอีกไฟล์ที่สำคัญและค่อนข้างยาว

เมธอดหลักคือ

- `PromotionAdmin()`
- `EventPromotion()`
- `SavePromotion(...)`
- `GiftPromotion(...)`
- `GiftPromotionToAll(...)`
- `ApproveClaim(...)`
- `RejectClaim(...)`

### 1. `PromotionAdmin()`

ใช้เปิดหน้าจัดการโปรโมชันหลัก

ดูว่า

- admin เท่านั้น
- เรียก `BuildPromotionViewModel()`
- ส่ง model ไปหน้า `PromotionAdmin.cshtml`

### 2. `EventPromotion()`

ใช้เปิดหน้าตรวจคำขอโปรโมชันกิจกรรม

ดูว่า

- admin/staff ใช้ได้
- ใช้ `BuildPromotionViewModel()` เหมือนกัน
- แต่ไปคนละหน้า view

### 3. `SavePromotion(...)`

เมธอดนี้เป็นหัวใจของระบบโปรโมชัน

เวลาอ่านอย่าอ่านรวดเดียว ให้แบ่งเป็นช่วง

#### ช่วงที่ 1: validation พื้นฐาน

- เช็กชื่อโปรโมชัน
- เช็กประเภทโปรโมชัน
- เช็กวันเริ่มและวันสิ้นสุด

#### ช่วงที่ 2: แยกตามประเภทโปรโมชัน

- `promoType == 1` = โปรส่วนลด
- `promoType == 2` = โปรซื้อแถม
- `promoType == 3` = โปรกิจกรรม

#### ช่วงที่ 3: เตรียมข้อมูลก่อนบันทึก

- จัดรูปแบบ discount type
- parse ของแถมจาก `rewardItemsJson`
- เตรียมรูปภาพ

#### ช่วงที่ 4: แก้ไขหรือเพิ่มใหม่

- ถ้ามี `promotionId` = แก้ไข
- ถ้าไม่มี = เพิ่มใหม่

#### ช่วงที่ 5: อัปเดตรายการของรางวัล

ดู `ReplaceRewardItems(...)`

สรุป:

`SavePromotion` คือเมธอดที่ทำให้เราเห็นว่าระบบรองรับโปรหลายแบบในฟังก์ชันเดียว

### 4. `GiftPromotion(...)`

ใช้มอบโปรโมชันให้ user 1 คน

ดูว่า

- หา `promotion`
- หา `user`
- เช็กว่าโปรยังใช้ได้ไหม
- เพิ่มหรือรีเซ็ตสิทธิ์ใน `UserPromotions`
- สร้าง notification

### 5. `GiftPromotionToAll(...)`

ใช้แจกโปรให้ผู้ใช้ทั้งหมด

ความต่างจาก `GiftPromotion(...)` คือ

- ดึง user ทุกคน
- วนลูปมอบสิทธิ์ให้ทีละคน
- ส่ง notification แบบหลายคน

### 6. `ApproveClaim(...)`

ใช้อนุมัติคำขอรับโปรกิจกรรม

ดูว่า

- เช็ก claim
- เช็กสถานะว่าเป็น `Pending`
- เพิ่มสิทธิ์ใน `UserPromotions`
- เปลี่ยน claim เป็น `Approved`
- บันทึกคนอนุมัติ
- สร้าง notification ให้ user

### 7. `RejectClaim(...)`

ใช้ปฏิเสธคำขอ

ดูว่า

- เปลี่ยน status เป็น `Rejected`
- บันทึกเหตุผล
- สร้าง notification กลับไปให้ user

### เมธอดช่วยที่ควรอ่านผ่าน ๆ

- `ParseRewardItems(...)`
- `ReplaceRewardItems(...)`
- `BuildPromotionViewModel()`
- `RedirectToPromotionPage(...)`
- `NormalizeDiscountType(...)`

ไม่ต้องจำทุกบรรทัด แต่ต้องรู้ว่าเมธอดพวกนี้มีไว้ “ช่วยให้เมธอดหลักสั้นลง”

### สรุปไฟล์ `AdminPromotionController`

ไฟล์นี้ตอบคำถามว่า

- หลังบ้านสร้างโปรยังไง
- แจกโปรยังไง
- ตรวจ claim โปรกิจกรรมยังไง

---

## 27. สอนอ่าน `AdminMemberController.cs`

ไฟล์นี้เกี่ยวกับการจัดการสมาชิกในระบบ

เมธอดหลักคือ

- `Member()`
- `SaveMember(...)`
- `DeleteMember(int userId)`

### 1. `Member()`

ใช้เปิดหน้าจัดการสมาชิก

ดูว่า

- admin เท่านั้น
- ดึง `Roles` ไปใส่ `ViewBag`
- ดึง `Users` พร้อม `Role`
- ส่งไปหน้า `Member.cshtml`

### 2. `SaveMember(...)`

ใช้ทั้งเพิ่มและแก้ไข user

ให้แบ่งอ่านเป็น 3 ช่วง

#### ช่วงที่ 1: validation

- เช็ก `username`
- เช็ก `roleId`

#### ช่วงที่ 2: กรณีแก้ไข

ถ้ามี `userId > 0`

- หา user เดิม
- อัปเดตข้อมูล
- ถ้ามีรหัสผ่านใหม่ ก็เปลี่ยนรหัสผ่าน

#### ช่วงที่ 3: กรณีเพิ่มใหม่

ถ้าไม่มี `userId`

- ต้องมีรหัสผ่าน
- สร้าง user ใหม่

สรุป:

เมธอดเดียวรองรับทั้งเพิ่มและแก้ไขสมาชิก

### 3. `DeleteMember(int userId)`

ใช้ลบผู้ใช้

จุดสำคัญคือ

- admin เท่านั้น
- ป้องกันไม่ให้ admin ลบบัญชีตัวเอง
- ถ้าพบ user ค่อยลบจริง

### สรุปไฟล์ `AdminMemberController`

ไฟล์นี้ตอบคำถามว่า

- admin ดูสมาชิกทั้งหมดได้ยังไง
- admin เพิ่มหรือแก้ไขสิทธิ์ผู้ใช้ยังไง
- admin ลบผู้ใช้ยังไง

---

## 28. สอนอ่าน `AdminDashboardController.cs`

ไฟล์นี้เป็นหน้าสรุปของระบบ

เมธอดหลักจริง ๆ มีตัวเดียว คือ

- `Dashbordadmin()`

### วิธีอ่านเมธอดนี้

ให้แบ่งเป็น 4 ช่วง

#### ช่วงที่ 1: เช็กสิทธิ์

- admin เท่านั้น

#### ช่วงที่ 2: เตรียมข้อมูลพื้นฐาน

- กำหนด `today`
- กำหนด `monthStart`
- ดึง `orders`
- ดึง `stocks`

#### ช่วงที่ 3: คำนวณ KPI

ดูพวกค่าใน `AdminDashboardViewModel` เช่น

- `TodayRevenue`
- `TodayOrders`
- `PendingVerificationOrders`
- `TotalUsers`
- `LowStockProducts`
- `OutOfStockProducts`
- `ActivePromotions`

#### ช่วงที่ 4: คำนวณข้อมูลสรุปเชิงแสดงผล

- `MonthlyRevenue`
- `RecentOrders`
- `LowStockItems`
- `CategorySummaries`

### สรุปเมธอดนี้แบบง่าย

เมธอดนี้ไม่ได้ “ทำธุรกรรม”

แต่มันทำหน้าที่

- ดึงข้อมูลจากหลายตาราง
- คำนวณให้เป็นตัวเลขสรุป
- รวมเป็น `AdminDashboardViewModel`

### สรุปไฟล์ `AdminDashboardController`

ไฟล์นี้ตอบคำถามว่า

- หลังบ้านมองภาพรวมของร้านยังไง
- ระบบคำนวณรายได้และ KPI ยังไง
- dashboard เอาข้อมูลมาจากตารางไหนบ้าง

---

## 29. สรุปการอ่านทั้งระบบอีกครั้ง

ถ้าจะอ่านให้เข้าใจทั้งโปรเจกต์จริง ๆ ให้จำ flow นี้

1. `AccountController`
2. `HomeController`
3. `ProfileController`
4. `OrderController`
5. `DeliveryController`
6. `NotificationController`
7. `AdminControllerBase`
8. `AdminOrderController`
9. `AdminStockController`
10. `AdminPromotionController`
11. `AdminMemberController`
12. `AdminDashboardController`

เหตุผลคือ

- ชุดแรก = ฝั่งลูกค้า
- ชุดหลัง = ฝั่งหลังบ้าน
- และ dashboard อ่านท้ายสุด เพราะเป็นหน้าสรุป

---

## 30. วิธีเช็กว่าตัวเองเริ่มเข้าใจแล้วหรือยัง

หลังอ่านแต่ละไฟล์ ลองสรุปตัวเอง 3 บรรทัด

1. ไฟล์นี้ดูแลเรื่องอะไร
2. เมธอดหลักมีอะไรบ้าง
3. เมธอดที่สำคัญที่สุดคืออะไร และมันทำอะไร

ตัวอย่าง

- `AccountController` = ดูแล login สมัครสมาชิก และลืมรหัสผ่าน
- เมธอดหลักคือ `Login`, `Signup`, `RequestPasswordReset`, `ResetPassword`
- เมธอดสำคัญสุดคือ `Login` เพราะเป็นจุดที่ระบบสร้าง session และแยก role

ถ้าสรุปได้แบบนี้ แปลว่าเริ่มอ่าน controller เป็นแล้ว
