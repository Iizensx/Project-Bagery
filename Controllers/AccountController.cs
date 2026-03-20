using Microsoft.AspNetCore.Mvc;
using _66022380.Models.Db;

namespace _66022380.Controllers;

public class AccountController : Controller
{
    private readonly BakerydbContext _db;
    private readonly ILogger<AccountController> _logger;

    public AccountController(BakerydbContext db, ILogger<AccountController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var user = _db.Users.FirstOrDefault(u =>
            (u.Username == username || u.Email == username) &&
            u.Password == password
        );

        if (user == null)
        {
            LogUserLogin(username, false, ipAddress);
            ViewBag.Error = "Invalid username or password";
            return View();
        }

        HttpContext.Session.SetString("UserId", user.UserId.ToString());
        HttpContext.Session.SetString("Username", user.Username);

        LogUserLogin(username, true, ipAddress);

        if (user.RoleId == 1)
            return RedirectToAction("Dashbordadmin", "AdminDashboard");

        return RedirectToAction("Home", "Home");
    }

    public IActionResult Signup() => View();

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

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Home", "Home");
    }

    private void LogUserLogin(string username, bool success, string ipAddress)
    {
        try
        {
            string logDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            string logFile = Path.Combine(logDir, $"login_{DateTime.Now:yyyy-MM-dd}.log");
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Username: {username} | Success: {success} | IP: {ipAddress}\n";

            using var writer = System.IO.File.AppendText(logFile);
            writer.WriteLine(logMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error logging user login: {Message}", ex.Message);
        }
    }
}
