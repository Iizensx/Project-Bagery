# ระบบจัดการร้านเบเกอรี่ (Bakery Management System) - สรุปข้อมูลทางเทคนิคฉบับสมบูรณ์

## ภาพรวมโครงการ (Project Overview)
เว็บแอปพลิเคชันที่พัฒนาด้วย ASP.NET Core MVC สำหรับบริหารจัดการธุรกิจร้านเบเกอรี่แบบครบวงจร ครอบคลุมระบบยืนยันตัวตนผู้ใช้, การจัดการคำสั่งซื้อ, การประมวลผลการชำระเงิน, การติดตามการจัดส่ง และฟังก์ชันสำหรับผู้ดูแลระบบ ระบบสร้างขึ้นบน .NET 10 ร่วมกับ Entity Framework Core และฐานข้อมูล MySQL

---

## 1. คอนโทรลเลอร์และแอคชันเมธอด (CONTROLLERS AND ACTION METHODS)

### 1.1 AccountController
**จัดการการยืนยันตัวตน การลงทะเบียน และเซสชันผู้ใช้**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Login()` | GET | แสดงฟอร์มเข้าสู่ระบบ |
| `Login(username, password)` | POST | ตรวจสอบสิทธิ์ผู้ใช้; เก็บ UserId และ Username ใน Session; ส่งไปหน้า Admin หาก RoleId=1 |
| `Signup()` | GET | แสดงฟอร์มลงทะเบียนสมาชิก |
| `Signup(...)` | POST | สร้างบัญชีใหม่ (RoleId=3); ตรวจสอบการยืนยันรหัสผ่าน; ส่งไปหน้า Login |
| `Logout()` | GET | ล้างข้อมูล Session ทั้งหมดและส่งกลับหน้าแรก |

**คุณสมบัติเด่น:**
- รองรับการเข้าสู่ระบบผ่าน Username หรือ Email
- ระบบบันทึก Log การล็อกอินพร้อม IP Address ลงใน `wwwroot/logs/login_YYYY-MM-DD.log`
- ระบบตรวจสอบสิทธิ์ตามบทบาท (Admin vs User) ผ่าน Session

---

### 1.2 HomeController
**หน้าเว็บหลักสำหรับลูกค้าและเมนูสินค้า**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Home()` | GET | แสดงหน้าแรกของเว็บไซต์ |
| `Menu()` | GET | ดึงข้อมูลสินค้าจากตาราง Stock พร้อมหมวดหมู่มาแสดงผล |
| `Privacy()` | GET | แสดงหน้าเกี่ยวกับนโยบายความเป็นส่วนตัว |

---

### 1.3 OrderController
**จัดการการสร้างคำสั่งซื้อ ตรวจสอบรายการ และการชำระเงิน**

| Action Method | HTTP | Purpose |
|---|---|---|
| `CreateOrder(...)` | POST | สร้าง Order ใหม่ (สถานะ "Pending"); บันทึก OrderDetail; ตัดการใช้โปรโมชั่น |
| `Payment(orderId)` | GET | สร้างชุดข้อมูล PromptPay QR Code และแปลงเป็นรูปภาพ Base64 เพื่อแสดงผล |
| `UploadSlip(...)` | POST | บันทึกรูปภาพหลักฐานการโอนเงินลงในโฟลเดอร์ `/uploads/slips/` |

**คุณสมบัติเด่น:**
- รองรับการชำระเงินผ่าน PromptPay (QR Code)
- ระบบตรวจสอบคูปองส่วนลดที่ผู้ใช้มี (UserPromos)
- ขั้นตอนการส่งหลักฐานการโอนเงิน (Slip Verification)

---

### 1.4 ProfileController
**จัดการข้อมูลส่วนตัวและที่อยู่จัดส่ง**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Profile()` | GET/POST | แสดงและแก้ไขข้อมูลส่วนตัว (ชื่อ, อีเมล, เบอร์โทร); เปลี่ยนรหัสผ่าน |
| `SaveAddress(...)` | POST | เพิ่มที่อยู่ใหม่หรือแก้ไขที่อยู่เดิม |
| `DeleteAddress(...)` | POST | ลบที่อยู่จัดส่ง (มีการตรวจสอบสิทธิ์เจ้าของที่อยู่) |

---

### 1.5 DeliveryController
**ติดตามสถานะคำสั่งซื้อและความคืบหน้าการจัดส่ง**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Delivery()` | GET | แสดงหน้าติดตามสถานะ; ดึงรายการที่กำลังดำเนินการและประวัติการสั่งซื้อ |
| `CompleteOrder()` | POST | ลูกค้ากดยืนยันได้รับสินค้า (เปลี่ยนสถานะจาก Shipped → Completed) |

**ลำดับสถานะ (Status Flow):**
`Pending (รอชำระ)` → `Paid (จ่ายแล้ว/รอตรวจ)` → `Preparing (กำลังผลิต)` → `Shipped (กำลังส่ง)` → `Completed (สำเร็จ)`

---

### 1.6 คอนโทรลเลอร์สำหรับผู้ดูแลระบบ (Admin Controllers)

#### AdminControllerBase
**คลาสฐานสำหรับหน้าผู้ดูแลระบบทั้งหมด**

ไม่ได้เป็นหน้าเว็บโดยตรง แต่เป็นคลาสกลางที่ Admin Controller ทุกตัวสืบทอดต่อ เพื่อรวมโค้ดที่ใช้ซ้ำร่วมกัน เช่น
- เก็บ `BakerydbContext` ไว้ให้คอนโทรลเลอร์ลูกใช้งาน
- ตรวจสอบว่าผู้ใช้ปัจจุบันเป็น Admin หรือไม่ ด้วย `IsCurrentUserAdmin()`
- อ่าน `UserId` จาก Session ด้วย `GetCurrentUserId()`
- ส่งกลับไปหน้า Login ด้วย `RedirectToAdminLogin()` เมื่อยังไม่ได้รับสิทธิ์

