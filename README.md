# Project Bagery

ระบบเว็บร้านเบเกอรี่ที่พัฒนาด้วย ASP.NET Core MVC เชื่อมต่อฐานข้อมูล MySQL ใช้สำหรับจัดการการขายสินค้า การสั่งซื้อ การชำระเงินด้วย PromptPay การติดตามสถานะการจัดส่ง และหลังบ้านสำหรับผู้ดูแลระบบ

## ภาพรวมระบบ

- Backend: C# / ASP.NET Core MVC
- ORM: Entity Framework Core
- Database: MySQL
- Frontend: Razor Views + Bootstrap + JavaScript
- Payment Flow: PromptPay QR + อัปโหลดสลิป
- State Management: Session

จุดเข้าใช้งานหลักของระบบถูกกำหนดไว้ที่:

- Default route: `Account/Home`
- Login ของผู้ใช้และแอดมิน: `Account/Login`
- Dashboard หลังบ้าน: `Account/Dashbordadmin`

## โครงสร้างโปรเจกต์

```text
Project-Bagery/
|- Controllers/           ควบคุม flow ของ request และ business flow หลัก
|- Models/Db/             entity และ DbContext ของฐานข้อมูล
|- Viewmodels/            model สำหรับส่งข้อมูลจาก controller ไปยัง view
|- Views/                 Razor views ของฝั่งลูกค้าและแอดมิน
|- Helpers/               utility ที่ใช้ร่วมกัน เช่น PromptPay QR payload
|- Migrations/            EF Core migrations และ SQL script ประกอบ
|- wwwroot/               static files, CSS, JS, รูป, ไฟล์อัปโหลด
|- Properties/            launch settings สำหรับรันในเครื่อง
|- Program.cs             จุดเริ่มต้นการตั้งค่า service และ middleware
|- appsettings.json       config หลัก เช่น connection string
|- 66022380.csproj        project file และ package references
```

## หน้าที่ของแต่ละส่วน

### 1. Controllers

ไฟล์หลักของระบบคือ `Controllers/AccountController.cs` ซึ่งรวมทั้ง flow ฝั่งลูกค้าและหลังบ้านไว้ใน controller เดียว

- `Home()` แสดงหน้าแรก
- `Signup()` และ `Login()` จัดการสมัครสมาชิกและเข้าสู่ระบบ
- `Profile()` จัดการข้อมูลผู้ใช้และที่อยู่
- `Menu()` แสดงรายการสินค้า
- `Checkout()` ใช้เป็นหน้าตรวจสอบก่อนสร้างออเดอร์
- `CreateOrder()` สร้างออเดอร์และบันทึกรายการสินค้า
- `Payment()` สร้าง PromptPay QR สำหรับออเดอร์
- `ConfirmPayment()` ให้แอดมินยืนยันการชำระเงินและตัด stock
- `Delivery()` แสดงสถานะออเดอร์ปัจจุบันและประวัติ
- `AcceptOrder()` เปลี่ยนสถานะเป็น `Preparing`
- `ShipOrder()` เปลี่ยนสถานะเป็น `Shipped`
- `CompleteOrder()` ให้ลูกค้ายืนยันรับสินค้าและย้ายข้อมูลเข้า `historyorder`
- `Dashbordadmin()` สรุปข้อมูลภาพรวมระบบ
- `Stock()` จัดการสินค้าและหมวดหมู่
- `Order()` ดูรายการออเดอร์ทั้งหมด
- `PromotionAdmin()` จัดการโปรโมชั่นและแจกโปร
- `Member()` จัดการสมาชิกและสิทธิ์ผู้ใช้

`HomeController.cs` และ `Lab8Controller.cs` มีบทบาทรอง ใช้กับหน้าทดลองหรือหน้าทั่วไป

### 2. Models/Db

โฟลเดอร์นี้เก็บ entity ของฐานข้อมูล และ `BakerydbContext`

