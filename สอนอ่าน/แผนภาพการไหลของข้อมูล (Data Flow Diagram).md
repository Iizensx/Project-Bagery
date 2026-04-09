# แผนภาพการไหลของข้อมูล (Data Flow Diagram)

ไฟล์นี้ใช้เป็นแนวทางทำ `Data Flow Diagram (DFD)` ของระบบร้านเบเกอรี่จากโค้ดจริงในโปรเจกต์นี้ โดยจัดให้สามารถนำไปใช้ได้ 2 แบบ

- ใช้ต่อใน `Mermaid`
- ใช้เป็นแบบร่างไปวาดต่อใน `Draw.io`

## ควรทำ DFD หัวข้อไหนบ้าง

หัวข้อที่คุณคิดไว้ถือว่า `ใช้ได้` และเหมาะกับงานนำเสนอภาพรวมของระบบแล้ว

1. `Login`
2. `Register / Signup`
3. `หน้าลูกค้า (Customer / User)`
4. `หน้าพนักงาน (Staff)`
5. `หน้าผู้ดูแลระบบ (Admin)`

## แนะนำให้เพิ่มอีกเล็กน้อย

ถ้าอาจารย์อยากเห็นภาพระบบชัดขึ้นกว่าเดิม แนะนำเพิ่มอีก 2 หัวข้อ

6. `Forgot Password + OTP`
7. `Checkout / Payment / Delivery`

เหตุผลคือ 2 ส่วนนี้เป็น flow หลักของระบบ และมีการรับส่งข้อมูลหลายจุด เช่น

- ผู้ใช้
- ระบบ
- ฐานข้อมูล
- session
- ไฟล์สลิป
- ประวัติคำสั่งซื้อ

ถ้าเวลาน้อยมาก ให้ใช้ 5 หัวข้อแรกได้เลย  
แต่ถ้าต้องการให้ดู “ครบระบบ” มากขึ้น แนะนำใช้ 7 หัวข้อ

---

## หลักการวาด DFD ของระบบนี้

ในการวาด DFD ของโปรเจกต์นี้ ให้คิดเป็น 4 ส่วน

- `External Entity`
  เช่น ลูกค้า, Staff, Admin
- `Process`
  เช่น Login, Signup, Create Order, Confirm Payment
- `Data Store`
  เช่น `Users`, `Orders`, `Addresses`, `Promotions`
- `Data Flow`
  คือข้อมูลที่วิ่งระหว่างกัน เช่น ข้อมูล login, ข้อมูลสมัครสมาชิก, รายการสั่งซื้อ, ข้อมูลสลิป

## Data Store หลักของระบบนี้

จากโครงสร้างระบบจริงของคุณ สามารถใช้อ้างอิงใน DFD ได้ประมาณนี้

- `D1 Users`
- `D2 Roles`
- `D3 Addresses`
- `D4 Promotions`
- `D5 UserPromotions`
- `D6 Orders`
- `D7 Orderdetails`
- `D8 Historyorders`
- `D9 Stocks`
- `D10 Categories`
- `D11 PromotionClaims`
- `D12 Notifications`
- `D13 Uploaded Files`
  เช่น `payment slips`, `promotion claim images`

---

# 1. DFD ภาพรวมระบบ

อันนี้เหมาะใช้เป็น `Context Diagram` หรือ `DFD Level 0`

```mermaid
flowchart LR
    C[Customer]
    S[Staff]
    A[Admin]

    SYS((Bakery Management System))

    D1[(Users)]
    D2[(Orders)]
    D3[(Stocks)]
    D4[(Promotions)]
    D5[(Addresses)]
    D6[(Historyorders)]

    C -->|Login / Signup / Order / Payment / Profile| SYS
    SYS -->|Order Status / Promotion / Profile Data| C

    S -->|Manage Orders / Confirm Payment / Ship Order| SYS
    SYS -->|Order Information / Payment Slip| S

    A -->|Manage Dashboard / Members / Stock / Promotion| SYS
    SYS -->|Summary / Reports / Account Data| A

    SYS --- D1
    SYS --- D2
    SYS --- D3
    SYS --- D4
    SYS --- D5
    SYS --- D6
```