#### AdminDashboardController
**หน้าแดชบอร์ดสำหรับสรุปภาพรวมธุรกิจ**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Dashbordadmin()` | GET | ตรวจสอบสิทธิ์ Admin; ดึงข้อมูล Orders, Users, Stocks, Promotions; คำนวณรายได้วันนี้, ออเดอร์วันนี้, จำนวนผู้ใช้, สินค้าใกล้หมด, สินค้าหมด, รายได้ย้อนหลัง 6 เดือน, ออเดอร์ล่าสุด และสรุปสต็อกตามหมวดหมู่ |

**คุณสมบัติเด่น:**
- แสดง KPI สำคัญของร้านในหน้าเดียว
- สรุปรายได้ย้อนหลังรายเดือนเพื่อใช้ทำกราฟ
- แสดงรายการออเดอร์ล่าสุดและสินค้าใกล้หมดสำหรับติดตามงานประจำวัน

#### AdminOrderController
**จัดการคำสั่งซื้อฝั่งผู้ดูแลระบบและการเปลี่ยนสถานะออเดอร์**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Order()` | GET | แสดงรายการคำสั่งซื้อทั้งหมด พร้อมข้อมูลลูกค้าและรายการสินค้า |
| `ConfirmPayment(orderId)` | POST | ยืนยันการชำระเงิน; เปลี่ยน `PaymentStatus` และ `Status` เป็น `Paid`; ตัดสต็อกสินค้าตามจำนวนที่สั่ง |
| `GetOrderDetails(orderId)` | GET | ส่งรายละเอียดออเดอร์กลับแบบ JSON เช่น ลูกค้า รายการสินค้า ยอดรวม สถานะ และรูปสลิป |
| `AcceptOrder(orderId)` | POST | เปลี่ยนสถานะออเดอร์จากขั้นก่อนหน้าเป็น `Preparing` เพื่อเริ่มผลิตสินค้า |
| `ShipOrder(orderId)` | POST | เปลี่ยนสถานะจาก `Preparing` เป็น `Shipped` เมื่อจัดส่งสินค้าแล้ว |

**คุณสมบัติเด่น:**
- มีขั้นตอนอนุมัติการชำระเงินก่อนตัดสต็อกจริง
- ใช้ JSON response สำหรับงานหน้าเว็บแบบโต้ตอบ
- มีการบันทึก Log เมื่อยืนยันจ่ายเงิน รับออเดอร์ และจัดส่ง

#### AdminStockController
**จัดการหมวดหมู่สินค้าและข้อมูลสต็อก**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Stock()` | GET | แสดงข้อมูลหมวดหมู่ทั้งหมดและสินค้าทั้งหมดสำหรับหน้าจัดการสต็อก |
| `SaveCategory(...)` | POST | เพิ่มหมวดหมู่ใหม่หรือแก้ไขหมวดหมู่เดิม พร้อมตรวจสอบว่าชื่อหมวดหมู่ไม่ว่าง |
| `SaveStock(...)` | POST | เพิ่มสินค้าใหม่หรือแก้ไขสินค้าเดิม พร้อมตรวจสอบชื่อสินค้า หมวดหมู่ ราคา และจำนวนสต็อก |

**คุณสมบัติเด่น:**
- ใช้ `ValidateAntiForgeryToken` กับฟอร์ม POST
- แยกการจัดการหมวดหมู่และสินค้าออกจากกันชัดเจน
- ใช้ `TempData` ส่งข้อความสำเร็จหรือข้อผิดพลาดกลับไปหน้าเดิม

#### AdminPromotionController
**จัดการโปรโมชั่นและการแจกคูปองให้ผู้ใช้**

| Action Method | HTTP | Purpose |
|---|---|---|
| `PromotionAdmin()` | GET | แสดงรายการโปรโมชั่นทั้งหมด รายชื่อผู้ใช้ และสถิติการแจก/การใช้งานโปรโมชั่น |
| `SavePromotion(...)` | POST | เพิ่มโปรโมชั่นใหม่หรือแก้ไขโปรโมชั่นเดิม โดยรองรับส่วนลดแบบ `Percent` และ `Baht` |
| `GiftPromotion(promotionId, userId)` | POST | แจกโปรโมชั่นให้ผู้ใช้รายบุคคล หรือรีเซ็ตสิทธิ์ให้ใช้ใหม่ได้ |
| `GiftPromotionToAll(promotionId)` | POST | แจกโปรโมชั่นเดียวกันให้ผู้ใช้ทุกคนในระบบ |

**คุณสมบัติเด่น:**
- สรุปจำนวนครั้งที่โปรโมชั่นถูกแจกและถูกใช้จริง
- รองรับทั้งการแจกเฉพาะคนและแจกทั้งระบบ
- ถ้ามีคูปองเดิมอยู่แล้ว ระบบจะรีเซ็ต `IsUsed` และ `UsedAt` ให้ใช้ซ้ำได้

#### AdminMemberController
**จัดการข้อมูลสมาชิกและบทบาทการใช้งาน**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Member()` | GET | แสดงรายชื่อสมาชิกทั้งหมดพร้อมข้อมูลบทบาท และเตรียมข้อมูล Role สำหรับหน้าแก้ไข |
| `SaveMember(...)` | POST | เพิ่มสมาชิกใหม่หรือแก้ไขข้อมูลสมาชิกเดิม รวมถึงกำหนดบทบาทผู้ใช้ |
| `DeleteMember(userId)` | POST | ลบสมาชิกออกจากระบบ โดยป้องกันไม่ให้ Admin ลบบัญชีที่กำลังใช้งานอยู่ |

**คุณสมบัติเด่น:**
- รองรับทั้งสร้างสมาชิกใหม่และแก้ไขบัญชีเดิมในเมธอดเดียว
- ตรวจสอบความถูกต้องของ Role ก่อนบันทึก
- ป้องกันการลบบัญชีของ Admin ที่กำลังล็อกอินอยู่

