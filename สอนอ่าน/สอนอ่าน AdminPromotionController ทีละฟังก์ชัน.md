# สอนอ่าน AdminPromotionController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/Admin/AdminPromotionController.cs`  
ไฟล์นี้ดูแลงานโปรโมชันของระบบแทบทั้งหมด

---

## AdminPromotionController มีหน้าที่อะไร

- เปิดหน้าจัดการโปรโมชัน
- เปิดหน้าตรวจคำขอโปรโมชัน event
- เพิ่มหรือแก้ไขโปรโมชัน
- มอบโปรโมชันให้ user คนเดียวหรือทุกคน
- อนุมัติหรือปฏิเสธคำขอ claim

---

## 1. Constructor

```csharp
public AdminPromotionController(BakerydbContext db, IWebHostEnvironment environment) : base(db)
{
    _environment = environment;
}
```

### ฟังก์ชันนี้ทำอะไร

รับฐานข้อมูลกับ environment สำหรับใช้ตอนบันทึกรูปโปรโมชัน

---

## 2. ฟังก์ชัน PromotionAdmin()

```csharp
public IActionResult PromotionAdmin()
{
    if (!IsCurrentUserAdmin())
        return RedirectToAdminLogin();

    var model = BuildPromotionViewModel();
    return View("~/Views/admin/PromotionAdmin.cshtml", model);
}
```

### ฟังก์ชันนี้ทำอะไร

เปิดหน้าจัดการโปรโมชันหลักของ admin

---

## 3. ฟังก์ชัน EventPromotion()

```csharp
public IActionResult EventPromotion()
{
    if (!IsCurrentUserAdminOrStaff())
        return RedirectToAdminLogin();

    ViewBag.Title = "Event Promotion";
    ViewBag.PageTitle = "Event Promotion";

    var model = BuildPromotionViewModel();
    return View("~/Views/admin/EventPromotion.cshtml", model);
}
```

### ฟังก์ชันนี้ทำอะไร

เปิดหน้าดูคำขอโปรโมชัน event  
admin และ staff เข้าได้

---

## 4. ฟังก์ชัน SavePromotion(...)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SavePromotion(...)
{
    if (!IsCurrentUserAdmin())
        return RedirectToAdminLogin();

    ...
}
```

### ฟังก์ชันนี้ทำอะไร

เป็นฟังก์ชันหลักสำหรับเพิ่มหรือแก้ไขโปรโมชัน

### โครงสร้างความคิด

1. เช็กสิทธิ์ admin
2. ตรวจข้อมูลพื้นฐาน เช่น ชื่อโปรโมชัน ประเภท ช่วงเวลา
3. แยก validation ตาม `promoType`
4. ถ้ามีรูปใหม่ ให้บันทึกรูป
5. ถ้า `promotionId > 0` = แก้โปรโมชันเดิม
6. ถ้าไม่มี = เพิ่มโปรโมชันใหม่
7. อัปเดตรายการของแถมผ่าน `ReplaceRewardItems(...)`

### จุดที่ควรจำ

- `promoType = 1` มักเป็นส่วนลด
- `promoType = 2` มักเป็นซื้อแถม
- `promoType = 3` มักเป็น event / claim

### สรุปสั้น ๆ

`SavePromotion()` คือหัวใจของไฟล์นี้

---

## 5. ฟังก์ชัน GiftPromotion(int promotionId, int userId)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult GiftPromotion(int promotionId, int userId)
{
    if (!IsCurrentUserAdmin())
        return RedirectToAdminLogin();

    ...
}
```

### ฟังก์ชันนี้ทำอะไร

มอบโปรโมชันให้ผู้ใช้รายบุคคล

### วิธีคิด

1. หา promotion และ user
2. เช็กว่าโปรโมชันยังใช้ได้
3. ถ้ายังไม่มีใน `UserPromotions` ให้เพิ่มใหม่
4. ถ้ามีอยู่แล้ว ให้รีเซ็ตสถานะ `IsUsed`

---

## 6. ฟังก์ชัน GiftPromotionToAll(int promotionId)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult GiftPromotionToAll(int promotionId)
{
    if (!IsCurrentUserAdmin())
        return RedirectToAdminLogin();

    ...
}
```

### ฟังก์ชันนี้ทำอะไร

แจกโปรโมชันให้ผู้ใช้ทุกคนในระบบ

### สรุปสั้น ๆ

วนทุก user แล้วเพิ่มหรือรีเซ็ตข้อมูลใน `UserPromotions`

---

## 7. ฟังก์ชัน ApproveClaim(int claimId, string? returnTo)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ApproveClaim(int claimId, string? returnTo)
{
    if (!IsCurrentUserAdminOrStaff())
        return RedirectToAdminLogin();

    ...
}
```