---

# 2. DFD ระบบ Login

## คำอธิบาย

ลูกค้ากรอก `Username` หรือ `Email` พร้อม `Password` ผ่านหน้า Login  
ระบบจะส่งข้อมูลไปยัง `AccountController` เพื่อตรวจสอบกับตาราง `Users`  
ถ้าข้อมูลถูกต้อง ระบบจะสร้าง session และส่งผู้ใช้ไปยังหน้าที่เหมาะสมตามบทบาท

```mermaid
flowchart LR
    U[User]
    P1((1.1 Login))
    D1[(Users)]
    D2[(Session)]
    D3[(Login Log)]

    U -->|Username / Email / Password| P1
    P1 -->|Read User Data| D1
    D1 -->|User Account| P1
    P1 -->|Create Session| D2
    P1 -->|Write Login Result| D3
    P1 -->|Login Success / Error Message / Redirect| U
```

## ถ้าจะวาดใน Draw.io

ให้วางดังนี้

- ซ้าย: `User`
- กลาง: `1.1 Login`
- ขวา: `Users`, `Session`, `Login Log`
- ลูกศรจาก User ไป Process เป็น `Username / Email / Password`
- ลูกศรกลับจาก Process ไป User เป็น `ผลการเข้าสู่ระบบ`

---

# 3. DFD ระบบ Register / Signup

## คำอธิบาย

ผู้ใช้กรอกข้อมูลสมัครสมาชิก เช่น `Username`, `Email`, `Phone`, `Password`, `ConfirmPassword`  
ระบบจะตรวจสอบข้อมูลซ้ำใน `Users`  
ถ้าผ่านเงื่อนไข ระบบจะบันทึกผู้ใช้ใหม่ และมอบโปรโมชั่นต้อนรับผ่าน `UserPromotions`

```mermaid
flowchart LR
    U[User]
    P2((1.1 Signup))
    D1[(Users)]
    D2[(Promotions)]
    D3[(UserPromotions)]

    U -->|Signup Form Data| P2
    P2 -->|Check Duplicate Username / Email| D1
    D1 -->|Existing User Data| P2
    P2 -->|Insert New User| D1
    P2 -->|Read Welcome Promotion| D2
    D2 -->|Promotion Data| P2
    P2 -->|Grant Welcome Promotion| D3
    P2 -->|Signup Result / Redirect to Login| U
```

---

# 4. DFD ระบบ Forgot Password + OTP

## คำอธิบาย

ผู้ใช้กรอกอีเมลเพื่อขอ OTP  
ระบบตรวจสอบอีเมลจาก `Users` แล้วสร้าง `OtpCode` และ `OtpExpiredAt`  
เมื่อผู้ใช้กรอก OTP และรหัสผ่านใหม่ ระบบจะตรวจสอบความถูกต้องก่อนอัปเดตรหัสผ่าน

```mermaid
flowchart LR
    U[User]
    P3((1.1 Request OTP))
    P4((1.2 Verify OTP))
    P5((1.3 Reset Password))
    D1[(Users)]
    D2[(Session)]

    U -->|Email| P3
    P3 -->|Find User by Email| D1
    D1 -->|User Data| P3
    P3 -->|Save OTP / Expire Time| D1
    P3 -->|OTP Result| U

    U -->|Email + OTP| P4
    P4 -->|Read OTP Data| D1
    D1 -->|OtpCode / OtpExpiredAt| P4
    P4 -->|Verified Session| D2
    P4 -->|Verify Result| U

    U -->|Email + OTP + New Password| P5
    P5 -->|Read Verified Session| D2
    P5 -->|Update Password| D1
    P5 -->|Reset Result / Redirect to Login| U
```

---

# 5. DFD หน้าลูกค้า (Customer / User)

## คำอธิบาย

ส่วนของลูกค้าครอบคลุมหลายหน้าหลัก เช่น

- Home
- Menu
- Promotion
- Profile
- Checkout
- Payment
- Delivery

ในภาพรวม ลูกค้าจะดูสินค้า ดูโปรโมชัน แก้ไขโปรไฟล์ สั่งซื้อ และติดตามสถานะคำสั่งซื้อ