ตารางหลักที่ใช้ในระบบ:

- `user` ข้อมูลผู้ใช้
- `role` บทบาทผู้ใช้ เช่น Admin, Staff, User
- `address` ที่อยู่จัดส่งของผู้ใช้
- `category` หมวดหมู่สินค้า
- `stock` ข้อมูลสินค้า ราคา รายละเอียด และจำนวนคงเหลือ
- `promotion` โปรโมชัน
- `user_promotion` โปรโมชันที่แจกให้ผู้ใช้แต่ละคน
- `order` หัวออเดอร์
- `orderdetail` รายการสินค้าในออเดอร์
- `historyorder` ประวัติออเดอร์ที่ปิดงานแล้ว

สถานะออเดอร์ที่พบในระบบ:

- `Pending`
- `Paid`
- `Preparing`
- `Shipped`
- `Completed`
- `Cancelled`

สถานะการชำระเงินที่พบใน flow:

- `Pending`
- `PendingVerify`
- `Paid`

### 3. Viewmodels

ใช้สำหรับจัดรูปข้อมูลก่อนส่งเข้า Razor View

- `ProfileViewModel` ข้อมูลโปรไฟล์และที่อยู่
- `OrderViewModel` โครงสร้างรับข้อมูลออเดอร์จากหน้า checkout
- `DeliveryTrackingViewModel` ใช้แสดงสถานะจัดส่งและประวัติ
- `AdminDashboardViewModel` ใช้สรุปข้อมูล dashboard
- `AdminStockViewModel` ใช้ในหน้าจัดการสินค้า/หมวดหมู่
- `AdminPromotionViewModel` ใช้ในหน้าจัดการโปรโมชัน

### 4. Views

แบ่งเป็น 3 กลุ่มหลัก

- `Views/Account/` หน้าฝั่งลูกค้า เช่น `Home`, `Menu`, `Checkout`, `Payment`, `Delivery`, `Profile`, `Login`, `Signup`
- `Views/admin/` หน้าหลังบ้าน เช่น `Dashbordadmin`, `Stock`, `Order`, `PromotionAdmin`, `Member`
- `Views/Shared/` layout และ partial กลาง เช่น navbar, footer, cart drawer, admin layout

### 5. Helpers

- `Helpers/PromptPayHelper.cs` ใช้สร้าง payload สำหรับ PromptPay QR

### 6. Migrations

ใช้เก็บ EF Core migrations และมี SQL script บางส่วนสำหรับรันตรงกับฐานข้อมูล เช่นการเพิ่มสถานะ `Completed` และตาราง `historyorder`

### 7. wwwroot

เก็บ static assets ของระบบ

- `wwwroot/css/` style หลัก
- `wwwroot/js/` script ฝั่งหน้าเว็บ
- `wwwroot/img/` รูปภาพในระบบ
- `wwwroot/uploads/slips/` ไฟล์สลิปที่ลูกค้าอัปโหลด
- `wwwroot/logs/` log การ login ที่ระบบสร้างระหว่าง runtime

## การตั้งค่าระบบ

### Startup และ middleware

ใน `Program.cs` ระบบตั้งค่าหลักดังนี้

- ลงทะเบียน MVC ด้วย `AddControllersWithViews()`
- เชื่อมต่อ MySQL ผ่าน `BakerydbContext`
- เปิดใช้งาน Session โดยกำหนด timeout 30 นาที
- ใช้ static files, routing, session
- map route ค่าเริ่มต้นไปที่ `Account/Home`

### Database connection

ค่าปัจจุบันใน `appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=bakerydb;user=root;password=1234;SslMode=none;;AllowPublicKeyRetrieval=true;"
}
```

หมายเหตุ: ใน `BakerydbContext.cs` ยังมี `OnConfiguring()` ที่ระบุ connection string ซ้ำไว้ด้วย จึงควรรู้ว่าปัจจุบันระบบมี config ซ้ำ 2 จุด

