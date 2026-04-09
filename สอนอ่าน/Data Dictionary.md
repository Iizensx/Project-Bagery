# Data Dictionary

## 1. ภาพรวมการออกแบบฐานข้อมูล

ฐานข้อมูลของระบบนี้ใช้ชื่อ `bakerydb` และออกแบบสำหรับระบบร้านเบเกอรี่ที่รองรับงานหลักดังนี้

- จัดการผู้ใช้งานและสิทธิ์การเข้าถึง
- จัดการข้อมูลสินค้าและหมวดหมู่
- จัดเก็บที่อยู่จัดส่งของลูกค้า
- จัดการคำสั่งซื้อ รายการสินค้าในคำสั่งซื้อ และสถานะการชำระเงิน
- จัดเก็บประวัติคำสั่งซื้อที่เสร็จสมบูรณ์
- จัดการโปรโมชัน คูปอง ของแถม และการเคลมโปรโมชัน

ลักษณะการออกแบบเป็นฐานข้อมูลเชิงสัมพันธ์ (Relational Database) บน MySQL โดยใช้ Foreign Key เชื่อมความสัมพันธ์ระหว่างตารางเพื่อรักษาความถูกต้องของข้อมูล

## 2. ตารางที่ใช้ในระบบ

ตารางหลักที่ระบบใช้งานปัจจุบันประกอบด้วย

1. `role`
2. `user`
3. `address`
4. `category`
5. `stock`
6. `promotion`
7. `promotion_reward_item`
8. `promotion_claim`
9. `user_promotion`
10. `order`
11. `orderdetail`
12. `historyorder`

## 3. ความสัมพันธ์ของข้อมูล

- `role` 1:N `user`
- `user` 1:N `address`
- `user` 1:N `order`
- `address` 1:N `order`
- `promotion` 1:N `order`
- `order` 1:N `orderdetail`
- `stock` 1:N `orderdetail`
- `category` 1:N `stock`
- `stock` 1:N `promotion_reward_item`
- `promotion` 1:N `promotion_reward_item`
- `promotion` 1:N `promotion_claim`
- `user` 1:N `promotion_claim`
- `user` N:M `promotion` ผ่านตาราง `user_promotion`
- `user` 1:N `historyorder`

## 4. Data Dictionary รายตาราง

### 4.1 ตาราง `role`

ใช้เก็บประเภทสิทธิ์ของผู้ใช้งานในระบบ

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `RoleId` | `int` | PK | No | Auto Increment | รหัสบทบาท |
| `RoleName` | `enum('Admin','Staff','User')` | - | No | - | ชื่อบทบาทของผู้ใช้ |

### 4.2 ตาราง `user`

ใช้เก็บข้อมูลสมาชิก ผู้ดูแลระบบ และพนักงาน

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `UserId` | `int` | PK | No | Auto Increment | รหัสผู้ใช้ |
| `Username` | `varchar(50)` | - | No | - | ชื่อผู้ใช้สำหรับเข้าสู่ระบบ |
| `Password` | `varchar(255)` | - | No | - | รหัสผ่านของผู้ใช้ |
| `Email` | `varchar(100)` | - | Yes | `NULL` | อีเมลติดต่อ |
| `Phone` | `varchar(20)` | - | Yes | `NULL` | เบอร์โทรศัพท์ |
| `RoleId` | `int` | FK | Yes | `NULL` | อ้างอิงไปยังตาราง `role` |

Foreign Key:

- `RoleId` อ้างอิง `role.RoleId`

### 4.3 ตาราง `address`

ใช้เก็บที่อยู่จัดส่งของลูกค้า โดยผู้ใช้ 1 คนสามารถมีหลายที่อยู่ได้

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `AddressId` | `int` | PK | No | Auto Increment | รหัสที่อยู่ |
| `UserId` | `int` | FK | Yes | `NULL` | รหัสผู้ใช้เจ้าของที่อยู่ |
| `AddressLine` | `varchar(255)` | - | Yes | `NULL` | รายละเอียดที่อยู่ |
| `District` | `varchar(100)` | - | Yes | `NULL` | เขตหรืออำเภอ |
| `Province` | `varchar(100)` | - | Yes | `NULL` | จังหวัด |
| `PostalCode` | `varchar(10)` | - | Yes | `NULL` | รหัสไปรษณีย์ |

