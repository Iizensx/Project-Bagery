using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers;

public class ProfileController : Controller
{
    private readonly BakerydbContext _db;

    // รับ BakerydbContext มาผ่าน Dependency Injection
    public ProfileController(BakerydbContext db)
    {
        _db = db;
    }

    // GET: ดึงข้อมูลโปรไฟล์ของผู้ใช้มาแสดงหน้า Profile
    public IActionResult Profile(int id = 0)
    {
        // ถ้าไม่ได้ส่ง id มา ให้ดึง UserId จาก Session แทน
        if (id <= 0)
        {
            id = int.TryParse(HttpContext.Session.GetString("UserId"), out var userId) ? userId : 0;
        }

        // ถ้ายังไม่มี id อยู่ดี แสดงว่ายังไม่ได้ login → redirect ไปหน้า Login
        if (id <= 0)
            return RedirectToAction("Login", "Account");

        // ค้นหา user จาก database ด้วย id
        var user = _db.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null)
            return RedirectToAction("Login", "Account");

        // ดึงรายการที่อยู่ทั้งหมดของ user คนนี้
        var addresses = _db.Addresses.Where(a => a.UserId == id).ToList();

        // สร้าง ViewModel เพื่อส่งข้อมูลไปแสดงที่ View
        var profileModel = new ProfileViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email ?? "",
            Phone = user.Phone ?? "",
            Addresses = addresses
        };

        return View("~/Views/Account/Profile.cshtml", profileModel);
    }

    // POST: รับข้อมูลจาก JSON body แล้วอัปเดตโปรไฟล์ผู้ใช้
    [HttpPost]
    public IActionResult Profile([FromBody] ProfileViewModel model)
    {
        // ตรวจสอบว่า UserId ถูกต้องไหม
        if (model.UserId <= 0)
            return Json(new { success = false, message = "ข้อมูลผู้ใช้ไม่ถูกต้อง" });

        // หา user จาก database
        var user = _db.Users.Find(model.UserId);
        if (user == null)
            return Json(new { success = false, message = "ไม่พบข้อมูลผู้ใช้" });

        // อัปเดตข้อมูลเฉพาะฟิลด์ที่กรอกมา (ไม่ทับข้อมูลเดิมถ้าว่าง)
        if (!string.IsNullOrWhiteSpace(model.Username))
            user.Username = model.Username;
        if (!string.IsNullOrWhiteSpace(model.Email))
            user.Email = model.Email;
        if (!string.IsNullOrWhiteSpace(model.Phone))
            user.Phone = model.Phone;

        // ถ้ามีการกรอก NewPassword มา ให้เปลี่ยนรหัสผ่าน
        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            // ต้องกรอก CurrentPassword ก่อนเสมอ
            if (string.IsNullOrEmpty(model.CurrentPassword))
                return Json(new { success = false, message = "กรุณากรอกรหัสผ่านปัจจุบัน" });

            // เปรียบเทียบรหัสผ่านปัจจุบันกับที่อยู่ใน database (Trim เผื่อมี space)
            string storedPassword = (user.Password ?? "").Trim();
            string enteredPassword = (model.CurrentPassword ?? "").Trim();
            bool passwordsMatch = string.Equals(storedPassword, enteredPassword, StringComparison.Ordinal);

            if (!passwordsMatch)
                return Json(new { success = false, message = "รหัสผ่านปัจจุบันไม่ถูกต้อง" });

            // ตรวจสอบว่า NewPassword กับ ConfirmPassword ตรงกันไหม
            if (model.NewPassword != model.ConfirmPassword)
                return Json(new { success = false, message = "รหัสผ่านใหม่ไม่ตรงกัน" });

            // รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร
            if (model.NewPassword.Length < 6)
                return Json(new { success = false, message = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร" });

            user.Password = model.NewPassword;
        }

        // บันทึกการเปลี่ยนแปลงลง database
        _db.SaveChanges();

        return Json(new { success = true, message = "บันทึกข้อมูลโปรไฟล์เรียบร้อยแล้ว!" });
    }

    // POST: เพิ่มหรือแก้ไขที่อยู่ของผู้ใช้
    [HttpPost]
    public IActionResult SaveAddress(int userId, int addressId, string addressLine, string district, string province, string postalCode)
    {
        // ถ้า addressId > 0 แสดงว่าเป็นการแก้ไขที่อยู่เดิม
        if (addressId > 0)
        {
            var address = _db.Addresses.Find(addressId);
            // ตรวจสอบว่าที่อยู่นี้เป็นของ user คนนี้จริงๆ ก่อนแก้ไข
            if (address != null && address.UserId == userId)
            {
                address.AddressLine = addressLine;
                address.District = district;
                address.Province = province;
                address.PostalCode = postalCode;
                _db.SaveChanges();
            }
        }
        else
        {
            // ถ้า addressId = 0 แสดงว่าเป็นการเพิ่มที่อยู่ใหม่
            var newAddress = new Address
            {
                UserId = userId,
                AddressLine = addressLine,
                District = district,
                Province = province,
                PostalCode = postalCode
            };
            _db.Addresses.Add(newAddress);
            _db.SaveChanges();
        }

        return Json(new { success = true, message = "บันทึกที่อยู่เรียบร้อยแล้ว!" });
    }

    // POST: ลบที่อยู่ของผู้ใช้ตาม addressId
    [HttpPost]
    public IActionResult DeleteAddress(int addressId, int userId)
    {
        var address = _db.Addresses.Find(addressId);
        // ตรวจสอบว่าที่อยู่นี้มีอยู่จริงและเป็นของ user คนนี้ก่อนลบ
        if (address != null && address.UserId == userId)
        {
            _db.Addresses.Remove(address);
            _db.SaveChanges();
            return Json(new { success = true, message = "ลบที่อยู่เรียบร้อยแล้ว!" });
        }

        return Json(new { success = false, message = "ไม่พบที่อยู่หรือคุณไม่มีสิทธิ์ลบ" });
    }

    // GET: ดึงรายการที่อยู่ทั้งหมดของ user คืนมาในรูปแบบ JSON
    [HttpGet]
    public IActionResult GetUserAddresses(int userId)
    {
        var addresses = _db.Addresses.Where(a => a.UserId == userId).ToList();
        return Json(new { success = true, addresses });
    }
}