#### หมายเหตุเพิ่มเติมเกี่ยวกับจำนวน Controller
- ถ้านับเฉพาะ Controller ที่ใช้งานจริงในโปรเจกต์ จะมีทั้งฝั่งผู้ใช้ทั่วไปและฝั่ง Admin
- ในโฟลเดอร์ `Controllers/Admin` มี **5 Controller หลัก** และ **1 Base Controller**
- ดังนั้นสรุปเดิมที่เขียนรวมเป็นหัวข้อเดียวอาจทำให้ดูเหมือนว่ายังอธิบายฝั่ง Admin ไม่ครบ

---

## 2. โมเดลฐานข้อมูล (DATABASE MODELS)



- **User:** ข้อมูลสมาชิกและบทบาท
- **Order / Orderdetail:** หัวข้อคำสั่งซื้อและรายการสินค้าในคำสั่งซื้อนั้นๆ
- **Stock:** ข้อมูลสินค้า ราคา และจำนวนคงเหลือ
- **Category:** หมวดหมู่สินค้า
- **Address:** ที่อยู่จัดส่ง (1 คน มีได้หลายที่อยู่)
- **Promotion / UserPromotion:** ข้อมูลส่วนลดและการถือครองคูปองของลูกค้าแต่ละคน
- **Historyorder:** ระบบจัดเก็บประวัติคำสั่งซื้อที่เสร็จสมบูรณ์แล้ว (Archive)

---

## 3. คุณสมบัติพิเศษและการเชื่อมต่อระบบภายนอก

### 3.1 ระบบชำระเงิน PromptPay
- ใช้ `PromptPayHelper` สร้าง EMV Payload ตามมาตรฐาน QR Code ของไทย
- รองรับทั้งการระบุยอดเงินและไม่ระบุยอดเงิน
- แสดงผลผ่านไลบรารี `QRCoder`

### 3.2 การจัดการสต็อกแบบ Real-time
- สต็อกจะถูก **ตัดออกทันที** เมื่อ Admin กดยืนยันความถูกต้องของสลิปโอนเงิน (Confirm Payment)
- ระบบแจ้งเตือนในหน้า Dashboard เมื่อสินค้าใกล้หมด

### 3.3 ระบบโปรโมชั่นหลายระดับ
- รองรับส่วนลดแบบ **เปอร์เซ็นต์ (%)** และ **จำนวนเงินคงที่ (฿)**
- ระบบตรวจสอบการใช้งาน 1 ครั้งต่อ 1 โค้ดต่อคน (IsUsed flag)

---

## 4. สถาปัตยกรรมทางเทคนิค (TECHNICAL ARCHITECTURE)

- **Framework:** .NET 10 (ASP.NET Core MVC)
- **Database:** MySQL 8.0 (Pomelo EntityFrameworkCore)
- **Frontend:** HTML5, CSS3, JavaScript, Bootstrap, jQuery
- **Session:** ระบบจัดเก็บข้อมูลผู้ใช้แบบชั่วคราว (Stateless Session)
- **Logging:** ระบบบันทึกเหตุการณ์ลงไฟล์ (File-based logging)

---

## 5. ข้อควรระวังและความปลอดภัย (SECURITY CONSIDERATIONS)

- ✅ มีระบบตรวจสอบสิทธิ์เข้าถึงหน้า Admin (Role-based Authorization)
- ✅ มีการตรวจสอบความเป็นเจ้าของข้อมูล (Ownership Verification) ก่อนแก้ไขที่อยู่หรือใช้คูปอง
- ⚠️ **ข้อแนะนำเพิ่มเติม:** ปัจจุบันรหัสผ่านถูกเก็บแบบข้อความธรรมดา (Plain-text) ควรเปลี่ยนไปใช้การเข้ารหัสแบบ **Password Hashing (เช่น BCrypt)** เพื่อความปลอดภัยสูงสุดในเวอร์ชันใช้งานจริง

---

## สถิติสรุปของระบบ (Summary Statistics)

| หัวข้อ | จำนวน |
|--------|-------|
| คอนโทรลเลอร์ (Controllers) | 11 ตัว (6 ทั่วไป + 5 Admin, ไม่นับ Base Controller) |
| ตารางฐานข้อมูล (Database Tables) | 10 ตาราง |
| หน้าจอการใช้งาน (Views) | 15+ หน้า |
| ไลบรารีภายนอกที่สำคัญ | QRCoder, Bootstrap, jQuery |





# Bakery Management System - Complete Technical Summary

## Project Overview
A comprehensive ASP.NET Core MVC web application for managing a bakery business, including user authentication, order management, payment processing, delivery tracking, and administrative functions. The system is built on .NET 10 with Entity Framework Core and MySQL database.

---

## 1. CONTROLLERS AND ACTION METHODS

### 1.1 AccountController
**Handles user authentication, registration, and session management**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Login()` | GET | Display login form |
| `Login(username, password)` | POST | Authenticate user against database; store UserId and Username in session; redirect to admin dashboard if RoleId=1, else to Home |
| `Signup()` | GET | Display user registration form |
| `Signup(username, email, phone, password, confirmPassword)` | POST | Create new user account with RoleId=3 (regular user); validate password confirmation; redirect to login |
| `Logout()` | GET | Clear all session data and redirect to home page |

**Key Features:**
- Password and username validation (supports login via username OR email)
- IP address logging for all login attempts to `wwwroot/logs/login_YYYY-MM-DD.log`
- Automatic session-based role detection (Admin vs Regular User)
- Plain-text password storage (note: not hashed)

---

### 1.2 HomeController
**Provides core user-facing pages and menu functionality**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Home()` | GET | Display homepage |
| `Menu()` | GET | Fetch and display all products from Stock table with Category information |
| `Contact()` | GET | Redirect to Home (placeholder for contact functionality) |
| `lab8()` | GET | Display all users in database (appears to be for testing/demonstration) |
| `Privacy()` | GET | Display privacy policy page |
| `Error()` | GET | Display error page with RequestId and disable caching |