Foreign Key:

- `UserId` อ้างอิง `user.UserId`

### 4.4 ตาราง `category`

ใช้เก็บหมวดหมู่สินค้า เช่น เค้ก คุกกี้ ขนมปัง หรือเครื่องดื่ม

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `CategoryId` | `int` | PK | No | Auto Increment | รหัสหมวดหมู่ |
| `CategoryName` | `varchar(100)` | - | No | - | ชื่อหมวดหมู่สินค้า |
| `Description` | `text` | - | Yes | `NULL` | รายละเอียดหมวดหมู่ |

### 4.5 ตาราง `stock`

ใช้เก็บข้อมูลสินค้า ราคา จำนวนคงเหลือ และหมวดหมู่สินค้า

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `ProductId` | `int` | PK | No | Auto Increment | รหัสสินค้า |
| `ProductName` | `varchar(100)` | - | No | - | ชื่อสินค้า |
| `Description` | `text` | - | Yes | `NULL` | รายละเอียดสินค้า |
| `Price` | `decimal(10,2)` | - | Yes | `NULL` | ราคาสินค้า |
| `Stock` | `int` | - | Yes | `0` | จำนวนสินค้าคงเหลือ |
| `CategoryId` | `int` | FK | Yes | `NULL` | รหัสหมวดหมู่สินค้า |
| `ImageUrl` | `varchar(255)` | - | Yes | `NULL` | URL รูปภาพสินค้า |

Foreign Key:

- `CategoryId` อ้างอิง `category.CategoryId`

### 4.6 ตาราง `promotion`

ใช้เก็บข้อมูลโปรโมชัน ทั้งส่วนลดทั่วไป โปรโมชันซื้อครบรับของแถม และโปรโมชันแบบกิจกรรม

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `PromotionId` | `int` | PK | No | Auto Increment | รหัสโปรโมชัน |
| `PromotionName` | `varchar(100)` | - | No | - | ชื่อโปรโมชัน |
| `Description` | `text` | - | Yes | `NULL` | รายละเอียดโปรโมชัน |
| `DiscountValue` | `decimal(10,2)` | - | No | - | มูลค่าส่วนลด |
| `DiscountType` | `enum('Percent','Fixed')` | - | No | - | ประเภทส่วนลด |
| `ImagePath` | `varchar(255)` | - | Yes | `NULL` | path รูปโปรโมชัน |
| `StartDate` | `datetime` | - | Yes | `NULL` | วันเริ่มใช้งานโปรโมชัน |
| `EndDate` | `datetime` | - | Yes | `NULL` | วันสิ้นสุดโปรโมชัน |
| `PromoType` | `int` | - | No | `1` | ประเภทโปรโมชัน เช่น ส่วนลดทั่วไป หรือโปรโมชันของแถม |
| `IsActive` | `tinyint(1)` | - | No | `1` | สถานะเปิดใช้งาน |
| `BuyQuantity` | `int` | - | Yes | `0` | จำนวนขั้นต่ำที่ต้องซื้อเพื่อเข้าเงื่อนไข |
| `RewardProductId` | `int` | FK | Yes | `NULL` | สินค้าที่ใช้เป็นของแถมหลัก |
| `RewardQuantity` | `int` | - | Yes | `0` | จำนวนของแถม |
| `IsCombinable` | `tinyint(1)` | - | No | `0` | ระบุว่าใช้ร่วมกับโปรอื่นได้หรือไม่ |
| `RequiresProof` | `tinyint(1)` | - | No | `0` | ระบุว่าต้องแนบหลักฐานเพื่อรับสิทธิ์หรือไม่ |
| `MaxUsePerUser` | `int` | - | No | `1` | จำนวนครั้งสูงสุดที่ผู้ใช้ 1 คนใช้ได้ |

Foreign Key:

- `RewardProductId` อ้างอิง `stock.ProductId`

### 4.7 ตาราง `promotion_reward_item`

