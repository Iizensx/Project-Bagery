using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _66022380.Models;

namespace _66022380.Controllers;

public class AccountController : Controller
{   
    

    public IActionResult Home()
    {
        return View();
    }
    public IActionResult Signup()
    {
        return View();
    }

    public IActionResult Profile()
    {
        return View();
    }
     public IActionResult Menu()
    {
        return View();
    }
     public IActionResult lab8()
    {
        return View();
    }

    
     public IActionResult Dashbordadmin()
    {
        return View("~/Views/admin/Dashbordadmin.cshtml");
    }

    public IActionResult Stock()
    {
        return View("~/Views/admin/Stock.cshtml");
    }


    

    [HttpPost]
    public IActionResult Signup(string username, string email, string phone, string password, string confirmPassword)
    {
        // Simple redirect to Home after signup
        return RedirectToAction("Home");
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (username == "admin" && password == "admin")
        {
            return RedirectToAction("Dashbordadmin");
        }
        
        // Default login logic (can be expanded)
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            return RedirectToAction("Home");
        }

        ViewBag.Error = "Invalid username or password";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