**Key Features:**
- Product listing with category filtering/organization
- LINQ Query Syntax used in `lab8()` action

---

### 1.3 OrderController
**Manages order creation, checkout, and payment processing**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Checkout()` | GET | Display checkout page |
| `CreateOrder([FromBody] OrderRequest)` | POST | Create new Order with status="Pending"; create OrderDetail entries for each item; mark promotions as used if applied |
| `GetCurrentUser()` | GET | Retrieve UserId from session for client-side order initialization |
| `GetUserPromos(userId)` | GET | Return all active (unused) promotions for a specific user as JSON |
| `Payment(orderId)` | GET | Generate PromptPay QR code payload; convert to PNG; embed as Base64 in view |
| `UploadSlip(orderId, slipImage)` | POST | Save payment slip image to `wwwroot/uploads/slips/`; update SlipImagePath in Order record |

**Key Features:**
- Order creation from JSON request body
- Stock lookup by ProductName
- PromptPay QR code generation for mobile banking payments (Thai payment method)
- Supports promotional discount application
- Slip verification workflow

**Database Operations:**
- Creates Order record first (to get OrderId)
- Creates Orderdetail records for line items
- Updates UserPromotion.IsUsed flag when promotion applied
- Updates Order.SlipImagePath for payment slip storage

---

### 1.4 ProfileController
**Manages user account information and delivery addresses**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Profile(id=0)` | GET | Display user profile and saved addresses; defaults to current session user if no ID provided; retrieves from database and populates ProfileViewModel |
| `Profile([FromBody] ProfileViewModel)` | POST | Update user profile (username, email, phone); optional password change with validation; requires current password verification |
| `SaveAddress(userId, addressId, addressLine, district, province, postalCode)` | POST | Create new address (addressId=0) or edit existing address (addressId>0); includes ownership validation |
| `DeleteAddress(addressId, userId)` | POST | Delete user's delivery address with ownership verification |

**Key Features:**
- Multi-address support per user
- Password change with current password verification
- Input validation (6+ character minimum for new passwords)
- Ownership verification for address operations (prevents cross-user access)

---

### 1.5 DeliveryController
**Tracks order status and delivery progress**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Delivery()` | GET | Display delivery tracking page; fetch user's active and completed orders; fetch order history; returns DeliveryTrackingViewModel with progress indicators |
| `CompleteOrder(orderId)` | POST | Mark order as Completed when customer receives goods; create Historyorder record if not exists; only allows if status="Shipped" |

**Key Features:**
- Real-time order status transitions (Pending → Paid → Preparing → Shipped → Completed)
- Order history archiving (Historyorder table)
- Delivery address and item summary aggregation
- Stage tracking for UI progress indicators
- Filters out Cancelled orders from tracking view

**Status Flow:**
```
Pending → Paid (payment confirmed) → Preparing (admin accepted) → Shipped → Completed (customer received)
```

---

### 1.6 Lab8Controller
**Test/demonstration controller**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Index()` | GET | Return empty view (appears to be placeholder/test) |
| `Privacy()` | GET | Display privacy page |
| `Error()` | GET | Display error page |

---

### 1.7 Admin Controllers (All inherit from AdminControllerBase)

#### AdminDashboardController
**Executive summary and KPI dashboard**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Dashbordadmin()` | GET | Display comprehensive admin dashboard with KPIs; calculates today's/month's revenue; counts orders by status; tracks low stock alerts; shows category summaries |

**Dashboard Metrics:**
- Today's Revenue (paid orders only)
- Today's Order Count
- Pending Payment Verification Orders
- Total Users
- Low Stock Products (≤10 units)
- Out of Stock Products (≤0 units)
- Active Promotions
- Orders by Status (Completed, Preparing, Shipped)
- 6-Month Revenue Trend (monthly breakdown)
- Recent Orders (last 6)
- Low Stock Items (top 6)
- Category Stock Summaries

---

#### AdminOrderController
**Order management and payment confirmation**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Order()` | GET | Display list of all orders with customer and line item details; ordered by OrderDate descending |
| `ConfirmPayment(orderId)` | POST | Verify payment received; reduce Stock quantity for each OrderDetail; update PaymentStatus="Paid" and Status="Paid"; log action |
| `GetOrderDetails(orderId)` | GET | Return order details as JSON (customer info, items, totals, payment proof) |
| `AcceptOrder(orderId)` | POST | Update Status from Pending→Preparing; prevents duplicate acceptance |
| `ShipOrder(orderId)` | POST | Update Status from Preparing→Shipped; requires prior Preparing status |

**Order Workflow:**
1. Customer creates order (Status: Pending, PaymentStatus: Pending)
2. Customer uploads payment slip
3. Admin confirms payment (Status→Paid, reduce stock)
4. Admin accepts order (Status→Preparing)
5. Admin ships order (Status→Shipped)
6. Customer marks received (Status→Completed)

---

