using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;
using Microsoft.EntityFrameworkCore;
using System.Xml;   

namespace _66022380.Controllers;

public class HomeController : Controller
{

    private readonly BakerydbContext _db;

    public HomeController(BakerydbContext db)
    {
        _db = db;
    }

    
    public IActionResult Home()
    {
        return View("~/Views/Account/Home.cshtml");
    }

    public IActionResult Menu()
    {
        var products = _db.Stocks
            .Include(s => s.Category)
            .ToList();

        return View("~/Views/Account/Menu.cshtml", products);
    }

    public IActionResult Contact()
    {
        return RedirectToAction("Home");
    }

    public IActionResult lab8()
    {
        var users = (from u in _db.Users select u).ToList();
        return View(users); 
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
