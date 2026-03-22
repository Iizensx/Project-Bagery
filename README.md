# Project Bagery 🥐

ระบบจัดการร้านเบเกอรี่แบบเต็มรูปแบบ (Full-stack) ที่พัฒนาด้วย **ASP.NET Core MVC (.NET 10)** เชื่อมต่อฐานข้อมูล **MySQL** 
ใช้สำหรับจัดการการขายสินค้า การสั่งซื้อ การชำระเงินด้วย **PromptPay QR** การติดตามสถานะการจัดส่งแบบ real-time และหลังบ้านบริหารจัดการแอดมิน

---

## 🎯 ภาพรวมระบบ

| หัวข้อ | รายละเอียด |
|--------|-----------|
| **Backend** | C# / ASP.NET Core MVC (.NET 10) |
| **ORM** | Entity Framework Core |
| **Database** | MySQL 8.0 (UTF-8MB4 encoding) |
| **Frontend** | Razor Views + Bootstrap 5 + JavaScript |
| **Payment** | PromptPay QR Code (QRCoder) + Manual Slip Upload |
| **Authentication** | Session-based (30-min timeout) with Role-based Access |
| **Timezone** | ไทย (UTF8MB4) |

### 🚪 จุดเข้าใช้งานหลักของระบบ

| เส้นทาง | วัตถุประสงค์ |
|---------|-----------|
| `Home/Home` | หน้าแรกของระบบ |
| `Account/Login` | เข้าสู่ระบบ (ผู้ใช้ & แอดมิน) |
| `Account/Signup` | ลงทะเบียนสมาชิกใหม่ |
| `Account/Home` | หน้าหลักหลังเข้าสู่ระบบ |
| `Account/Menu` | ดูรายการเมนู (สินค้าทั้งหมด) |
| `Account/Checkout` | หน้าสรุปก่อนสร้างออเดอร์ |
| `Order/Payment` | หน้าชำระเงิน (PromptPay QR) |
| `Account/Delivery` | ติดตามสถานะและประวัติออเดอร์ |
| `Account/Profile` | จัดการโปรไฟล์และที่อยู่ |
| `Admin/Dashbordadmin` | Dashboard หลังบ้าน (Admin only) |

---

## 📁 โครงสร้างโปรเจกต์