#### AdminStockController
**Product and category inventory management**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Stock()` | GET | Display all categories and products; returns AdminStockViewModel with organized data |
| `SaveCategory(categoryId, categoryName, description)` | POST | Create new category (categoryId=0) or update existing (categoryId>0) with validation |
| `SaveStock(productId, productName, description, price, stock1, categoryId)` | POST | Create new product or update existing with full validation; validates category existence and pricing |

**Validations:**
- Category name required
- Product name required
- Product must belong to valid category
- Price ≥ 0
- Stock quantity ≥ 0

---

#### AdminPromotionController
**Promotional discount creation and distribution**

| Action Method | HTTP | Purpose |
|---|---|---|
| `PromotionAdmin()` | GET | Display all promotions with user and usage statistics (total gifted, total used) |
| `SavePromotion(promotionId, promotionName, description, discountValue, discountType)` | POST | Create or update promotion; supports Percent or Baht discount types |
| `GiftPromotion(promotionId, userId)` | POST | Assign promotion to specific user; creates UserPromotion record or resets usage flag |
| `GiftPromotionToAll(promotionId)` | POST | Broadcast promotion to all users in system; batch operation with upsert logic |

**Promotion Features:**
- Discount Types: Percentage (%) or Fixed Baht Amount (฿)
- Usage Tracking: IsUsed flag and UsedAt timestamp
- Distribution: Individual user or broadcast to all
- Reset Capability: Can mark used promotions as unused for re-distribution

---

#### AdminMemberController
**User account management and role assignment**

| Action Method | HTTP | Purpose |
|---|---|---|
| `Member()` | GET | Display all users with roles; provides RoleId list for assignment |
| `SaveMember(userId, username, email, phone, password, roleId)` | POST | Create new user (userId=0, requires password) or update existing user; validates role exists |
| `DeleteMember(userId)` | POST | Remove user from system; prevents deletion of current admin to avoid lockout |

**Role System:**
- RoleId=1: Admin
- RoleId=3: Regular User
- RoleId field is mandatory

---

## 2. DATABASE MODELS/ENTITIES

### User
```
- UserId (PK)
- Username (required, unique)
- Password (required, plain-text)
- Email (optional)
- Phone (optional)
- RoleId (FK to Role)
+ Relationships: Addresses, Orders, Historyorders
```

### Order
```
- OrderId (PK, INT)
- UserId (FK)
- AddressId (FK)
- OrderDate (DATETIME, default: CURRENT_TIMESTAMP)
- TotalAmount (DECIMAL 10,2)
- Status (ENUM: Pending, Paid, Preparing, Shipped, Completed, Cancelled)
- PaymentStatus (VARCHAR 50: Pending, Paid, PendingVerify)
- PromotionId (FK, nullable)
- SlipImagePath (VARCHAR, path to uploaded payment proof)
+ Relationships: User, Address, Promotion, Orderdetails[]
```

### Orderdetail
```
- OrderDetailId (PK)
- OrderId (FK)
- ProductId (FK to Stock)
- Quantity (INT)
- UnitPrice (DECIMAL 10,2)
+ Relationships: Order, Stock(Product)
```

### Stock (Products)
```
- ProductId (PK)
- ProductName (required)
- Description (optional, TEXT)
- Price (DECIMAL 10,2)
- Stock1 (INT, current quantity)
- CategoryId (FK)
+ Relationships: Category, Orderdetails[]
```

### Category
```
- CategoryId (PK)
- CategoryName (required, VARCHAR 100)
- Description (optional, TEXT)
+ Relationships: Stocks[]
```

### Address
```
- AddressId (PK)
- UserId (FK)
- AddressLine (VARCHAR 255)
- District (VARCHAR 100)
- Province (VARCHAR 100)
- PostalCode (VARCHAR 10)
+ Relationships: User, Orders[]
```

### Promotion
```
- PromotionId (PK)
- PromotionName (required)
- Description (optional)
- DiscountValue (DECIMAL)
- DiscountType (VARCHAR: "Percent" or "Baht")
+ Relationships: Orders[], UserPromotions[]
```

### UserPromotion (Promotion Distribution)
```
- UserId (FK, part of PK)
- PromotionId (FK, part of PK)
- IsUsed (INT: 0=unused, 1=used)
- UsedAt (DATETIME, nullable)
+ Relationships: User, Promotion
```

### Historyorder (Order Archive)
```
- HistoryOrderId (PK)
- OrderId (FK, nullable)
- UserId (FK)
- OrderDate (DATETIME)
- CompletedAt (DATETIME)
- TotalAmount (DECIMAL 10,2)
- Status (VARCHAR 50: usually "Completed")
- PaymentStatus (VARCHAR 50)
- DeliveryAddress (VARCHAR 500, concatenated address)
- ItemSummary (TEXT, e.g., "Product1 x2, Product2 x1")
+ Relationships: User
```

### Role
```
- RoleId (PK)
- RoleName (VARCHAR)
+ Relationships: Users[]
```

**Database Connection:**
- Engine: MySQL 8.0.43
- Host: localhost:3306
- Database: bakerydb
- User: root
- Password: 1234

---

## 3. VIEWMODELS AND DATA TRANSFER OBJECTS

### OrderRequest
**Used for creating orders via AJAX/API**
```csharp
- UserId: int
- Items: List<OrderItemRequest> (product name, price, quantity)
- TotalAmount: decimal
- AddressId: int?
- PromotionId: int?
```

### OrderItemRequest
**Individual line item for orders**
```csharp
- ProductName: string
- Price: decimal
- Quantity: int
```

### ProfileViewModel
**User profile data and address management**
```csharp
- UserId: int
- Username: string
- Email: string
- Phone: string
- CurrentPassword: string (for validation during password change)
- NewPassword: string
- ConfirmPassword: string
- Addresses: List<Address>
```

### DeliveryTrackingViewModel
**Order tracking and delivery status information**
```csharp
- HasOrder: bool
- OrderId: int
- OrderNumber: string (format: ORD-{OrderId})
- OrderStatus: string (Pending, Paid, Preparing, Shipped, Completed)
- PaymentStatus: string
- StatusTitle: string (Thai display text)
- StatusMessage: string (contextual message)
- EtaText: string (estimated time of arrival)
- TrackingStage: int (0-5 for progress indicators)
- CreatedAtText: string (formatted date/time)
- TotalAmount: decimal
- DeliveryAddress: string (full concatenated address)
- ItemSummary: string (e.g., "เค้กช็อคโกแลต x2, คุกกี้ x1")
- ShowPaymentAction: bool
- PaymentUrl: string
- ShowDeliveryActions: bool
- IsCompleted: bool
- OrderHistory: List<DeliveryOrderHistoryItem>
```

### DeliveryOrderHistoryItem
**Historical order record for user's past purchases**
```csharp
- OrderId: int
- OrderNumber: string
- OrderStatus: string
- PaymentStatus: string
- CreatedAtText: string
- SortDate: DateTime (for sorting)
- TotalAmount: decimal
- ItemSummary: string
- DeliveryAddress: string
- PromotionName: string
- DiscountDisplay: string
- IsActive: bool
```

### AdminDashboardViewModel
**Executive dashboard with KPIs and charts data**
```csharp
- TodayRevenue: decimal
- TodayOrders: int
- PendingVerificationOrders: int
- TotalUsers: int
- LowStockProducts: int (≤10 units)
- OutOfStockProducts: int (≤0 units)
- ActivePromotions: int
- CompletedOrders: int
- PreparingOrders: int
- ShippedOrders: int
- MonthlyRevenue: List<AdminDashboardMonthlyRevenueItem> (last 6 months)
- RecentOrders: List<AdminDashboardRecentOrderItem> (last 6)
- LowStockItems: List<AdminDashboardLowStockItem> (top 6)
- CategorySummaries: List<AdminDashboardCategoryItem>
```

### AdminDashboardMonthlyRevenueItem
**Monthly revenue chart data**
```csharp
- Label: string (e.g., "Jan", "Feb")
- Revenue: decimal
- Orders: int (order count that month)
```

### AdminDashboardRecentOrderItem
**Recent order summary**
```csharp
- OrderId: int
- CustomerName: string
- TotalAmount: decimal
- Status: string
- PaymentStatus: string
- OrderDateText: string (formatted)
```

### AdminDashboardLowStockItem
**Low stock alert item**
```csharp
- ProductId: int
- ProductName: string
- CategoryName: string
- Stock: int (current quantity)
```

### AdminDashboardCategoryItem
**Category inventory summary**
```csharp
- CategoryName: string
- ProductCount: int
- TotalStock: int (sum of all products in category)
```

### AdminPromotionViewModel
**Promotion management screen data**
```csharp
- Promotions: List<Promotion>
- Users: List<User>
- PromotionUsageCounts: Dictionary<int, int> (promotionId → count used)
- PromotionGiftCounts: Dictionary<int, int> (promotionId → count gifted)
```

### AdminStockViewModel
**Product and category management data**
```csharp
- Categories: List<Category> (with included Stocks)
- Products: List<Stock> (with included Category)
```

---

## 4. HELPER FILES AND UTILITIES

### PromptPayHelper.cs
**Generates PromptPay QR code payloads for Thai mobile banking**

**Methods:**
- `GeneratePayload(phoneOrId, amount)` → string
  - Accepts phone number (0812345678) or Thai ID
  - Normalizes format (converts 0812345678 → 66812345678)
  - Generates EMV payload with CRC checksum
  - Includes amount if provided (amount=0 for no fixed amount)
  - Returns complete QR code payload string compatible with QRCoder

**Usage:**
- Called from OrderController.Payment() to generate payment QR codes
- Payload converted to PNG via QRCoder library
- Encoded as Base64 for embedding in HTML

**Technical Details:**
- GUID: A000000677010111 (PromptPay identifier)
- Format: EMVCo standard
- CRC-16 CCITT checksum validation
- UTF-8 encoding

---

## 5. VIEWS STRUCTURE AND ORGANIZATION

### View Hierarchy
```
Views/
├── Shared/
│   ├── _Layoutmain.cshtml (main layout for regular users)
│   ├── _AdminLayout.cshtml (layout for admin pages)
│   ├── _Navbar.cshtml (navigation bar - user)
│   ├── _Dashbordnavbar.cshtml (navigation bar - admin)
│   ├── _Footer.cshtml (footer component)
│   ├── _CartDrawer.cshtml (shopping cart sidebar)
│   ├── _ValidationScriptsPartial.cshtml (client-side validation)
│   ├── Error.cshtml (error page)
│   ├── _ViewImports.cshtml (global using statements)
│   └── _ViewStart.cshtml (layout decorator setup)
│
├── Account/ (user-facing pages)
│   ├── Home.cshtml (homepage)
│   ├── Menu.cshtml (product catalog)
│   ├── Checkout.cshtml (cart summary and address selection)
│   ├── Payment.cshtml (payment method selection with PromptPay QR code)
│   ├── Delivery.cshtml (order tracking/delivery status)
│   ├── Profile.cshtml (user account and address management)
│   ├── Login.cshtml (authentication form)
│   ├── Signup.cshtml (user registration form)
│   └── Test1.cshtml (test/placeholder view)
│
├── admin/ (administrative pages, lowercase directory)
│   ├── Dashbordadmin.cshtml (KPI dashboard)
│   ├── Order.cshtml (order management with status transitions)
│   ├── Stock.cshtml (product and category inventory management)
│   ├── PromotionAdmin.cshtml (promotion creation and distribution)
│   └── Member.cshtml (user account and role management)
│
└── Home/
    ├── Index.cshtml (home page - alternate)
    ├── Privacy.cshtml (privacy policy)
    ├── lab8.cshtml (test/lab view)
