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
    public IActionResult Profile() => View();
    public IActionResult Menu() => View();
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

        if (user.RoleId == 1)
            return RedirectToAction("Dashbordadmin");

        return RedirectToAction("Home");
    }

    public IActionResult Lab10(int id = 0)
    {   
        var check = ( from us in _db.Users where us.UserId == id select new UserViewModel
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