using Microsoft.AspNetCore.Mvc;
using _66022380.Models.Db;

namespace _66022380.Controllers;

public class AccountController : Controller
{
    private readonly BakerydbContext _db;
    private readonly ILogger<AccountController> _logger;

    // รับ BakerydbContext และ Logger มาผ่าน Dependency Injection
    public AccountController(BakerydbContext db, ILogger<AccountController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: แสดงหน้า Login
    public IActionResult Login() => View();

    // POST: รับ username และ password แล้วตรวจสอบกับ database
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        // ดึง IP ของคนที่ login มาเพื่อบันทึก log
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        // ค้นหา user ที่ username หรือ email ตรงกัน และ password ถูกต้อง
        var user = _db.Users.FirstOrDefault(u =>
            (u.Username == username || u.Email == username) &&
            u.Password == password
        );

        if (user == null)
        {
            // login ไม่สำเร็จ → บันทึก log แล้วแสดง error
            LogUserLogin(username, false, ipAddress);
            ViewBag.Error = "Invalid username or password";
            return View();
        }

        // login สำเร็จ → เก็บ UserId และ Username ไว้ใน Session
        HttpContext.Session.SetString("UserId", user.UserId.ToString());
        HttpContext.Session.SetString("Username", user.Username);

        LogUserLogin(username, true, ipAddress);

        // ถ้าเป็น Admin (RoleId = 1) ให้ไปหน้า Admin Dashboard
        if (user.RoleId == 1)
            return RedirectToAction("Dashbordadmin", "AdminDashboard");

        // ถ้าเป็น user ทั่วไป ให้ไปหน้า Home
        return RedirectToAction("Home", "Home");
    }

    // GET: แสดงหน้าสมัครสมาชิก
    public IActionResult Signup() => View();

    // POST: รับข้อมูลแล้วสร้าง User ใหม่ลง database
    [HttpPost]
    public IActionResult Signup(string username, string email, string phone, string password, string confirmPassword)
    {
        // ตรวจสอบว่า password และ confirmPassword ตรงกันไหม
        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match";
            return View();
        }

        // สร้าง User ใหม่ โดย RoleId = 3 หมายถึง user ทั่วไป
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

        // สมัครเสร็จแล้วพาไปหน้า Login
        return RedirectToAction("Login");
    }

    // GET: ล้าง Session ทั้งหมดแล้ว redirect กลับหน้า Home
    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Home", "Home");
    }

    // Helper: บันทึก log การ login ลงไฟล์ .log แยกตามวันที่
    private void LogUserLogin(string username, bool success, string ipAddress)
    {
        try
        {
            // สร้าง folder logs ถ้ายังไม่มี
            string logDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            // ตั้งชื่อไฟล์ log ตามวันที่ เช่น login_2026-03-22.log
            string logFile = Path.Combine(logDir, $"login_{DateTime.Now:yyyy-MM-dd}.log");

            // รูปแบบ log: [เวลา] Username | สำเร็จไหม | IP
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Username: {username} | Success: {success} | IP: {ipAddress}\n";

            // เขียนต่อท้ายไฟล์ (AppendText ไม่ทับข้อมูลเดิม)
            using var writer = System.IO.File.AppendText(logFile);
            writer.WriteLine(logMessage);
        }
        catch (Exception ex)
        {
            // ถ้าเขียน log ไม่ได้ ให้บันทึก error ผ่าน ILogger แทน
            _logger.LogError("Error logging user login: {Message}", ex.Message);
        }
    }
}