ใช้เก็บรายการของแถมหลายรายการภายใต้โปรโมชันเดียว กรณีโปรโมชันมีของแถมมากกว่า 1 ชนิด

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `RewardItemId` | `int` | PK | No | Auto Increment | รหัสรายการของแถม |
| `PromotionId` | `int` | FK | No | - | รหัสโปรโมชัน |
| `ProductId` | `int` | FK | No | - | รหัสสินค้าในฐานะของแถม |
| `Quantity` | `int` | - | No | `1` | จำนวนของแถม |
| `SortOrder` | `int` | - | No | `0` | ลำดับการแสดงผล |

Foreign Key:

- `PromotionId` อ้างอิง `promotion.PromotionId`
- `ProductId` อ้างอิง `stock.ProductId`

### 4.8 ตาราง `promotion_claim`

ใช้เก็บคำขอรับสิทธิ์โปรโมชันที่ต้องแนบหลักฐาน เช่น กิจกรรมหรือโปรโมชันพิเศษ

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `ClaimId` | `int` | PK | No | Auto Increment | รหัสคำขอเคลม |
| `PromotionId` | `int` | FK | No | - | รหัสโปรโมชันที่ต้องการเคลม |
| `UserId` | `int` | FK | No | - | รหัสผู้ใช้ที่ส่งคำขอ |
| `ProofImagePath` | `varchar(255)` | - | No | - | ที่เก็บไฟล์หลักฐาน |
| `Note` | `text` | - | Yes | `NULL` | หมายเหตุจากผู้ใช้ |
| `Status` | `enum('Pending','Approved','Rejected')` | - | No | `Pending` | สถานะการพิจารณา |
| `RequestedAt` | `datetime` | - | No | `CURRENT_TIMESTAMP` | วันที่ส่งคำขอ |
| `ReviewedAt` | `datetime` | - | Yes | `NULL` | วันที่ตรวจสอบคำขอ |
| `ReviewedByUserId` | `int` | - | Yes | `NULL` | ผู้ตรวจสอบคำขอ |
| `ReviewNote` | `text` | - | Yes | `NULL` | หมายเหตุจากผู้ตรวจสอบ |

Foreign Key:

- `PromotionId` อ้างอิง `promotion.PromotionId`
- `UserId` อ้างอิง `user.UserId`

### 4.9 ตาราง `user_promotion`

ใช้เก็บข้อมูลว่าโปรโมชันใดถูกมอบให้กับผู้ใช้คนใด และมีการใช้งานไปแล้วหรือไม่

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `UserId` | `int` | PK, FK | No | - | รหัสผู้ใช้ |
| `PromotionId` | `int` | PK, FK | No | - | รหัสโปรโมชัน |
| `IsUsed` | `tinyint(1)` หรือ `int` | - | No | `0` | สถานะการใช้สิทธิ์ |
| `UsedAt` | `datetime` | - | Yes | `NULL` | วันที่ใช้งานโปรโมชัน |

Foreign Key:

- `UserId` อ้างอิง `user.UserId`
- `PromotionId` อ้างอิง `promotion.PromotionId`

หมายเหตุ:

- ใน EF Core model ปัจจุบัน ตารางนี้ถูกใช้งานแบบคีย์ผสม (`UserId`, `PromotionId`)
- ในไฟล์ `bakerydb.sql` เดิม ตารางเดียวกันยังมีคอลัมน์ `Id` และ `AssignedAt`
- สำหรับการทำเอกสารโครงการ ควรยึดโครงสร้างที่ระบบใช้งานจริงล่าสุด และระบุหมายเหตุความต่างนี้ไว้

### 4.10 ตาราง `order`