```
Project-Bagery/
├── Controllers/                    // ควบคุม business logic หลัก
│   ├── AccountController.cs        // Login, Signup,Menu,Profile,Payment confirmation
│   ├── HomeController.cs           // Homepage, Contact  
│   ├── OrderController.cs          // Checkout, CreateOrder, Payment, Slip upload
│   ├── ProfileController.cs        // Profile management, Address CRUD
│   ├── DeliveryController.cs       // Tracking orders and delivery status
│   ├── Lab8Controller.cs           // Testing/Lab pages
│   └── Admin/                      // Admin-only controllers
│       ├── AdminControllerBase.cs  // Base class with auth check
│       ├── AdminDashboardController.cs  // KPI Dashboard
│       ├── AdminOrderController.cs      // Order management
│       ├── AdminStockController.cs      // Stock & Category management
│       ├── AdminPromotionController.cs  // Promotion management
│       └── AdminMemberController.cs     // User management
│
├── Models/
│   ├── Db/                         // Database entities (Created by EF Core)
│   │   ├── BakerydbContext.cs      // DbContext
│   │   ├── User.cs                 // ผู้ใช้
│   │   ├── Role.cs                 // บทบาท (Admin, Staff, User)
│   │   ├── Address.cs              // ที่อยู่จัดส่ง
│   │   ├── Stock.cs                // สินค้า
│   │   ├── Category.cs             // หมวดหมู่
│   │   ├── Order.cs                // หัวออเดอร์
│   │   ├── Orderdetail.cs          // รายการสินค้าในออเดอร์
│   │   ├── Promotion.cs            // โปรโมชัน
│   │   ├── UserPromotion.cs        // โปรโมชันที่แจกให้ผู้ใช้
│   │   └── Historyorder.cs         // ประวัติออเดอร์ที่ปิดงาน
│   │
│   └── ErrorViewModel.cs           // Error viewmodel
│
├── Viewmodels/                     // ViewModels สำหรับ Views
│   ├── OrderViewModel.cs           // Order data model
│   ├── ProfileViewModel.cs         // Profile update model  
│   ├── DeliveryTrackingViewModel.cs // Delivery tracking model
│   ├── AdminDashboardViewModel.cs   // Dashboard KPI data
│   ├── AdminStockViewModel.cs       // Stock management model
│   └── AdminPromotionViewModel.cs   // Promotion management model
│
├── Views/
│   ├── Account/                    // ฝั่งลูกค้า
│   │   ├── Home.cshtml             // หน้าแรก
│   │   ├── Login.cshtml            // เข้าสู่ระบบ
│   │   ├── Signup.cshtml           // ลงทะเบียน
│   │   ├── Menu.cshtml             // ดูเมนู/สินค้า
│   │   ├── Checkout.cshtml         // ยืนยันก่อนสั่ง
│   │   ├── Payment.cshtml          // ชำระเงิน (PromptPay QR)
│   │   ├── Delivery.cshtml         // ติดตามจัดส่ง
│   │   ├── Profile.cshtml          // จัดการโปรไฟล์
│   │   └── Test1.cshtml            // หน้าทดสอบ
│   │
│   ├── admin/                      // หลังบ้าน (Admin only)
│   │   ├── Dashbordadmin.cshtml    // Dashboard
│   │   ├── Order.cshtml            // จัดการออเดอร์
│   │   ├── Stock.cshtml            // จัดการสต็อก/หมวดหมู่
│   │   ├── PromotionAdmin.cshtml   // จัดการโปรโมชัน
│   │   └── Member.cshtml           // จัดการผู้ใช้
│   │
│   ├── Home/                       // Shared pages
│   │   ├── Index.cshtml
│   │   ├── Privacy.cshtml
│   │   └── lab8.cshtml
│   │
│   └── Shared/                     // Layout & Partials
│       ├── _Layoutmain.cshtml      // Main layout ฝั่งลูกค้า
│       ├── _AdminLayout.cshtml     // Layout หลังบ้าน
│       ├── _Navbar.cshtml          // Menu bar ฝั่งลูกค้า
│       ├── _Dashbordnavbar.cshtml  // Menu bar หลังบ้าน
│       ├── _CartDrawer.cshtml      // Cart sidebar
│       ├── _Footer.cshtml          // Footer
│       ├── _ViewStart.cshtml
│       ├── _ViewImports.cshtml
│       ├── Error.cshtml
│       └── _ValidationScriptsPartial.cshtml
│
├── Helpers/
│   └── PromptPayHelper.cs          // PromptPay QR code generation (EMV standard)
│
├── Migrations/                     // EF Core migrations
│   ├── [Date]_AddPreparingStatusToOrder.cs
│   ├── [Date]_AddCompletedStatusToOrder.cs
│   ├── [Date]_AddHistoryorderTable.cs
│   ├── BakerydbContextModelSnapshot.cs
│   └── SQL/                        // SQL scripts
│
├── wwwroot/                        // Static assets
│   ├── css/
│   │   └── site.css                // Styling หลัก
│   ├── js/
│   │   └── site.js                 // JavaScript logic
│   ├── img/                        // Images/logos
│   ├── uploads/
│   │   └── slips/                  // Payment slips (uploaded)
│   ├── logs/                       // Login logs (runtime)
│   └── lib/                        // Third-party libraries
│       ├── bootstrap/
│       ├── jquery/
│       └── jquery-validation/
│
├── Properties/
│   └── launchSettings.json         // URL config (5082, 7070)
│
├── Program.cs                      // Application startup & middleware
├── appsettings.json                // Config & connection string
├── 66022380.csproj                 // Project file & dependencies
├── 66022380.sln                    // Solution file
├── bakerydb.sql                    // Database SQL dump
└── README.md                       // This file
```

---

## 💾 ฐานข้อมูล (10 ตาราง)

