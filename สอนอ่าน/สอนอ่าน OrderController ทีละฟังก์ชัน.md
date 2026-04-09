# สอนอ่าน OrderController ทีละฟังก์ชัน

ไฟล์นี้ใช้สำหรับฝึกอ่าน `Controllers/OrderController.cs` แบบทีละเมธอด  
`OrderController` เป็นหนึ่งในไฟล์สำคัญที่สุดของระบบ เพราะเกี่ยวกับ flow การสั่งซื้อโดยตรง

---

## OrderController มีหน้าที่อะไร

ไฟล์นี้ดูแลเรื่อง

- เปิดหน้า checkout
- สร้างออเดอร์
- ดึงโปรโมชันของผู้ใช้
- รับคำขอ claim โปรโมชัน
- เปิดหน้าชำระเงิน
- อัปโหลดสลิป

สรุปง่าย ๆ คือเป็น controller ที่เชื่อม "ตะกร้า -> ออเดอร์ -> การจ่ายเงิน"

---

## วิธีอ่านไฟล์นี้

เวลาอ่าน `OrderController` ให้จับ flow ตามนี้

1. `Checkout()`
2. `CreateOrder(...)`
3. `Payment(orderId)`
4. `UploadSlip(...)`

และค่อยดูฟังก์ชันเสริมอย่าง `GetUserPromos()` หรือ `SubmitPromotionClaim()`

---

## 1. Constructor

```csharp
public OrderController(BakerydbContext db)
{
    _db = db;
}
```

### ฟังก์ชันนี้ทำอะไร

รับฐานข้อมูลมาเก็บไว้ใน `_db`

---

## 2. ฟังก์ชัน Checkout()

```csharp
public IActionResult Checkout()
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId <= 0)
        return RedirectToAction("Login", "Account");

    ViewBag.CurrentUserId = currentUserId;
    return View("~/Views/Account/Checkout.cshtml");
}
```

### ฟังก์ชันนี้ทำอะไร

เปิดหน้า checkout

### จุดสำคัญ

```csharp
var currentUserId = GetCurrentUserId();
```

ดึง `UserId` จาก session เพื่อเช็กว่าล็อกอินอยู่ไหม

```csharp
if (currentUserId <= 0)
    return RedirectToAction("Login", "Account");
```

ถ้ายังไม่ login จะไม่ให้เข้าหน้า checkout

### สรุปสั้น ๆ

`Checkout()` = เช็ก login ก่อน แล้วค่อยเปิดหน้าชำระรายการสั่งซื้อ

---

## 3. ฟังก์ชัน CreateOrder(...)

```csharp
[HttpPost]
public IActionResult CreateOrder([FromBody] OrderRequest model)
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId <= 0)
        return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อนชำระเงิน" });

    if (model == null || model.Items == null || !model.Items.Any())
        return Json(new { success = false, message = "ข้อมูลออเดอร์ไม่ถูกต้อง" });

    Promotion? selectedPromotion = null;
    var rewardItems = new List<PromotionRewardItem>();
    if (model.PromotionId.HasValue && model.PromotionId > 0)
    {
        selectedPromotion = _db.Promotions.FirstOrDefault(p => p.PromotionId == model.PromotionId.Value);
        if (selectedPromotion == null || !IsPromotionAvailable(selectedPromotion))
            return Json(new { success = false, message = "โปรโมชันนี้ปิดใช้งานหรือหมดเวลาแล้ว" });
    }

    var order = new Order
    {
        UserId = currentUserId,
        AddressId = model.AddressId,
        PromotionId = model.PromotionId,
        TotalAmount = model.TotalAmount,
        OrderDate = DateTime.Now,
        Status = "Pending",
        PaymentStatus = "Pending"
    };

    _db.Orders.Add(order);
    _db.SaveChanges();

    foreach (var item in model.Items)
    {
        var product = item.ProductId.HasValue && item.ProductId.Value > 0
            ? _db.Stocks.FirstOrDefault(s => s.ProductId == item.ProductId.Value)
            : _db.Stocks.FirstOrDefault(s => s.ProductName == item.ProductName);

        if (product == null)
            continue;

        _db.Orderdetails.Add(new Orderdetail
        {
            OrderId = order.OrderId,
            ProductId = product.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.Price
        });
    }

    _db.SaveChanges();

    NotificationHelper.AddNotification(
        _db,
        currentUserId,
        "order",
        "Order created",
        $"Your order #{order.OrderId} was created successfully.",
        Url.Action("Payment", "Order", new { orderId = order.OrderId }),
        "Order",
        order.OrderId);

    _db.SaveChanges();

    return Json(new { success = true, orderId = order.OrderId });
}
```

### ฟังก์ชันนี้ทำอะไร

เป็นหัวใจของระบบสั่งซื้อ  
ใช้สร้างออเดอร์จริงในฐานข้อมูล

### โครงสร้างความคิดของฟังก์ชันนี้

