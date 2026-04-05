using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;
using _66022380.Viewmodels;

namespace _66022380.Controllers;

public class AccountController : Controller
{
    private readonly BakerydbContext _db;
    private readonly ILogger<AccountController> _logger;
    private const int WelcomePromotionId = 1;

    public AccountController(BakerydbContext db, ILogger<AccountController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Login() => View(new AuthUserViewModel());
    [HttpPost]
    public IActionResult Login(AuthUserViewModel model)
    {
        model ??= new AuthUserViewModel();
        model.Username = model.Username?.Trim() ?? string.Empty;
        model.Password ??= string.Empty;

        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(string.Empty, "Please enter your username/email and password.");
            return View(model);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var user = _db.Users.FirstOrDefault(u =>
            (u.Username == model.Username || u.Email == model.Username) &&
            !string.IsNullOrEmpty(u.Password));

        if (user == null || !string.Equals(user.Password, model.Password, StringComparison.Ordinal))
        {
            LogUserLogin(model.Username, false, ipAddress);
            ModelState.AddModelError(string.Empty, "Invalid username or password");
            model.Password = string.Empty;
            return View(model);
        }

        HttpContext.Session.SetString("UserId", user.UserId.ToString());
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("RoleId", (user.RoleId ?? 0).ToString());
        HttpContext.Session.SetString("RoleName", user.RoleId switch
        {
            1 => "Admin",
            2 => "Staff",
            _ => "User"
        });

        LogUserLogin(model.Username, true, ipAddress);

        if (user.RoleId == 1)
            return RedirectToAction("Dashbordadmin", "AdminDashboard");

        if (user.RoleId == 2)
            return RedirectToAction("Order", "AdminOrder");

        return RedirectToAction("Home", "Home");
    }

    public IActionResult Signup() => View(new AuthUserViewModel());

    [HttpPost]
    public IActionResult Signup(AuthUserViewModel model)
    {
        model ??= new AuthUserViewModel();
        model.Username = model.Username?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
        model.Phone = model.Phone?.Trim() ?? string.Empty;
        model.Password ??= string.Empty;
        model.ConfirmPassword ??= string.Empty;

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match");
            model.Password = string.Empty;
            model.ConfirmPassword = string.Empty;
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(string.Empty, "Please fill in all required fields");
            model.Password = string.Empty;
            model.ConfirmPassword = string.Empty;
            return View(model);
        }

        if (_db.Users.Any(u => u.Username == model.Username || u.Email == model.Email))
        {
            ModelState.AddModelError(string.Empty, "Username or email already exists");
            model.Password = string.Empty;
            model.ConfirmPassword = string.Empty;
            return View(model);
        }

        var newUser = new User
        {
            Username = model.Username,
            Email = model.Email,
            Phone = model.Phone,
            Password = model.Password,
            RoleId = 3
        };

        _db.Users.Add(newUser);
        _db.SaveChanges();

        GrantWelcomePromotion(newUser.UserId);

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Home", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View("~/Views/Account/ForgotPassword.cshtml");
    }

    [HttpPost]
    public IActionResult RequestPasswordReset([FromBody] ForgotPasswordRequestViewModel model)
    {
        var email = model.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return Json(new { success = false, message = "Please enter your email first." });

        var user = _db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
            return Json(new { success = false, message = "This email was not found in the system." });

        var otpCode = RandomNumberGenerator.GetInt32(1000, 10000).ToString("0000");
        var otpExpiredAt = DateTime.Now.AddMinutes(5);

        user.OtpCode = otpCode;
        user.OtpExpiredAt = otpExpiredAt;
        _db.SaveChanges();

        HttpContext.Session.Remove("PasswordResetVerifiedEmail");
        HttpContext.Session.Remove("PasswordResetVerifiedOtp");
        HttpContext.Session.Remove("PasswordResetVerifiedUserId");

        return Json(new
        {
            success = true,
            message = "Mock OTP created successfully.",
            mockOtp = otpCode,
            expiresAt = otpExpiredAt.ToString("HH:mm")
        });
    }

    [HttpPost]
    public IActionResult VerifyPasswordResetOtp([FromBody] ForgotPasswordVerifyOtpViewModel model)
    {
        var email = model.Email?.Trim();
        var otpCode = model.OtpCode?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            return Json(new { success = false, message = "Please enter both email and OTP." });

        var user = _db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null || string.IsNullOrWhiteSpace(user.OtpCode) || !user.OtpExpiredAt.HasValue)
        {
            HttpContext.Session.Remove("PasswordResetVerifiedEmail");
            HttpContext.Session.Remove("PasswordResetVerifiedOtp");
            HttpContext.Session.Remove("PasswordResetVerifiedUserId");
            return Json(new { success = false, message = "No OTP request was found. Please request a new code." });
        }