```mermaid
flowchart LR
    C[Customer]
    P1((1.1 View Menu / Home))
    P2((1.2 View Promotion))
    P3((1.3 Manage Profile))
    P4((1.4 Create Order))
    P5((1.5 Upload Slip))
    P6((1.6 Track Delivery))

    D1[(Stocks)]
    D2[(Categories)]
    D3[(Promotions)]
    D4[(Users)]
    D5[(Addresses)]
    D6[(Orders)]
    D7[(Orderdetails)]
    D8[(Historyorders)]
    D9[(UserPromotions)]
    D10[(PromotionClaims)]
    D11[(Uploaded Files)]

    C --> P1
    P1 --> D1
    P1 --> D2
    P1 -->|Product List| C

    C --> P2
    P2 --> D3
    P2 --> D9
    P2 --> D10
    P2 -->|Promotion Info| C

    C --> P3
    P3 --> D4
    P3 --> D5
    P3 -->|Profile / Address Result| C

    C --> P4
    P4 --> D5
    P4 --> D3
    P4 --> D6
    P4 --> D7
    P4 --> D9
    P4 -->|Order Created| C

    C --> P5
    P5 --> D11
    P5 --> D6
    P5 -->|Upload Result| C

    C --> P6
    P6 --> D6
    P6 --> D8
    P6 -->|Tracking / History / Complete Result| C
```

---

# 6. DFD หน้าพนักงาน (Staff)

## คำอธิบาย

ในระบบนี้ Staff เกี่ยวข้องกับการจัดการคำสั่งซื้อเป็นหลัก เช่น

- ดูรายการออเดอร์
- ยืนยันการชำระเงิน
- รับออเดอร์
- เปลี่ยนสถานะเป็นจัดส่ง

```mermaid
flowchart LR
    S[Staff]
    P1((1.1 View Orders))
    P2((1.2 Confirm Payment))
    P3((1.3 Accept Order))
    P4((1.4 Ship Order))

    D1[(Orders)]
    D2[(Orderdetails)]
    D3[(Stocks)]
    D4[(Notifications)]

    S --> P1
    P1 --> D1
    P1 --> D2
    P1 -->|Order List| S

    S --> P2
    P2 --> D1
    P2 --> D2
    P2 --> D3
    P2 --> D4
    P2 -->|Payment Confirmation Result| S

    S --> P3
    P3 --> D1
    P3 --> D4
    P3 -->|Accept Result| S

    S --> P4
    P4 --> D1
    P4 --> D4
    P4 -->|Shipping Result| S
```

---

# 7. DFD หน้าผู้ดูแลระบบ (Admin)

## คำอธิบาย

Admin มีสิทธิ์ดูภาพรวมและจัดการข้อมูลหลักของระบบ เช่น

- Dashboard
- สมาชิก
- สินค้าและหมวดหมู่
- โปรโมชัน
- การอนุมัติ promotion claim

```mermaid
flowchart LR
    A[Admin]
    P1((1.1 View Dashboard))
    P2((1.2 Manage Members))
    P3((1.3 Manage Stock / Category))
    P4((1.4 Manage Promotion))
    P5((1.5 Review Promotion Claims))

    D1[(Users)]
    D2[(Orders)]
    D3[(Stocks)]
    D4[(Categories)]
    D5[(Promotions)]
    D6[(PromotionClaims)]
    D7[(UserPromotions)]

    A --> P1
    P1 --> D1
    P1 --> D2
    P1 --> D3
    P1 --> D5
    P1 -->|Dashboard Summary| A

    A --> P2
    P2 --> D1
    P2 -->|Member Result| A

    A --> P3
    P3 --> D3
    P3 --> D4
    P3 -->|Stock / Category Result| A

    A --> P4
    P4 --> D5
    P4 --> D7
    P4 -->|Promotion Result| A

    A --> P5
    P5 --> D6
    P5 --> D7
    P5 -->|Claim Review Result| A
```

---

# 8. DFD ระบบ Menu / Promotion

## คำอธิบาย