| Table | Purpose | Key Fields |
|-------|---------|-----------|
| **user** | ข้อมูลผู้ใช้ | UserId, Username, Email, Phone, Password (hash), RoleId, CreatedDate |
| **role** | บทบาทผู้ใช้ | RoleId (1=Admin, 3=User) |
| **address** | ที่อยู่จัดส่ง | AddressId, UserId, AddressLine, District, Province, PostalCode, IsDefault |
| **stock** | สินค้า | StockId, StockName, Description, Price, Quantity, CategoryId, Image |
| **category** | หมวดหมู่ | CategoryId, CategoryName |
| **order** | หัวออเดอร์ | OrderId, UserId, OrderDate, TotalPrice, Status, PaymentStatus, AddressId |
| **orderdetail** | รายการในออเดอร์ | OrderDetailId, OrderId, StockId, Quantity, Price |
| **promotion** | โปรโมชัน | PromotionId, PromotionName, DiscountType (%), DiscountValue, StartDate, EndDate |
| **user_promotion** | แจกโปรให้ผู้ใช้ | UserPromotionId, UserId, PromotionId, IsUsed |
| **historyorder** | ประวัติออเดอร์ | HistoryOrderId, OrderId (copied), เข้าหลังจบออเดอร์ |

### 📊 Order Status Pipeline

```
Pending → Paid → Preparing → Shipped → Completed
                                    ↓ (User Confirm)
                            Historyorder (Archive)
                         
(Anytime) → Cancelled
```

### 💳 Payment Status

- **Pending**: ยังไม่ชำระ
- **PendingVerify**: อัปโหลดสลิปแล้ว รอแอดมินยืนยัน
- **Paid**: แอดมินยืนยันแล้ว

---

## ⚙️ Controllers และ Actions

### 1. **AccountController** (Shared - Customer + Admin)

| Action | HTTP | วัตถุประสงค์ |
|--------|------|-----------|
| `Home()` | GET | หน้าแรก |
| `Login()` | GET/POST | เข้าสู่ระบบ + บันทึก IP log |
| `Signup()` | GET/POST | ลงทะเบียนสมาชิก (RoleId=3) |
| `Logout()` | GET | ออกจากระบบ |
| `Menu()` | GET | แสดงรายการสินค้า (filter by category) |
| `Profile()` | GET/POST | ดูและแก้ไขข้อมูลโปรไฟล์ |

### 2. **OrderController** (Customer)

| Action | HTTP | วัตถุประสงค์ |
|--------|------|-----------|
| `Checkout()` | GET | หน้าสรุปก่อนสั่ง |
| `CreateOrder()` | POST | สร้างออเดอร์ + OrderDetails |
| `Payment()` | GET | สร้าง PromptPay QR code |
| `UploadSlip()` | POST | อัปโหลดหลักฐานการชำระเงิน |
| `GetCurrentUser()` | GET | ข้อมูลผู้ใช้ปัจจุบัน (API) |
| `GetUserPromos()` | GET | โปรโมชันของผู้ใช้ (API) |

### 3. **ProfileController** (Customer)

| Action | HTTP | วัตถุประสงค์ |
|--------|------|-----------|
| `Profile()` | GET/POST | ดู/แก้ไขโปรไฟล์ + รหัสผ่าน |
| `SaveAddress()` | POST | เพิ่ม/แก้ไขที่อยู่ |
| `DeleteAddress()` | POST | ลบที่อยู่ |
| `GetUserAddresses()` | GET | ดึงที่อยู่ทั้งหมดของผู้ใช้ (API) |

### 4. **DeliveryController** (Customer)

| Action | HTTP | วัตถุประสงค์ |
|--------|------|-----------|
| `Delivery()` | GET | ดูสถานะจัดส่งและประวัติ |
| `UpdateStatus()` | POST | อัปเดตสถานะออเดอร์ (Admin) |
| `GetDeliveryStatus()` | GET | ข้อมูลสถานะปัจจุบัน (API) |

### 5. **Admin Controllers** (Admin only - RoleId == 1)

#### **AdminDashboardController**
- `Dashboard()` - Show KPIs: Revenue (today/month), Order counts, Stock alerts

#### **AdminOrderController**
- `Order()` - View all orders
- `AcceptOrder()` - Change to "Preparing"
- `ShipOrder()` - Change to "Shipped"  
- `ConfirmPayment()` - Verify payment + deduct stock
- `CompleteOrder()` - Archive to historyorder
- `CancelOrder()` - Cancel order

#### **AdminStockController**
- `Stock()` - View/Create/Edit stock items
- `AddCategory()` - Add product category
- `LowStockAlerts()` - Items with qty ≤ 10

