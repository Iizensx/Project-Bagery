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

    // รับ BakerydbContext มาผ่าน Dependency Injection
    public HomeController(BakerydbContext db)
    {
        _db = db;
    }

    // GET: แสดงหน้าแรก (Home)
    public IActionResult Home()
    {
        return View("~/Views/Account/Home.cshtml");
    }

    // GET: ดึงสินค้าทั้งหมดพร้อม Category แล้วแสดงหน้าเมนู
    public IActionResult Menu()
    {
        var products = _db.Stocks
            .Include(s => s.Category) // JOIN กับตาราง Category เพื่อดึงชื่อหมวดหมู่
            .ToList();

        return View("~/Views/Account/Menu.cshtml", products);
    }

    // GET: กด Contact แล้ว redirect กลับไปหน้า Home
    public IActionResult Contact()
    {
        return RedirectToAction("Home");
    }

    // GET: ดึง User ทั้งหมดจาก database มาแสดง (ใช้ LINQ Query Syntax)
    public IActionResult lab8()
    {
        var users = (from u in _db.Users select u).ToList();
        return View(users);
    }

    // GET: แสดงหน้า Privacy Policy
    public IActionResult Privacy()
    {
        return View();
    }

    // GET: แสดงหน้า Error พร้อมแนบ RequestId เพื่อใช้ตรวจสอบ log
    // ResponseCache: ไม่ให้ cache หน้า Error เพราะข้อมูลต้องแสดงแบบ real-time เสมอ
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}