```

### Key View Components

**_CartDrawer.cshtml**
- Side panel displaying selected products
- Shows item count and total price
- Supports quantity adjustment
- Linked to checkout process

**_Navbar.cshtml**
- User navigation menu
- Login/Logout links
- Links to Menu, Profile, Delivery Tracking, Checkout
- Conditional display based on authentication state

**_Dashbordnavbar.cshtml**
- Admin-specific navigation
- Links to Dashboard, Orders, Stock, Promotions, Members

**_ValidationScriptsPartial.cshtml**
- jQuery validation library
- Client-side form validation scripts

---

## 6. SPECIAL FEATURES AND INTEGRATIONS

### 6.1 Payment Integration - PromptPay
**Thai Mobile Banking Payment System**
- QR Code-based payment with 20-digit payload
- Supports both fixed and dynamic payment amounts
- QR image generated as PNG and embedded as Base64 in view
- Payment slip upload and verification workflow
  - Customer uploads bank transfer receipt
  - Admin verifies and confirms payment
  - Stock automatically reduced upon confirmation

**Payment workflow:**
1. Customer proceeds to checkout
2. OrderController.Payment() generates PromptPay QR code
3. Customer scans with Thai bank app (e.g., Promptpay, K Bank App)
4. Transfers money to bakery phone number
5. Screenshots/captures slip image
6. Uploads slip via OrderController.UploadSlip()
7. Slip saved to `wwwroot/uploads/slips/{filename}`
8. Admin verifies receipt and confirms in AdminOrderController.ConfirmPayment()
9. PaymentStatus changes to "Paid", stock decreases

### 6.2 Stock Management
**Real-time inventory tracking**
- Low stock alerts on admin dashboard (≤10 units)
- Out of stock warnings (≤0 units)
- Stock automatically decremented when admin confirms payment
- Prevents overselling by manual confirmation process
- Category-based organization
- Stock summaries by category on dashboard

**Stock workflow:**
```
Stock Management:
  1. Admin adds products with initial stock quantity
  2. Customer places order (stock NOT reduced yet)
  3. Admin confirms payment → Stock reduced by order quantity
  4. Dashboard shows low stock alerts
  5. Admin can restock by editing product