#### **AdminPromotionController**
- `Promotion()` - View/Create promotions
- `DistributePromo()` - Assign to users
- `DeletePromo()` - Remove promotion

#### **AdminMemberController**
- `Member()` - View/Edit users
- `ChangeRole()` - Change user role
- `LockUnlockUser()` - Disable/enable account

### 6. **HomeController** (General)
- `Home()` - Homepage
- `Menu()` - Products listing
- `Contact()` - Contact page
- `Privacy()` - Privacy policy

### 7. **Lab8Controller** (Testing)
- `Index()` / `Privacy()` / `Error()` - Lab pages

---

## 🎨 ViewModels (6 models)

| ViewModel | ใช้ที่ | Properties |
|-----------|-------|-----------|
| **OrderViewModel** | Checkout, Payment | Items[], UserId, TotalPrice, PromotionId, Address |
| **ProfileViewModel** | Profile page | UserId, Username, Email, Phone, Password, Addresses[] |
| **DeliveryTrackingViewModel** | Delivery page | Orders[], HistoryOrders[], CurrentStatus |
| **AdminDashboardViewModel** | Admin dashboard | TodayRevenue, MonthRevenue, PendingOrders, LowStockItems[] |
| **AdminStockViewModel** | Stock management | StockList[], CategoryList[], SearchTerm |
| **AdminPromotionViewModel** | Promotion mgmt | PromotionList[], UserList[], DistributionHistory[] |

---

## 🔐 Authentication & Authorization

### Session-Based Auth
- **Duration**: 30 minutes idle timeout
- **Storage**: Session variables (`UserId`, `Username`, `RoleId`)
- **Enforcement**: Role check in controller (`if (RoleId != 1) redirect to Home`)

### Roles
- **RoleId = 1**: Admin (full access to admin controllers)
- **RoleId = 3**: User (access to customer features only)

### Login Flow
```
1. User enters username + password
2. Query user table
3. If found & password match:
   - Store UserId, Username, RoleId in session
   - Log IP to wwwroot/logs/login_YYYY-MM-DD.log
   - Redirect to dashboard (Admin) or Home (User)
4. Else: Show error
```

---

## 💳 Payment Integration (PromptPay)

### Implementation
- **Library**: QRCoder NuGet package
- **Standard**: EMV QR Code Format
- **Helper**: `Helpers/PromptPayHelper.cs`

### Flow
```
1. Order created (Status=Pending, PaymentStatus=Pending)
2. User goes to Payment page
3. System generates PromptPay QR with:
   - Order ID
   - Amount
   - Shop PromptPay ID/Phone
4. User scans QR → Mobile banking
5. User takes screenshot → Upload slip
6. Admin verifies slip → Click "ConfirmPayment"
7. Stock deducted, Order status → Paid
```

### QR Payload (Example)
```
00020126...10330066300012A0000000365840010502050403402040063...61111112890204admin0607401330802D0310123...626
(EMV format: merchant info, amount, Thai bank, etc.)
```

---

## 📱 User Flows

### 🛒 Customer - Order to Delivery

```
1. Signup/Login
   └─ Profile created (RoleId=3, Session)

2. Browse Menu
   └─ View stock by category

3. Add to Cart (Frontend localStorage/session)
   └─ See promotions

4. Checkout
   └─ Select delivery address
   └─ Apply promotion (discount %)
   └─ Review total

5. Create Order
   └─ OrderId generated
   └─ OrderDetails saved
   └─ Status: Pending

6. Payment
   └─ View PromptPay QR
   └─ User scans → Mobile banking payment
   └─ Upload payment slip

7. Admin Confirms Payment
   └─ Verify slip
   └─ Deduct stock
   └─ Status: Paid

8. Delivery Tracking
   └─ Status: Preparing (Admin accepts)
   └─ Status: Shipped (Admin ships)
   └─ Status: Shipped → Completed (User confirms receipt)
   └─ Archive to Historyorder

9. Order History
   └─ View in Delivery page
   └─ Access past orders
```

### 🔧 Admin - Order Management