หัวข้อนี้แนะนำให้แยกเพิ่ม ถ้าต้องการให้ฝั่งลูกค้าดูชัดขึ้น เพราะเป็นหน้าที่ผู้ใช้เห็นบ่อยมาก

```mermaid
flowchart LR
    C[Customer]
    P1((1.1 View Menu))
    P2((1.2 View Promotion))
    P3((1.3 Submit Promotion Claim))

    D1[(Stocks)]
    D2[(Categories)]
    D3[(Promotions)]
    D4[(UserPromotions)]
    D5[(PromotionClaims)]
    D6[(Uploaded Files)]

    C --> P1
    P1 --> D1
    P1 --> D2
    P1 -->|Menu Data| C

    C --> P2
    P2 --> D3
    P2 --> D4
    P2 -->|Promotion Data| C

    C --> P3
    P3 --> D3
    P3 --> D5
    P3 --> D6
    P3 -->|Claim Result| C
```

---

# 9. DFD ระบบสั่งซื้อ ชำระเงิน และติดตามสถานะ

## แนวที่แนะนำสำหรับส่งงาน

ถ้าต้องการให้รูปออกมาดูเป็น DFD มาตรฐานแบบงานตัวอย่าง แนะนำให้แยก flow นี้เป็น 4 process หลักตามโค้ดจริงของระบบ

- `1.1 Create Order`
- `1.2 Payment`
- `1.3 Upload Slip`
- `1.4 Track Delivery / Complete Order`

กระบวนการเหล่านี้อ้างอิงจาก action จริงในระบบ ได้แก่ `CreateOrder`, `Payment`, `UploadSlip`, `Delivery` และ `CompleteOrder`

## Mermaid เวอร์ชันมาตรฐาน

```mermaid
flowchart LR
    C[Customer]

    P1([1.1 Create Order])
    P2([1.2 Payment])
    P3([1.3 Upload Slip])
    P4([1.4 Track Delivery / Complete Order])

    D1[(D1 Addresses)]
    D2[(D2 Orders)]
    D3[(D3 Orderdetails)]
    D4[(D4 Promotions)]
    D5[(D5 UserPromotions)]
    D6[(D6 Slip Files)]
    D7[(D7 Historyorders)]

    C -->|Order data / Address / Promotion| P1
    P1 -->|Read address| D1
    P1 -->|Read promotion| D4
    P1 -->|Update used promotion| D5
    P1 -->|Create order| D2
    P1 -->|Create order items| D3
    P1 -->|Order result / OrderId| C

    C -->|Open payment page| P2
    P2 -->|Read order amount| D2
    P2 -->|QR Code / Payment info| C

    C -->|Slip image| P3
    P3 -->|Save slip file| D6
    P3 -->|Update payment status| D2
    P3 -->|Upload result| C

    C -->|Track request / Complete request| P4
    P4 -->|Read order status| D2
    P4 -->|Read order history| D7
    P4 -->|Update completed order| D2
    P4 -->|Save completed history| D7
    P4 -->|Tracking status / Complete result| C
```

## คำอธิบายแบบภาษาคน

- ลูกค้าส่งข้อมูลสินค้า ที่อยู่ และโปรโมชันเข้าสู่ process `1.1 Create Order`
- ระบบอ่านข้อมูลที่อยู่จาก `Addresses` และตรวจสอบโปรโมชันจาก `Promotions` กับ `UserPromotions`
- เมื่อข้อมูลถูกต้อง ระบบจะสร้างรายการใน `Orders` และ `Orderdetails` แล้วส่ง `OrderId` กลับให้ลูกค้า
- จากนั้นลูกค้าจะเข้าสู่ process `1.2 Payment` เพื่อดูยอดชำระและ QR Code ของคำสั่งซื้อ
- เมื่อลูกค้าโอนเงินแล้ว จะเข้าสู่ process `1.3 Upload Slip` เพื่อแนบรูปสลิป โดยระบบจะเก็บไฟล์ไว้ใน `Slip Files` และอัปเดตสถานะการชำระเงินใน `Orders`
- ในขั้นตอนสุดท้าย ลูกค้าสามารถเข้าสู่ process `1.4 Track Delivery / Complete Order` เพื่อดูสถานะการจัดส่งจาก `Orders` และประวัติจาก `Historyorders`
- ถ้าลูกค้าได้รับสินค้าแล้ว ระบบจะอัปเดตคำสั่งซื้อเป็นสถานะเสร็จสมบูรณ์ และบันทึกข้อมูลลง `Historyorders`