        if (user.OtpExpiredAt.Value < DateTime.Now)
        {
            HttpContext.Session.Remove("PasswordResetVerifiedEmail");
            HttpContext.Session.Remove("PasswordResetVerifiedOtp");
            HttpContext.Session.Remove("PasswordResetVerifiedUserId");
            return Json(new { success = false, message = "OTP has expired. Please request a new code." });
        }

        if (!string.Equals(user.OtpCode, otpCode, StringComparison.Ordinal))
        {
            HttpContext.Session.Remove("PasswordResetVerifiedEmail");
            HttpContext.Session.Remove("PasswordResetVerifiedOtp");
            HttpContext.Session.Remove("PasswordResetVerifiedUserId");
            return Json(new { success = false, message = "OTP is incorrect. Please try again." });
        }

        HttpContext.Session.SetString("PasswordResetVerifiedEmail", email);
        HttpContext.Session.SetString("PasswordResetVerifiedOtp", otpCode);
        HttpContext.Session.SetString("PasswordResetVerifiedUserId", user.UserId.ToString());
        return Json(new { success = true, message = "OTP verified. You can now set a new password." });
    }

    [HttpPost]
    public IActionResult ResetPassword([FromBody] ForgotPasswordResetViewModel model)
    {
        var email = model.Email?.Trim();
        var otpCode = model.OtpCode?.Trim();
        var verifiedEmail = HttpContext.Session.GetString("PasswordResetVerifiedEmail");
        var verifiedOtp = HttpContext.Session.GetString("PasswordResetVerifiedOtp");
        var verifiedUserId = HttpContext.Session.GetString("PasswordResetVerifiedUserId");

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(otpCode) ||
            string.IsNullOrWhiteSpace(model.NewPassword) ||
            string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            return Json(new { success = false, message = "Please fill in all fields." });
        }

        if (!string.Equals(email, verifiedEmail, StringComparison.OrdinalIgnoreCase))
            return Json(new { success = false, message = "OTP verification session is invalid. Please verify OTP again." });

        if (!string.Equals(otpCode, verifiedOtp, StringComparison.Ordinal))
            return Json(new { success = false, message = "OTP does not match the verified request. Please verify OTP again." });

        if (model.NewPassword != model.ConfirmPassword)
            return Json(new { success = false, message = "New password and confirmation do not match." });

        if (model.NewPassword.Length < 6)
            return Json(new { success = false, message = "New password must be at least 6 characters long." });

        var user = _db.Users.FirstOrDefault(u =>
            u.Email == email &&
            verifiedUserId != null &&
            u.UserId.ToString() == verifiedUserId);
        if (user == null)
            return Json(new { success = false, message = "User was not found." });

        if (!user.OtpExpiredAt.HasValue || user.OtpExpiredAt.Value < DateTime.Now || string.IsNullOrWhiteSpace(user.OtpCode))
            return Json(new { success = false, message = "OTP has expired. Please request a new code." });

        if (!string.Equals(user.OtpCode, otpCode, StringComparison.Ordinal))
            return Json(new { success = false, message = "OTP is incorrect for this email. Please verify again." });

        user.Password = model.NewPassword;
        user.OtpCode = null;
        user.OtpExpiredAt = null;
        _db.SaveChanges();

        HttpContext.Session.Remove("PasswordResetVerifiedEmail");
        HttpContext.Session.Remove("PasswordResetVerifiedOtp");
        HttpContext.Session.Remove("PasswordResetVerifiedUserId");

        return Json(new
        {
            success = true,
            message = "Password changed successfully. Please sign in again.",
            redirectUrl = Url.Action("Login", "Account")
        });
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

    private void GrantWelcomePromotion(int userId)
    {
        try
        {
            var welcomePromotion = _db.Promotions.FirstOrDefault(p => p.PromotionId == WelcomePromotionId);
            if (welcomePromotion == null)
            {
                _logger.LogWarning("Welcome promotion {PromotionId} was not found for new user {UserId}", WelcomePromotionId, userId);
                return;
            }

            var alreadyGranted = _db.UserPromotions.Any(up => up.UserId == userId && up.PromotionId == WelcomePromotionId);
            if (alreadyGranted)
                return;

            _db.UserPromotions.Add(new UserPromotion
            {
                UserId = userId,
                PromotionId = WelcomePromotionId,
                IsUsed = 0,
                UsedAt = null
            });

            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to grant welcome promotion {PromotionId} to user {UserId}", WelcomePromotionId, userId);
        }
    }
}