```
Admin Dashboard
├─ View KPIs
│  ├─ Today Revenue
│  ├─ Monthly Revenue  
│  ├─ Orders by status
│  └─ Low stock alerts

Order Management
├─ View all orders (list/grid)
├─ Verify payment slip
├─ Confirm payment → Deduct stock
├─ Accept order → Status: Preparing
├─ Ship order → Status: Shipped
└─ Complete order → Archive

Stock Management
├─ Add/Edit/Delete items
├─ Manage categories
├─ Stock quantity alerts (≤10 units)
└─ Upload product images

Promotion Management
├─ Create promotion (% or ฿ discount)
├─ Set validity period
├─ Distribute to users
├─ Track usage

Member Management
├─ View all users
├─ Change user role
├─ Lock/unlock accounts
└─ View user purchase history
```

---

## ⚡ Features & Highlights

| Feature | Status | Implementation |
|---------|--------|-----------------|
| **PromptPay Payment** | ✅ | EMV QR format, manual slip verification |
| **Real-time Order Tracking** | ✅ | 6-stage pipeline with status updates |
| **Admin Dashboard KPIs** | ✅ | Revenue, order counts, stock alerts |
| **Stock Management** | ✅ | Inventory tracking, low-stock alerts (≤10 units) |
| **Promotion System** | ✅ | % or ฿ discounts, per-user distribution |
| **Multi-Address Support** | ✅ | AddressLine + District + Province + PostalCode |
| **Order History** | ✅ | Archive completed orders to historyorder table |
| **Role-Based Access** | ✅ | Admin (RoleId=1) vs User (RoleId=3) |
| **Session Authentication** | ✅ | 30-min timeout, IP logging |
| **Category Filtering** | ✅ | Browse by product category |
| **Search** | ⚙️ | Can be extended |
| **Email Notifications** | ⚠️ | Not implemented |
| **SMS Alerts** | ⚠️ | Not implemented |
| **Payment Timeout** | ⚠️ | Auto-cancel after X hours |

---

## 🔧 Configuration & Setup

### Database Connection
**File**: `appsettings.json`
```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=bakerydb;user=root;password=1234;SslMode=none;;AllowPublicKeyRetrieval=true;"
}
```

### Local URLs
**File**: `Properties/launchSettings.json`
- HTTP: `http://localhost:5082`
- HTTPS: `https://localhost:7070`

### Session Configuration
**File**: `Program.cs`
- Idle timeout: **30 minutes**
- HttpOnly cookie: **enabled**
- Essential cookie: **enabled**

### Middleware Pipeline
```
HTTP Request
  ↓
HTTPS Redirect
  ↓
Static Files
  ↓
Session Middleware
  ↓
Routing
  ↓
Authorization/Authentication
  ↓
Controller Action
```

---

## 🚀 How to Run

### Prerequisites
- **.NET SDK**: 10.0+
- **MySQL**: 8.0+
- **Visual Studio Code** or **Visual Studio 2022+**

### Steps

1. **Clone/Open Project**
   ```bash
   cd Project-Bagery
   ```

2. **Configure Database Connection**
   - Edit `appsettings.json`
   - Set MySQL username/password
   - Create empty `bakerydb` database

3. **Apply Migrations**
   ```bash
   dotnet ef database update
   ```
   - Or run `bakerydb.sql` script directly

4. **Install Dependencies**
   ```bash
   dotnet restore
   ```

5. **Run Application**
   ```bash
   dotnet run
   ```
   - Visit: `https://localhost:7070`

6. **Test**
   - **Admin**: Create user with RoleId=1 or edit existing
   - **User**: Signup with default RoleId=3

---

## 📋 Database Setup

### SQL Script
File: `bakerydb.sql` (ready to import)
```sql
CREATE DATABASE bakerydb;
-- Tables will be created by EF Core migrations
```

### EF Core Migrations
Located in `Migrations/` folder:
- `20260319191210_AddPreparingStatusToOrder`
- `20260320093000_AddCompletedStatusToOrder`
- `20260320103000_AddHistoryorderTable`

### Manual SQL Changes
- File: `Migrations/SQL/` contains any manual SQL updates

---

## 🔍 File Structure Details

### Controllers Deep Dive

#### `AccountController.cs` - Mixed Customer/Admin
- Handles login/signup for both user types
- User creation assigns RoleId=3 by default
- Password stored as hash (recommend: bcrypt)

