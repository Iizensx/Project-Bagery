# อ่านโค้ด Controller ทั้งหมด

ไฟล์นี้ทำขึ้นเพื่อใช้เป็นแนวทางอ่านโค้ด `Controller` ของโปรเจกต์แบบค่อย ๆ ไล่จากง่ายไปยาก โดยเน้นให้อ่านแล้วเข้าใจว่าแต่ละไฟล์ทำหน้าที่อะไร ไม่ใช่แค่อ่านผ่าน ๆ แล้วจำชื่อฟังก์ชันอย่างเดียว

แนวคิดหลักของการอ่าน `Controller` คือ

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