```

### 6.3 Promotional System
**Multi-tier discount management**
- Create flexible promotions (percent or fixed baht amount)
- Distribute to individual users or broadcast to all users
- Track usage per user per promotion
- Reset/reuse promotions for future campaigns
- Integration with order creation (PromotionId optional in order)

**Features:**
- Discount Types: 
  - Percent: e.g., 10% off
  - Baht: e.g., ฿50 discount
- One-time use tracking per user per promotion
- UsedAt timestamp for audit trail
- Can be gifted, reset, and re-gifted

### 6.4 Order Status Management
**Multi-stage order fulfillment pipeline**
```
Order Lifecycle:
Pending (created) 
  → Paid (payment verified) 
  → Preparing (being baked) 
  → Shipped (delivered to customer) 
  → Completed (customer confirmed receipt)
  
Alternative:
  → Cancelled (order cancelled)
```

**Status Controls:**
- CustomerCreates Order → Status: Pending, PaymentStatus: Pending
- CustomerUploads Slip → SlipImagePath saved
- AdminConfirmsPayment → PaymentStatus: Paid, Status: Paid, Stock reduced
- AdminAccepts Order → Status: Preparing
- AdminShips Order → Status: Shipped
- CustomerReceives → Status: Completed (with Historyorder record)

### 6.5 Delivery Tracking
**Real-time order tracking for customers**
- Order status visibility at each stage
- ETA estimation (placeholder)
- Concatenated delivery address display
- Item summary with quantities
- Order history (past orders in Historyorder table)
- Supports multiple concurrent orders per user

### 6.6 User Address Management
**Multi-address support**
- Store multiple delivery addresses per user
- Address components: AddressLine, District, Province, PostalCode
- Select address during checkout
- Full CRUD operations (Create, Read, Update, Delete)
- Prevents cross-user access via ownership validation

### 6.7 Session-Based Authentication
**Stateless, session-driven user identification**
- 30-minute idle timeout
- Stores UserId and Username in HttpContext.Session
- Role-based access control (RoleId: 1=Admin, 3=User)
- Used instead of ASP.NET Identity for simplicity
- Login attempt logging with IP addresses
- Log files: `wwwroot/logs/login_YYYY-MM-DD.log`

**Log format:**
```
[2026-03-22 14:30:45] Username: john_doe | Success: True | IP: 192.168.1.100
```

### 6.8 Admin Dashboard Analytics
**KPI monitoring and business intelligence**
- Real-time metrics calculation
- Today's revenue tracking (paid orders only)
- Order count by status (Pending, Paid, Preparing, Shipped, Completed)
- 6-month revenue trend chart
- Low stock alerts (top 6 items)
- Category inventory summaries
- Recent orders display (last 6)
- User count tracking

---

## 7. TECHNICAL ARCHITECTURE

### Technology Stack
- **Framework:** ASP.NET Core (.NET 10)
- **ORM:** Entity Framework Core (Pomelo.EntityFrameworkCore.MySql)
- **Database:** MySQL 8.0.43
- **Frontend:** Razor Views, HTML/CSS/JavaScript, Bootstrap
- **Payment:** PromptPay (QRCoder library)
- **Session:** ASP.NET Core Session middleware
- **Logging:** ILogger<T> and file-based logging

### Project Dependencies
- Microsoft.AspNetCore.Mvc
- Microsoft.EntityFrameworkCore
- Pomelo.EntityFrameworkCore.MySql
- QRCoder (for PromptPay QR generation)
- Bootstrap (CSS framework)
- jQuery/jQuery.Validation (client-side)

### Database Schema Features
- UTF-8MB4 character set (Thai language support)
- Foreign key constraints
- ENUM types for order status
- Datetime default values (CURRENT_TIMESTAMP)
- Precision decimal fields (10,2) for currency
- Indexed foreign keys for performance

### Folder Structure
```
Project-Bagery/
├── Controllers/
│   ├── AccountController.cs
│   ├── HomeController.cs
│   ├── OrderController.cs
│   ├── ProfileController.cs
│   ├── DeliveryController.cs
│   ├── Lab8Controller.cs
│   └── Admin/
│       ├── AdminControllerBase.cs
│       ├── AdminDashboardController.cs
│       ├── AdminOrderController.cs
│       ├── AdminStockController.cs
│       ├── AdminPromotionController.cs
│       └── AdminMemberController.cs
├── Models/
│   ├── Db/ (Entity Framework models)
│   │   ├── BakerydbContext.cs
│   │   ├── User.cs, Order.cs, Stock.cs, etc.
│   ├── ErrorViewModel.cs
│   └── (ViewModels)
│       ├── OrderViewModel.cs
│       ├── ProfileViewModel.cs
│       ├── DeliveryTrackingViewModel.cs
│       ├── AdminDashboardViewModel.cs
│       ├── AdminPromotionViewModel.cs
│       └── AdminStockViewModel.cs
├── Helpers/
│   └── PromptPayHelper.cs
├── Views/
│   ├── Account/
│   ├── admin/
│   ├── Home/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── lib/
│   ├── uploads/slips/ (payment proof storage)
│   └── logs/ (login attempt logs)
├── Migrations/ (EF Core migrations)
├── Program.cs (app configuration)
├── appsettings.json
└── 66022380.csproj
```

---

## 8. WORKFLOW EXAMPLES

### Complete Order Flow
```
1. User Registration (AccountController.Signup)
   → New User created with RoleId=3