ใช้เก็บข้อมูลคำสั่งซื้อหลักของลูกค้า

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `OrderId` | `int` | PK | No | Auto Increment | รหัสคำสั่งซื้อ |
| `UserId` | `int` | FK | Yes | `NULL` | ผู้สั่งซื้อ |
| `AddressId` | `int` | FK | Yes | `NULL` | ที่อยู่จัดส่ง |
| `OrderDate` | `datetime` | - | Yes | `CURRENT_TIMESTAMP` | วันที่สั่งซื้อ |
| `TotalAmount` | `decimal(10,2)` | - | Yes | `NULL` | ยอดรวมคำสั่งซื้อ |
| `Status` | `enum('Pending','Paid','Preparing','Shipped','Completed','Cancelled')` | - | Yes | `NULL` | สถานะคำสั่งซื้อ |
| `PaymentStatus` | `enum('Pending','Paid','PendingVerify','Refunded')` หรือ `varchar(50)` | - | Yes | `NULL` | สถานะการชำระเงิน |
| `PromotionId` | `int` | FK | Yes | `NULL` | โปรโมชันที่ใช้กับคำสั่งซื้อ |
| `SlipImagePath` | `varchar(255)` หรือ `text` | - | Yes | `NULL` | path รูปสลิปการชำระเงิน |

Foreign Key:

- `UserId` อ้างอิง `user.UserId`
- `AddressId` อ้างอิง `address.AddressId`
- `PromotionId` อ้างอิง `promotion.PromotionId`

ดัชนีที่สำคัญ:

- `idx_order_status`
- `idx_order_user`
- `idx_order_date`

### 4.11 ตาราง `orderdetail`

ใช้เก็บรายการสินค้าในแต่ละคำสั่งซื้อ โดย 1 คำสั่งซื้อสามารถมีได้หลายรายการสินค้า

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `OrderDetailId` | `int` | PK | No | Auto Increment | รหัสรายการสั่งซื้อย่อย |
| `OrderId` | `int` | FK | Yes | `NULL` | รหัสคำสั่งซื้อหลัก |
| `ProductId` | `int` | FK | Yes | `NULL` | รหัสสินค้า |
| `Quantity` | `int` | - | Yes | `NULL` | จำนวนสินค้าที่สั่ง |
| `UnitPrice` | `decimal(10,2)` | - | Yes | `NULL` | ราคาต่อหน่วย |

Foreign Key:

- `OrderId` อ้างอิง `order.OrderId`
- `ProductId` อ้างอิง `stock.ProductId`

หมายเหตุ:

- หาก `UnitPrice = 0` ระบบใช้ตีความว่าเป็นสินค้าของแถมจากโปรโมชัน

### 4.12 ตาราง `historyorder`

ใช้เก็บประวัติคำสั่งซื้อที่เสร็จสมบูรณ์แล้ว เพื่อแยกออกจากรายการที่กำลังดำเนินการ

| ชื่อฟิลด์ | ชนิดข้อมูล | คีย์ | Null | ค่าเริ่มต้น | คำอธิบาย |
|---|---|---|---|---|---|
| `HistoryOrderId` | `int` | PK | No | Auto Increment | รหัสประวัติคำสั่งซื้อ |
| `OrderId` | `int` | FK เชิงตรรกะ | Yes | `NULL` | อ้างอิงคำสั่งซื้อเดิม |
| `UserId` | `int` | FK | Yes | `NULL` | รหัสผู้ใช้ |
| `OrderDate` | `datetime` | - | Yes | `NULL` | วันที่สร้างคำสั่งซื้อ |
| `CompletedAt` | `datetime` | - | Yes | `NULL` | วันที่คำสั่งซื้อเสร็จสมบูรณ์ |
| `TotalAmount` | `decimal(10,2)` | - | Yes | `NULL` | ยอดรวมที่ชำระ |
| `Status` | `varchar(50)` | - | Yes | `NULL` | สถานะสุดท้ายของคำสั่งซื้อ |
| `PaymentStatus` | `varchar(50)` | - | Yes | `NULL` | สถานะการชำระเงิน |
| `DeliveryAddress` | `varchar(500)` | - | Yes | `NULL` | ที่อยู่จัดส่งแบบสรุป |
| `ItemSummary` | `text` | - | Yes | `NULL` | สรุปรายการสินค้า |

Foreign Key:

- `UserId` อ้างอิง `user.UserId`

หมายเหตุ:

- ในสคีมาปัจจุบัน `OrderId` ใช้เชื่อมโยงเชิงตรรกะกับคำสั่งซื้อเดิม แต่ไม่ได้บังคับ Foreign Key ในระดับฐานข้อมูล

