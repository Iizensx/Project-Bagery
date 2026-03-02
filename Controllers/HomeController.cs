using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;

namespace _66022380.Controllers;

public class HomeController : Controller
{

    private readonly BakerydbContext _db;

    public HomeController(BakerydbContext db)
    {
        _db = db;
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