## ถ้าจะวาดใน Draw.io ให้ดูเหมือน DFD มาตรฐาน

ให้จัด layout แบบนี้

- ซ้ายสุด: `Customer`
- แถวกลาง: `1.1 Create Order` -> `1.2 Payment` -> `1.3 Upload Slip` -> `1.4 Track Delivery / Complete Order`
- ด้านขวา: `D1 Addresses`, `D2 Orders`, `D3 Orderdetails`, `D4 Promotions`, `D5 UserPromotions`, `D6 Slip Files`, `D7 Historyorders`

### หลักการจัดวาง

1. วาง `Customer` ไว้ทางซ้ายเพียงกล่องเดียว
2. วาง process เป็นกล่องสี่เหลี่ยมมุมมนเรียงแนวนอน 4 กล่อง
3. วาง data store เรียงแนวตั้งทางขวา
4. ให้เส้นข้อมูลวิ่งจากซ้ายไปขวาเป็นหลัก เพื่อลดเส้นไขว้
5. ใช้ชื่อบนลูกศรสั้น ๆ เช่น `Order data`, `Read order`, `Save slip`, `Tracking status`

## ถ้าต้องการเวอร์ชันย่อมาก

ถ้าอาจารย์ต้องการรูปที่ง่ายและไม่แน่นเกินไป สามารถย่อเหลือ 3 process ได้

- `1.1 Create Order`
- `1.2 Upload Slip`
- `1.3 Track Delivery`

และใช้ data store หลักเพียง

- `Orders`
- `Orderdetails`
- `Addresses`
- `Slip Files`
- `Historyorders`

---

# 10. ถ้าจะทำใน Draw.io ควรวาดอย่างไร

ถ้าใช้ Draw.io ให้ใช้รูปทรงตามนี้

- `External Entity`
  ใช้สี่เหลี่ยม
- `Process`
  ใช้วงกลมหรือสี่เหลี่ยมมุมมน
- `Data Store`
  ใช้สัญลักษณ์ฐานข้อมูลหรือสี่เหลี่ยมเปิดด้านข้าง
- `Data Flow`
  ใช้ลูกศรพร้อมข้อความกำกับ

## รูปแบบที่แนะนำ

สำหรับแต่ละหัวข้อ ให้จัด layout แบบซ้ายไปขวา

- ซ้าย: ผู้ใช้งาน เช่น Customer / Staff / Admin
- กลาง: Process หลักของระบบ
- ขวา: Data Store

ตัวอย่างการจัด

- ซ้าย `Customer`
- กลาง `Login Process`
- ขวา `Users`, `Roles`, `Session`

---

# 11. สรุปว่าควรทำหัวข้อไหนบ้าง

ถ้าจะทำแบบพอดีและส่งงานได้ครบ แนะนำชุดนี้

1. Login
2. Register / Signup
3. Forgot Password + OTP
4. Customer
5. Staff
6. Admin

ถ้าต้องการให้ละเอียดขึ้นอีก ให้เพิ่ม

7. Menu / Promotion
8. Checkout / Payment / Delivery

## สรุปสุดท้าย

ถ้าอาจารย์ต้องการ `ภาพรวมระบบ` ให้ใช้ DFD ตามบทบาทผู้ใช้

- Customer
- Staff
- Admin

ถ้าอาจารย์ต้องการ `เห็นกระบวนการหลักของระบบ`

- Login
- Signup
- Forgot Password
- Checkout / Payment / Delivery

ดังนั้นแนวที่ดีที่สุดคือใช้ทั้ง 2 มุมร่วมกัน คือ

- วาดภาพรวมระบบ 1 รูป
- วาดระบบหลักแยกอีก 4-7 รูป

แบบนี้จะดูครบ เข้าใจง่าย และตรงกับระบบจริงของโปรเจกต์คุณมากที่สุด