### URL สำหรับรันในเครื่อง

จาก `Properties/launchSettings.json`

- HTTP: `http://localhost:5082`
- HTTPS: `https://localhost:7070`

## Workflow การทำงานของระบบ

### 1. Authentication และการระบุตัวตน

1. ผู้ใช้สมัครสมาชิกผ่าน `Signup`
2. ระบบสร้าง record ในตาราง `user` โดยกำหนด `RoleId = 3` สำหรับผู้ใช้ทั่วไป
3. ผู้ใช้ login ผ่าน username หรือ email
4. ระบบตรวจสอบข้อมูลในตาราง `user`
5. เมื่อสำเร็จ ระบบเก็บ `UserId` และ `Username` ลงใน session
6. ถ้า `RoleId == 1` จะพาไปหน้า `Dashbordadmin`
7. ถ้าไม่ใช่แอดมิน จะพาไปหน้า `Home`

เพิ่มเติม:

- ระบบบันทึก log การ login ลงไฟล์ใน `wwwroot/logs/login_yyyy-MM-dd.log`
- การตรวจสิทธิ์แอดมินในโค้ดปัจจุบันใช้การเช็ก `RoleId == 1`

### 2. Flow ฝั่งลูกค้า

#### 2.1 ดูสินค้า

1. ผู้ใช้เข้า `Menu`
2. Controller ดึงข้อมูลจาก `stock` พร้อม `category`
3. View แสดงสินค้าให้เลือกซื้อ

#### 2.2 จัดการโปรไฟล์และที่อยู่

1. ผู้ใช้เข้า `Profile`
2. ระบบอ่านข้อมูลผู้ใช้จาก session
3. ดึงข้อมูลจาก `user` และ `address`
4. ผู้ใช้สามารถแก้ไขข้อมูลส่วนตัว เพิ่ม/แก้ไข/ลบที่อยู่ได้

#### 2.3 สร้างออเดอร์

1. ผู้ใช้เลือกสินค้าและไปหน้า `Checkout`
2. หน้าเว็บส่งข้อมูลออเดอร์เข้า `CreateOrder()`
3. ระบบสร้างข้อมูลใน `order`
   - `Status = Pending`
   - `PaymentStatus = Pending`
4. ระบบสร้างรายการสินค้าใน `orderdetail`
5. ถ้ามีการใช้โปรโมชัน ระบบจะ mark `user_promotion.IsUsed = 1`
6. ระบบตอบกลับ `orderId` เพื่อใช้ไปต่อในหน้าชำระเงิน

#### 2.4 ชำระเงิน

1. ผู้ใช้เข้า `Payment(orderId)`
2. ระบบดึงข้อมูลออเดอร์และรายการสินค้า
3. สร้าง PromptPay payload ผ่าน `PromptPayHelper`
4. สร้าง QR Code และส่งให้ view
5. ผู้ใช้อัปโหลดสลิปการโอน
6. ระบบบันทึกไฟล์ไว้ที่ `wwwroot/uploads/slips/`
7. ระบบอัปเดตออเดอร์:
   - `SlipImagePath = /uploads/slips/...`
   - `PaymentStatus = PendingVerify`

#### 2.5 ติดตามการจัดส่ง

1. ผู้ใช้เข้า `Delivery`
2. ระบบดึงออเดอร์ของผู้ใช้จาก `order`
3. ถ้ามีออเดอร์ที่ยังไม่ `Completed` จะถือเป็น active order
4. ระบบสร้าง `DeliveryTrackingViewModel` เพื่อแสดง:
   - หมายเลขออเดอร์
   - สถานะออเดอร์
   - สถานะการชำระเงิน
   - ที่อยู่จัดส่ง
   - รายการสินค้า
   - timeline ของการดำเนินงาน