### ฟังก์ชันนี้ทำอะไร

อนุมัติคำขอ claim โปรโมชัน event

### วิธีคิด

1. หา claim ที่ยัง `Pending`
2. ถ้ายังไม่มีโปรโมชันนี้ใน `UserPromotions` ให้เพิ่มให้ user
3. เปลี่ยนสถานะ claim เป็น `Approved`
4. บันทึกผู้ตรวจและเวลาตรวจ

---

## 8. ฟังก์ชัน RejectClaim(int claimId, string? reviewNote, string? returnTo)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult RejectClaim(int claimId, string? reviewNote, string? returnTo)
{
    if (!IsCurrentUserAdminOrStaff())
        return RedirectToAdminLogin();

    ...
}
```

### ฟังก์ชันนี้ทำอะไร

ปฏิเสธคำขอ claim โปรโมชัน event

### สรุปสั้น ๆ

เปลี่ยนสถานะ claim เป็น `Rejected` แล้วเก็บเหตุผลไว้ใน `ReviewNote`

---

## 9. ฟังก์ชัน ParseRewardItems(...)

```csharp
private List<PromotionRewardItemInput> ParseRewardItems(string? rewardItemsJson, int? rewardProductId, int? rewardQuantity)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

แปลงข้อมูลของแถมจาก JSON หรือจากค่าฟอร์ม ให้กลายเป็น list ที่ระบบใช้ได้

---

## 10. ฟังก์ชัน ReplaceRewardItems(...)

```csharp
private void ReplaceRewardItems(int promotionId, List<PromotionRewardItemInput> rewardItems)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

ลบ reward item เดิมของโปรโมชันนั้น แล้วเพิ่มรายการใหม่แทน

---

## 11. ฟังก์ชัน BuildPromotionViewModel()

```csharp
private AdminPromotionViewModel BuildPromotionViewModel()
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

รวบรวมข้อมูลทั้งหมดที่หน้า `PromotionAdmin` และ `EventPromotion` ต้องใช้ เช่น

- รายการโปรโมชัน
- รายชื่อผู้ใช้
- รายการสินค้า
- คำขอ claim ที่รออนุมัติ
- จำนวนการแจกและการใช้งานโปรโมชัน

---

## 12. ฟังก์ชัน RedirectToPromotionPage(string? returnTo)

```csharp
private IActionResult RedirectToPromotionPage(string? returnTo)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

ช่วยตัดสินใจว่าจะ redirect กลับหน้า `PromotionAdmin` หรือ `EventPromotion`

---

## 13. ฟังก์ชัน NormalizeDiscountType(string? discountType)

```csharp
private static string? NormalizeDiscountType(string? discountType)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

ปรับชื่อชนิดส่วนลดให้เหลือรูปแบบที่ระบบใช้จริง เช่น `Percent` หรือ `Fixed`

---

## 14. ฟังก์ชัน IsPromotionAvailable(Promotion promotion)

```csharp
private static bool IsPromotionAvailable(Promotion promotion)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

ตรวจว่าโปรโมชันยังเปิดใช้งานและอยู่ในช่วงเวลาใช้งานหรือไม่

---

## 15. ฟังก์ชัน SaveImageAsync(...)

```csharp
private async Task<string> SaveImageAsync(IFormFile file, string folderName)
{
    ...
}
```

### ฟังก์ชันนี้ทำอะไร

บันทึกรูปโปรโมชันลงใน `wwwroot/uploads/...` แล้วคืน path กลับมา

---

## สรุปภาพรวมของ AdminPromotionController

- `PromotionAdmin()` เปิดหน้าจัดการโปรโมชัน
- `EventPromotion()` เปิดหน้าตรวจ claim โปรโมชัน event
- `SavePromotion()` เพิ่มหรือแก้ไขโปรโมชัน
- `GiftPromotion()` แจกโปรให้ user คนเดียว
- `GiftPromotionToAll()` แจกโปรให้ทุกคน
- `ApproveClaim()` และ `RejectClaim()` ใช้จัดการคำขอ claim