#### `OrderController.cs` - Pure Customer
- Order creation validates stock & applies promos
- PromptPay QR generation uses `PromptPayHelper`
- Slip upload saves to `wwwroot/uploads/slips/`

#### `ProfileController.cs` - Customer Profile Management
- Supports multiple addresses per user
- Password update validation
- Address CRUD operations

#### `DeliveryController.cs` - Tracking
- Real-time status updates
- Transition from Shipped → Completed archives to historyorder
- Timeline view of order stages

#### `Admin Controllers` - Fully Separated
- `AdminControllerBase` enforces RoleId==1 check
- Each admin function is isolated by responsibility
- Dashboard shows business KPIs

### Views Organization

#### Customer Views (`Views/Account/`)
- `Home.cshtml` - Dashboard after login
- `Menu.cshtml` - Product listing with category filter
- `Checkout.cshtml` - Order review before payment
- `Payment.cshtml` - PromptPay QR display + slip upload
- `Delivery.cshtml` - Real-time tracking + history
- `Profile.cshtml` - User info + address management
- `Login/Signup.cshtml` - Auth pages

#### Admin Views (`Views/admin/`)
- `Dashbordadmin.cshtml` - KPI cards + charts
- `Order.cshtml` - Order list + status mgmt
- `Stock.cshtml` - Inventory + categories
- `PromotionAdmin.cshtml` - Promo creation + distribution
- `Member.cshtml` - User list + role management

#### Shared (`Views/Shared/`)
- `_Layoutmain.cshtml` - Customer theme
- `_AdminLayout.cshtml` - Admin theme (dark/different style)
- Navigation bars, footer, shared partials

### Static Assets (`wwwroot/`)
- **CSS**: `site.css` (Bootstrap customization)
- **JS**: `site.js` (Frontend logic: cart, validation, API calls)
- **Uploads**: `uploads/slips/` (Payment proof images)
- **Logs**: `logs/` (IP + login timestamps)

### Helpers

#### `PromptPayHelper.cs`
Generates EMV QR code payload for PromptPay:
```csharp
public static string GenerateQRCode(
    decimal amount,
    string merchantPromptPay,
    string reference)
{
    // EMV QR format construction
    // Returns QR string for QRCoder to render
}
```

---

## 🛡️ Security Notes

### Current State ✅
- Session-based authentication
- SQL stored via hashing (need to verify bcrypt usage)
- HTTPS enforced (HSTS enabled)
- HttpOnly cookies for session
- SQL injection protection via EF Core parameterized queries

### Recommendations ⚠️
- [ ] Use bcrypt/Argon2 for password hashing (not plain hash)
- [ ] Implement CSRF tokens in forms
- [ ] Add rate limiting on login endpoint
- [ ] Validate file uploads (payment slips)
- [ ] Implement input sanitization on text fields
- [ ] Add audit logging for admin actions
- [ ] Use HTTPS in production (currently dev only)
- [ ] Store sensitive config in secrets manager (not appsettings.json)
- [ ] Implement email verification for signup
- [ ] Add 2FA for admin accounts

---

## 🎯 Next Steps / Future Enhancements

1. **Notifications**
   - Email confirmation for order placement
   - SMS reminder for pending payment
   - Push notifications for delivery updates

2. **Reporting**
   - Monthly sales report (PDF export)
   - Customer spending trends
   - Popular products analysis

3. **Integration**
   - Real payment gateway (Omise, 2C2P, etc.)
   - Google Maps for delivery tracking
   - Email service (SendGrid, etc.)

4. **Performance**
   - Redis cache for stock/promo data
   - Database query optimization
   - Image optimization

5. **UI/UX**
   - Mobile-first responsive design
   - Dark mode
   - Progressive Web App (PWA)

6. **Testing**
   - Unit tests for biz logic
   - Integration tests for payment flow
   - End-to-end tests for order lifecycle

---

## 📞 Support & Contact

ระบบนี้เป็นโปรเจกต์จำนวน 66022380 สำหรับการศึกษา และการพัฒนาต่อ

---

**Last Updated**: March 2026
**Framework**: ASP.NET Core MVC (.NET 10)
**Database**: MySQL 8.0
**Status**: Active Development
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