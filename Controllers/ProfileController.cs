using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers;

public class ProfileController : Controller
{
    private readonly BakerydbContext _db;

    public ProfileController(BakerydbContext db)
    {
        _db = db;
    }

    public IActionResult Profile(int id = 0)
    {
        if (id <= 0)
        {
            id = int.TryParse(HttpContext.Session.GetString("UserId"), out var userId) ? userId : 0;
        }

        if (id <= 0)
            return RedirectToAction("Login", "Account");

        var user = _db.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var addresses = _db.Addresses.Where(a => a.UserId == id).ToList();

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

    [HttpPost]
    public IActionResult Profile([FromBody] ProfileViewModel model)
    {
        if (model.UserId <= 0)
            return Json(new { success = false, message = "ข้อมูลผู้ใช้ไม่ถูกต้อง" });

        var user = _db.Users.Find(model.UserId);
        if (user == null)
            return Json(new { success = false, message = "ไม่พบข้อมูลผู้ใช้" });

        if (!string.IsNullOrWhiteSpace(model.Username))
            user.Username = model.Username;
        if (!string.IsNullOrWhiteSpace(model.Email))
            user.Email = model.Email;
        if (!string.IsNullOrWhiteSpace(model.Phone))
            user.Phone = model.Phone;

        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            if (string.IsNullOrEmpty(model.CurrentPassword))
                return Json(new { success = false, message = "กรุณากรอกรหัสผ่านปัจจุบัน" });

            if (!string.Equals(user.Password, model.CurrentPassword, StringComparison.Ordinal))
                return Json(new { success = false, message = "รหัสผ่านปัจจุบันไม่ถูกต้อง" });

            if (model.NewPassword != model.ConfirmPassword)
                return Json(new { success = false, message = "รหัสผ่านใหม่ไม่ตรงกัน" });

            if (model.NewPassword.Length < 6)
                return Json(new { success = false, message = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร" });

            user.Password = model.NewPassword;
        }

        _db.SaveChanges();

        return Json(new { success = true, message = "บันทึกข้อมูลโปรไฟล์เรียบร้อยแล้ว!" });
    }

    [HttpPost]
    public IActionResult SaveAddress(int userId, int addressId, string addressLine, string district, string province, string postalCode)
    {
        if (addressId > 0)
        {
            var address = _db.Addresses.Find(addressId);
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

    [HttpPost]
    public IActionResult DeleteAddress(int addressId, int userId)
    {
        var address = _db.Addresses.Find(addressId);
        if (address != null && address.UserId == userId)
        {
            _db.Addresses.Remove(address);
            _db.SaveChanges();
            return Json(new { success = true, message = "ลบที่อยู่เรียบร้อยแล้ว!" });
        }

        return Json(new { success = false, message = "ไม่พบที่อยู่หรือคุณไม่มีสิทธิ์ลบ" });
    }

    [HttpGet]
    public IActionResult GetUserAddresses(int userId)
    {
        var addresses = _db.Addresses.Where(a => a.UserId == userId).ToList();
        return Json(new { success = true, addresses });
    }
}