5. ถ้ามีออเดอร์ที่ปิดงานแล้ว ระบบดึงประวัติจาก `historyorder`

#### 2.6 ปิดออเดอร์เมื่อรับของแล้ว

1. เมื่อลูกค้าได้รับสินค้าแล้ว จะเรียก `CompleteOrder(orderId)`
2. ระบบตรวจว่าผู้ใช้ใน session เป็นเจ้าของออเดอร์จริง
3. ระบบอนุญาตเฉพาะออเดอร์ที่อยู่ในสถานะ `Shipped`
4. ระบบบันทึก snapshot ของออเดอร์ลง `historyorder`
5. ระบบเปลี่ยนสถานะออเดอร์เป็น `Completed`

### 3. Flow ฝั่งแอดมิน

#### 3.1 Dashboard

หน้า `Dashbordadmin` ใช้ดูภาพรวมของระบบ เช่น

- รายได้วันนี้
- จำนวนออเดอร์วันนี้
- ออเดอร์ที่รอตรวจสลิป
- จำนวนผู้ใช้ทั้งหมด
- สินค้าใกล้หมด / หมดสต็อก
- จำนวนโปรโมชันที่ใช้งานอยู่
- รายได้ย้อนหลังรายเดือน
- ออเดอร์ล่าสุด

#### 3.2 จัดการสินค้าและหมวดหมู่

หน้า `Stock` ใช้ดูและแก้ไข:

- หมวดหมู่สินค้า (`category`)
- สินค้า (`stock`)
- จำนวนคงเหลือ
- ราคา
- รายละเอียดสินค้า

#### 3.3 ตรวจสอบออเดอร์และยืนยันการชำระเงิน

1. แอดมินเปิดหน้า `Order`
2. ดูรายละเอียดออเดอร์ผ่าน `GetOrderDetails(orderId)`
3. เมื่อสลิปถูกต้อง ให้เรียก `ConfirmPayment(orderId)`
4. ระบบจะ:
   - ลด stock ของสินค้าทุกรายการในออเดอร์
   - เปลี่ยน `PaymentStatus = Paid`
   - เปลี่ยน `Status = Paid`

#### 3.4 เริ่มเตรียมสินค้า

1. แอดมินเรียก `AcceptOrder(orderId)`
2. ระบบเปลี่ยนสถานะเป็น `Preparing`

#### 3.5 จัดส่งสินค้า

1. แอดมินเรียก `ShipOrder(orderId)`
2. ระบบตรวจว่าออเดอร์อยู่ในสถานะ `Preparing`
3. ระบบเปลี่ยนสถานะเป็น `Shipped`

#### 3.6 จัดการโปรโมชัน

หน้า `PromotionAdmin` รองรับ

- เพิ่ม/แก้ไขโปรโมชัน
- แจกโปรให้ผู้ใช้รายคนผ่าน `GiftPromotion`
- แจกโปรให้ผู้ใช้ทั้งหมดผ่าน `GiftPromotionToAll`
- ดูจำนวนการแจกและการใช้งานโปร

#### 3.7 จัดการสมาชิก

หน้า `Member` รองรับ

- ดูรายชื่อผู้ใช้
- เพิ่มผู้ใช้ใหม่
- แก้ไขข้อมูลผู้ใช้
- เปลี่ยน role
- ลบผู้ใช้

## ลำดับสถานะของออเดอร์

flow ปัจจุบันของระบบเป็นดังนี้

```text
สร้างออเดอร์
Pending / Payment Pending
    ->
ลูกค้าอัปโหลดสลิป
Pending / Payment PendingVerify
    ->
แอดมินยืนยันการชำระเงิน
Paid / Payment Paid
    ->
แอดมินรับออเดอร์
Preparing
    ->
แอดมินจัดส่ง
Shipped
    ->
ลูกค้ายืนยันรับสินค้า
Completed + บันทึก historyorder
```