## 5. กฎธุรกิจที่สำคัญจากการออกแบบข้อมูล

### 5.1 การจัดการสิทธิ์ผู้ใช้

- ผู้ใช้ทุกคนต้องมีบทบาท (`RoleId`) เพื่อกำหนดสิทธิ์การใช้งาน
- ระบบรองรับอย่างน้อย 3 บทบาท คือ `Admin`, `Staff`, `User`

### 5.2 การจัดการคำสั่งซื้อ

- ผู้ใช้ 1 คนสามารถมีคำสั่งซื้อหลายรายการ
- คำสั่งซื้อ 1 รายการสามารถมีสินค้าได้หลายรายการผ่าน `orderdetail`
- คำสั่งซื้อสามารถผูกกับที่อยู่จัดส่งและโปรโมชันได้

### 5.3 การจัดการสถานะคำสั่งซื้อ

สถานะคำสั่งซื้อ (`Status`) ที่พบในระบบ:

- `Pending`
- `Paid`
- `Preparing`
- `Shipped`
- `Completed`
- `Cancelled`

สถานะการชำระเงิน (`PaymentStatus`) ที่พบในระบบ:

- `Pending`
- `PendingVerify`
- `Paid`
- `Refunded`

### 5.4 การจัดการโปรโมชัน

- โปรโมชัน 1 รายการสามารถถูกแจกให้ผู้ใช้หลายคนผ่าน `user_promotion`
- โปรโมชันบางประเภทมีของแถม ซึ่งเก็บได้ทั้งแบบของแถมหลัก (`RewardProductId`) และแบบหลายรายการ (`promotion_reward_item`)
- โปรโมชันบางประเภทต้องแนบหลักฐานก่อนรับสิทธิ์ โดยเก็บคำขอไว้ใน `promotion_claim`

### 5.5 การเก็บประวัติ

- เมื่อคำสั่งซื้อเสร็จสมบูรณ์ ระบบจะคัดลอกข้อมูลสำคัญบางส่วนไปเก็บใน `historyorder`
- แนวทางนี้ช่วยแยกข้อมูลคำสั่งซื้อที่จบแล้วออกจากข้อมูลคำสั่งซื้อที่ยังติดตามสถานะอยู่

## 6. หมายเหตุเชิงเอกสารและข้อเสนอแนะ

### 6.1 จุดที่ควรระบุในรายงาน

สำหรับการทำเอกสารหรือรายงานโครงการ ควรอธิบายเพิ่มว่า

- ฐานข้อมูลรองรับทั้งงานขายและงานบริหารร้านเบเกอรี่
- มีการออกแบบให้รองรับโปรโมชันหลายรูปแบบ ไม่ได้จำกัดแค่ส่วนลดทั่วไป
- ใช้การแยก `order` และ `historyorder` เพื่อให้การค้นหางานปัจจุบันกับประวัติย้อนหลังทำได้ง่าย

### 6.2 จุดที่ควรปรับปรุงหากพัฒนาต่อ

- ควรทำ schema ให้ตรงกันทุกแหล่งระหว่าง `bakerydb.sql`, migration และ EF model
- ควรเพิ่ม Unique Constraint ให้ `Username` และอาจรวมถึง `Email`
- ควรจัดเก็บรหัสผ่านแบบเข้ารหัส hash แทน plain text
- ควรระบุความหมายของ `PromoType` เป็น enum หรือมีตารางอ้างอิงเพื่อให้อ่านง่ายขึ้น
- ควรพิจารณาเพิ่ม Foreign Key ของ `ReviewedByUserId` ไปยัง `user.UserId`

## 7. สรุป

การออกแบบฐานข้อมูลของระบบนี้ครอบคลุมกระบวนการหลักของร้านเบเกอรี่ตั้งแต่ผู้ใช้ สินค้า คำสั่งซื้อ การชำระเงิน การจัดส่ง และโปรโมชัน โดยมีโครงสร้างที่เหมาะกับการพัฒนาเว็บแอปพลิเคชันจริง และสามารถนำไปใช้ประกอบเอกสารโครงการหรือรายงานวิเคราะห์ระบบได้อย่างชัดเจน