1. เช็กว่าผู้ใช้ login อยู่ไหม
2. เช็กว่าข้อมูลออเดอร์ถูกต้องไหม
3. ถ้ามีโปรโมชัน ให้ตรวจสอบโปรโมชันก่อน
4. สร้าง `Order`
5. เพิ่มรายการสินค้าใน `Orderdetails`
6. บันทึกลงฐานข้อมูล
7. สร้าง notification

### จุดสำคัญที่ควรดู

```csharp
var order = new Order
{
    UserId = currentUserId,
    AddressId = model.AddressId,
    PromotionId = model.PromotionId,
    TotalAmount = model.TotalAmount,
    OrderDate = DateTime.Now,
    Status = "Pending",
    PaymentStatus = "Pending"
};
```

นี่คือจุดที่สร้างหัวออเดอร์

```csharp
_db.Orders.Add(order);
_db.SaveChanges();
```

ต้องบันทึกก่อน เพื่อให้ได้ `OrderId` ไปใช้ใน `Orderdetails`

```csharp
foreach (var item in model.Items)
{
    ...
}
```

วนเพิ่มรายการสินค้าแต่ละชิ้นลงตารางรายละเอียดออเดอร์

### สรุปสั้น ๆ

`CreateOrder()` = สร้างออเดอร์หลัก เพิ่มรายละเอียดสินค้า และแจ้งเตือนผู้ใช้

---

## 4. ฟังก์ชัน GetCurrentUser()

```csharp
[HttpGet]
public IActionResult GetCurrentUser()
{
    var userId = GetCurrentUserId();
    if (userId > 0)
        return Json(new { success = true, userId });

    return Json(new { success = false, userId = 0 });
}
```

### ฟังก์ชันนี้ทำอะไร

ส่ง `UserId` ของคนที่ login อยู่กลับไปให้หน้าเว็บ

---

## 5. ฟังก์ชัน GetUserPromos(int userId)

```csharp
[HttpGet]
public IActionResult GetUserPromos(int userId)
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId <= 0 || currentUserId != userId)
        return Json(new { success = false, promos = Array.Empty<object>() });

    var promos = _db.UserPromotions
        .Include(up => up.Promotion)
        .Where(up => up.UserId == userId && up.IsUsed == 0 && up.Promotion != null)
        .AsEnumerable()
        .Where(up => IsPromotionAvailable(up.Promotion))
        .Select(up => new
        {
            up.PromotionId,
            up.Promotion.PromotionName
        })
        .ToList();

    return Json(new { success = true, promos });
}
```

### ฟังก์ชันนี้ทำอะไร

ดึงโปรโมชันที่ผู้ใช้คนนั้นถืออยู่ และยังใช้ได้

### จุดสำคัญ

```csharp
if (currentUserId <= 0 || currentUserId != userId)
```

กันไม่ให้คนอื่นมาดูโปรโมชันของ user คนอื่น

```csharp
.Where(up => up.UserId == userId && up.IsUsed == 0 && up.Promotion != null)
```

เอาเฉพาะโปรโมชันของ user คนนี้ที่ยังไม่ถูกใช้

### สรุปสั้น ๆ

`GetUserPromos()` = ส่งโปรโมชันที่ user ใช้ได้ตอน checkout

---

## 6. ฟังก์ชัน SubmitPromotionClaim(...)

