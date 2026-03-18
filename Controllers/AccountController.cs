using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace _66022380.Controllers;

public class AccountController : Controller
{
    private readonly BakerydbContext _db;
    public AccountController(BakerydbContext db)
    {
        _db = db;
    }

    // ─── หน้า User ───────────────────────────────────────
    public IActionResult Home() => View();
    public IActionResult Signup() => View();

    public IActionResult Profile(int id = 0)
    {
        if (id <= 0)
        {
            id = int.TryParse(HttpContext.Session.GetString("UserId"), out var userId) ? userId : 0;
        }

        if (id <= 0)
            return RedirectToAction("Login");

        var user = _db.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null)
            return RedirectToAction("Login");

        var addresses = _db.Addresses.Where(a => a.UserId == id).ToList();

        var profileModel = new ProfileViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email ?? "",
            Phone = user.Phone ?? "",
            Addresses = addresses
        };

        return View(profileModel);
    }

    [HttpPost]
    public IActionResult Profile([FromBody] ProfileViewModel model)
    
    {
        if (model.UserId <= 0)
            return Json(new { success = false, message = "ข้อมูลผู้ใช้ไม่ถูกต้อง" });

        var user = _db.Users.Find(model.UserId);
        if (user == null)
            return Json(new { success = false, message = "ไม่พบข้อมูลผู้ใช้" });

        // Update profile info only if they are provided (not empty)
        if (!string.IsNullOrWhiteSpace(model.Username))
            user.Username = model.Username;
        if (!string.IsNullOrWhiteSpace(model.Email))
            user.Email = model.Email;
        if (!string.IsNullOrWhiteSpace(model.Phone))
            user.Phone = model.Phone;

        // Check if user wants to change password
        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            // Validate current password was provided
            if (string.IsNullOrEmpty(model.CurrentPassword))
                return Json(new { success = false, message = "กรุณากรอกรหัสผ่านปัจจุบัน" });

            // Compare passwords with robust handling for whitespace and encoding
            string storedPassword = (user.Password ?? "").Trim();
            string enteredPassword = (model.CurrentPassword ?? "").Trim();
            
            // Use case-sensitive comparison with ordinal rules
            bool passwordsMatch = string.Equals(storedPassword, enteredPassword, StringComparison.Ordinal);
            
            if (!passwordsMatch)
                return Json(new { success = false, message = "รหัสผ่านปัจจุบันไม่ถูกต้อง" });

            // Validate new password matches confirmation
            if (model.NewPassword != model.ConfirmPassword)
                return Json(new { success = false, message = "รหัสผ่านใหม่ไม่ตรงกัน" });

            // Validate password length
            if (model.NewPassword.Length < 6)
                return Json(new { success = false, message = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร" });

            // Update password
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

    public IActionResult Menu()
    {
        var products = _db.Stocks
            .Include(s => s.Category)
            .ToList();
        return View(products);
    }
    public IActionResult lab8() => View();
    public IActionResult Delivery() => View();

    // ─── หน้า Admin ──────────────────────────────────────
    public IActionResult Dashbordadmin() => View("~/Views/admin/Dashbordadmin.cshtml");
    public IActionResult Stock() => View("~/Views/admin/Stock.cshtml");
    public IActionResult Order() => View("~/Views/admin/Order.cshtml");

    // ─── Auth ─────────────────────────────────────────────
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var user = _db.Users.FirstOrDefault(u =>
            (u.Username == username || u.Email == username) &&
            u.Password == password
        );

        if (user == null)
        {
            ViewBag.Error = "Invalid username or password";
            return View();
        }

        HttpContext.Session.SetString("UserId", user.UserId.ToString());
        HttpContext.Session.SetString("Username", user.Username);

        if (user.RoleId == 1)
            return RedirectToAction("Dashbordadmin");

        return RedirectToAction("Home");
    }

    public IActionResult Lab10(int id = 0)
    {
        var check = (from us in _db.Users
                     where us.UserId == id
                     select new UserViewModel
                     {
                         UserId = us.UserId,
                         Username = us.Username,
                         Email = us.Email,
                         Phone = us.Phone
                     }).FirstOrDefault();
        return View(check);
    }

    [HttpPost]
    public IActionResult Lab10(UserViewModel model)
    {
        if (model.UserId > 0)
        {

            var user = _db.Users.Find(model.UserId);
            if (user != null)
            {
                user.Username = model.Username;
                user.Email = model.Email;
                user.Phone = model.Phone;
                _db.SaveChanges();
            }
        }
        else
        {
            // Add new user
            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                Phone = model.Phone,
                Password = "default123",
                RoleId = 3
            };
            _db.Users.Add(newUser);
            _db.SaveChanges();
        }

        return RedirectToAction("Member");
    }

    public IActionResult Lab10D(String id)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserId == int.Parse(id));

        if (user != null)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
        }

        return RedirectToAction("Member", "Account");
    }



    [HttpPost]
    public IActionResult Signup(string username, string email, string phone, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match";
            return View();
        }

        var newUser = new User
        {
            Username = username,
            Email = email,
            Phone = phone,
            Password = password,
            RoleId = 3
        };

        _db.Users.Add(newUser);
        _db.SaveChanges();

        return RedirectToAction("Login");
    }

    // ─── Admin Data ───────────────────────────────────────
    public IActionResult Member()
    {
        var users = _db.Users.Include(u => u.Role).ToList();
        return View("~/Views/admin/Member.cshtml", users);
    }

    // ─── System ───────────────────────────────────────────
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}