## ความสัมพันธ์ของข้อมูลแบบย่อ

- ผู้ใช้หนึ่งคนมีหลายที่อยู่
- ผู้ใช้หนึ่งคนมีหลายออเดอร์
- ออเดอร์หนึ่งรายการมีหลาย `orderdetail`
- `orderdetail` แต่ละรายการอ้างอิงสินค้าใน `stock`
- สินค้าหลายรายการอยู่ภายใต้หมวดหมู่หนึ่ง
- โปรโมชั่นสามารถผูกกับออเดอร์ และแจกให้ผู้ใช้ผ่าน `user_promotion`

## การรันโปรเจกต์ในเครื่อง

### สิ่งที่ต้องมี

- .NET SDK ที่รองรับ target framework ของโปรเจกต์
- MySQL Server
- ฐานข้อมูล `bakerydb`

### ขั้นตอนรัน

1. สร้างฐานข้อมูล `bakerydb`
2. ตรวจสอบ connection string ใน `appsettings.json`
3. รัน migration หรือเตรียม schema ให้ตรงกับ entity ปัจจุบัน
4. รันคำสั่ง:

```bash
dotnet restore
dotnet run
```

5. เปิดผ่าน `http://localhost:5082` หรือ `https://localhost:7070`

## ไฟล์ที่ควรรู้ก่อนเริ่มพัฒนา

- `Program.cs` จุดเริ่มต้นของระบบ
- `Controllers/AccountController.cs` business flow หลักแทบทั้งหมด
- `Models/Db/BakerydbContext.cs` โครงสร้างข้อมูลและความสัมพันธ์
- `Viewmodels/` โครงสร้างข้อมูลที่ view ใช้จริง
- `Views/Account/` flow ฝั่งลูกค้า
- `Views/admin/` flow ฝั่งแอดมิน
- `Helpers/PromptPayHelper.cs` logic สร้าง PromptPay QR

## หมายเหตุสำคัญสำหรับทีมพัฒนา

- ระบบปัจจุบันใช้ session สำหรับ login state
- มีการเรียก `UseAuthentication()` แต่ยังไม่เห็นการตั้งค่า authentication scheme แบบเต็มรูปแบบ
- รหัสผ่านใน flow ปัจจุบันถูกตรวจเทียบตรงจากฐานข้อมูล จึงควรพิจารณา hash password หากจะนำไปใช้จริง
- `AccountController` รับผิดชอบหลายหน้าที่มาก อาจแยกเป็นหลาย controller ได้ในอนาคต เช่น `Auth`, `Customer`, `Admin`, `Order`
- มีไฟล์เอกสารเก่า `repo.txt` แต่ตัวอักษรเพี้ยนจาก encoding จึงควรใช้ README นี้เป็นเอกสารหลักแทน

## สรุป

โปรเจกต์นี้เป็นระบบร้านเบเกอรี่แบบ full flow ตั้งแต่สมัครสมาชิก เลือกสินค้า สร้างออเดอร์ ชำระเงิน ตรวจสลิป ตัด stock ติดตามจัดส่ง ไปจนถึงปิดออเดอร์และดูประวัติ โดยใช้ ASP.NET Core MVC + EF Core + MySQL เป็นแกนหลัก และรวม logic ส่วนใหญ่ไว้ที่ `AccountController`


Controllers/
├── AccountController.cs        // เหลือแค่ Auth (Login, Signup, Logout)
├── ProfileController.cs        // Profile, Address
├── OrderController.cs          // CreateOrder, Payment, UploadSlip, CompleteOrder
├── DeliveryController.cs       // Delivery tracking
└── Admin/
    ├── AdminDashboardController.cs
    ├── AdminOrderController.cs     // ConfirmPayment, AcceptOrder, ShipOrder
    ├── AdminStockController.cs     // Stock, Category
    ├── AdminPromotionController.cs
    └── AdminMemberController.cs