2. Browse Products (HomeController.Menu)
   → Display all Stock with Categories

3. Add to Cart & Checkout (OrderController.Checkout)
   → Select delivery address
   → Apply promotion (if available)

4. Create Order (OrderController.CreateOrder via AJAX)
   → Order created (Status: Pending, PaymentStatus: Pending)
   → OrderDetails created for each item
   → Promotion marked as used

5. Payment (OrderController.Payment)
   → Generate PromptPay QR code
   → Customer scans and transfers via bank app
   → Upload payment slip (OrderController.UploadSlip)
   → Slip saved to server

6. Admin Confirmation (AdminOrderController)
   → View pending orders
   → Verify payment slip
   → ConfirmPayment: Status→Paid, Stock reduced
   → AcceptOrder: Status→Preparing
   → ShipOrder: Status→Shipped

7. Customer Delivery (DeliveryController.Delivery)
   → Track order status in real-time
   → Confirm receipt: CompleteOrder
   → Status→Completed
   → Order archived to Historyorder

8. Order History (DeliveryController.Delivery)
   → View past orders
   → See delivery addresses used
   → Track historical spending
```

### Admin Management Workflows

**Stock Management:**
```
Admin → AdminStockController.Stock
  → Add Category (SaveCategory)
  → Add Product to Category (SaveStock)
  → View inventory dashboard (AdminDashboardController.Dashbordadmin)
  → Monitor low stock alerts
  → Re-stock products as needed
```

**Promotion Management:**
```
Admin → AdminPromotionController.PromotionAdmin
  → Create promotion with discount details (SavePromotion)
  → Gift to specific users (GiftPromotion)
  → Broadcast to all users (GiftPromotionToAll)
  → Monitor usage statistics
  → Reset for future campaigns
```

**User Management:**
```
Admin → AdminMemberController.Member
  → Create new user account (SaveMember)
  → Assign roles (1=Admin, 3=User)
  → Update user info
  → Delete inactive users
```

---

## 9. SECURITY CONSIDERATIONS

### Current Implementation
- ✅ Session-based authentication with 30-min timeout
- ✅ Role-based authorization (Admin check via RoleId)
- ✅ Ownership verification (address, promotion operations)
- ✅ Password validation on change (current password required)

### Noted Vulnerabilities
- ⚠️ Passwords stored in plain-text (no hashing)
- ⚠️ No CSRF protection visible on form submissions
- ⚠️ No input sanitization against XSS
- ⚠️ Hardcoded database credentials in code
- ⚠️ Direct SQL operations without parameterization visible
- ⚠️ All user data accessible via Session.GetString("UserId")

### Recommendations
- Implement password hashing (BCrypt, PBKDF2, or Argon2)
- Add [ValidateAntiForgeryToken] to all POST actions
- Implement HTTPS enforcement
- Use secure session cookie settings (Secure, HttpOnly, SameSite)
- Move credentials to appsettings.json (configuration system)
- Add input validation attributes on ViewModels
- Implement rate limiting on login attempts

---

## 10. CUSTOMIZATION POINTS

### Adding New Features

**New Product Category:**
- Add to Category table via AdminStockController.SaveCategory()
- Products auto-appear in Menu grouped by category

**New Discount Type:**
- Modify DiscountType validation in AdminPromotionController.SavePromotion()
- Currently supports: "Percent", "Baht"
- Can extend to: "Tiered", "BuyXGetY", etc.

**New Order Status:**
- Modify Order.Status ENUM in database migration
- Update status transition logic in AdminOrderController
- Add matching status in DeliveryTrackingViewModel

**New Payment Method:**
- Create parallel helper class to PromptPayHelper
- Add payment method selection in Payment view
- Implement workflow in OrderController

**New Admin Reports:**
- Create new ViewModel based on AdminDashboardViewModel
- Add action method to AdminDashboardController
- Create corresponding view file
- Add navigation link to _Dashbordnavbar

---

## 11. DEPLOYMENT NOTES

### Required Configuration
- MySQL 8.0+ with bakerydb database
- Database user: root, password: 1234 (update in BakerydbContext.OnConfiguring)
- Directory structure:
  - `wwwroot/logs/` – for login attempt logs (auto-created)
  - `wwwroot/uploads/slips/` – for payment proof images (auto-created)

### Build & Run
```bash
dotnet build
dotnet ef database update  # Apply migrations
dotnet run
```

### Typical Server Setup
- Deploy to IIS/Linux with Kestrel reverse proxy
- Configure HTTPS certificates
- Set environment to Production
- Implement proper error handling
- Configure logging severity
- Set up database backups
- Monitor disk space for uploaded slips

---

## Summary Statistics

| Aspect | Count |
|--------|-------|
| Controllers | 7 (1 home, 1 order, 1 profile, 1 delivery, 1 account, 3 admin) |
| Database Tables | 10 (User, Order, Orderdetail, Stock, Category, Address, Promotion, UserPromotion, Role, Historyorder) |
| Action Methods | 40+ (across all controllers) |
| ViewModels | 6 (Order, Profile, Delivery, AdminDashboard, AdminPromotion, AdminStock) |
| Views | 15+ (organized by feature area) |
| Helper Classes | 1 (PromptPayHelper) |
| External Libraries | 3 (QRCoder, Bootstrap, jQuery) |