```csharp
[HttpPost]
public async Task<IActionResult> SubmitPromotionClaim(int promotionId, IFormFile proofImage, string? note)
{
    try
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdString, out var userId) || userId <= 0)
            return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อน" });

        var promotion = _db.Promotions.FirstOrDefault(p => p.PromotionId == promotionId);
        if (promotion == null || promotion.PromoType != 3 || !promotion.RequiresProof || !IsPromotionAvailable(promotion))
            return Json(new { success = false, message = "ไม่พบโปรโมชันอีเวนต์ที่เปิดรับอยู่ตอนนี้" });

        if (proofImage == null || proofImage.Length == 0)
            return Json(new { success = false, message = "กรุณาแนบรูปยืนยัน" });

        var extension = Path.GetExtension(proofImage.FileName);
        if (proofImage.Length > PromotionClaimMaxFileSizeBytes)
            return Json(new { success = false, message = "รูปมีขนาดใหญ่เกินไป" });

        if (string.IsNullOrWhiteSpace(extension) || !SupportedPromotionClaimExtensions.Contains(extension))
            return Json(new { success = false, message = "รองรับเฉพาะไฟล์รูปบางประเภท" });

        var fileName = $"claim_{userId}_{promotionId}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "promotion-claims");
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
            await proofImage.CopyToAsync(stream);

        _db.PromotionClaims.Add(new PromotionClaim
        {
            PromotionId = promotionId,
            UserId = userId,
            ProofImagePath = "/uploads/promotion-claims/" + fileName,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            Status = "Pending",
            RequestedAt = DateTime.Now
        });

        _db.SaveChanges();
        return Json(new { success = true, message = "ส่งรูปยืนยันเรียบร้อย" });
    }
    catch
    {
        return Json(new { success = false, message = "ระบบมีปัญหาระหว่างบันทึกรูป" });
    }
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้ส่งหลักฐานเพื่อ claim โปรโมชันประเภท event

### จุดสำคัญ

- เช็ก login
- เช็กว่าโปรโมชันเป็น event และยังใช้ได้
- เช็กชนิดไฟล์และขนาดไฟล์
- บันทึกรูปลงโฟลเดอร์
- สร้างข้อมูลในตาราง `PromotionClaims`

### สรุปสั้น ๆ

`SubmitPromotionClaim()` = อัปโหลดหลักฐานและสร้างคำขอรับโปรโมชัน event

---

## 7. ฟังก์ชัน Payment(int orderId)

```csharp
public IActionResult Payment(int orderId)
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId <= 0)
        return RedirectToAction("Login", "Account");

    var order = _db.Orders
        .Include(o => o.Orderdetails)
        .ThenInclude(d => d.Product)
        .Include(o => o.Promotion)
        .FirstOrDefault(o => o.OrderId == orderId && o.UserId == currentUserId);

    if (order == null)
        return RedirectToAction("Home", "Home");

    var payload = PromptPayHelper.GeneratePayload("0943253900", order.TotalAmount ?? 0);

    using var qrGenerator = new QRCodeGenerator();
    var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
    using var qrCode = new PngByteQRCode(qrData);
    var qrBytes = qrCode.GetGraphic(10);

    ViewBag.QrBase64 = Convert.ToBase64String(qrBytes);
    ViewBag.OrderId = orderId;
    ViewBag.TotalAmount = order.TotalAmount;

    return View("~/Views/Account/Payment.cshtml", order);
}
```

### ฟังก์ชันนี้ทำอะไร

เปิดหน้าชำระเงินของออเดอร์

### จุดสำคัญ

```csharp
.FirstOrDefault(o => o.OrderId == orderId && o.UserId == currentUserId);
```

กันไม่ให้เปิดออเดอร์ของคนอื่น

```csharp
var payload = PromptPayHelper.GeneratePayload("0943253900", order.TotalAmount ?? 0);
```

สร้างข้อมูลสำหรับ PromptPay

```csharp
ViewBag.QrBase64 = Convert.ToBase64String(qrBytes);
```

แปลง QR เป็น base64 เพื่อให้หน้า view แสดงรูปได้

### สรุปสั้น ๆ

`Payment()` = โหลดออเดอร์แล้วสร้าง QR สำหรับจ่ายเงิน

---

## 8. ฟังก์ชัน UploadSlip(...)

```csharp
[HttpPost]
public async Task<IActionResult> UploadSlip(int orderId, IFormFile slipImage)
{
    try
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อนชำระเงิน" });

        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId && o.UserId == currentUserId);
        if (order == null)
            return Json(new { success = false, message = "ไม่พบ Order" });

        if (slipImage == null || slipImage.Length == 0)
            return Json(new { success = false, message = "กรุณาเลือกไฟล์" });

        var fileName = $"slip_{orderId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(slipImage.FileName)}";
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "slips");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
            await slipImage.CopyToAsync(stream);

        order.SlipImagePath = "/uploads/slips/" + fileName;
        order.PaymentStatus = "PendingVerify";
        _db.SaveChanges();

        return Json(new { success = true, message = "อัปโหลดสลิปเรียบร้อย รอ Admin ยืนยัน" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
    }
}
```

### ฟังก์ชันนี้ทำอะไร

ใช้อัปโหลดสลิปการโอนเงินหลังผู้ใช้ชำระเงินแล้ว

### จุดสำคัญ

```csharp
order.SlipImagePath = "/uploads/slips/" + fileName;
order.PaymentStatus = "PendingVerify";
```

บันทึก path ของรูป และเปลี่ยนสถานะรอแอดมินตรวจ

### สรุปสั้น ๆ

`UploadSlip()` = รับรูปสลิป เก็บไฟล์ และเปลี่ยนสถานะการชำระเงิน

---

## 9. ฟังก์ชัน IsPromotionAvailable(...)

```csharp
private static bool IsPromotionAvailable(Promotion promotion)
{
    if (!promotion.IsActive)
        return false;

    var now = DateTime.Now;
    if (promotion.StartDate.HasValue && promotion.StartDate.Value > now)
        return false;

    if (promotion.EndDate.HasValue && promotion.EndDate.Value < now)
        return false;

    return true;
}
```

### ฟังก์ชันนี้ทำอะไร

ตรวจว่าโปรโมชันยังใช้งานได้หรือไม่

---

## 10. ฟังก์ชัน GetCurrentUserId()

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

## สรุปภาพรวมของ OrderController

- `Checkout()` เช็ก login ก่อนเปิด checkout
- `CreateOrder()` เป็นหัวใจของ flow สั่งซื้อ
- `GetUserPromos()` ดึงโปรโมชันที่ใช้ได้
- `SubmitPromotionClaim()` ส่งหลักฐาน claim โปรโมชัน event
- `Payment()` สร้าง QR PromptPay
- `UploadSlip()` เปลี่ยนสถานะ order เป็น `PendingVerify`
