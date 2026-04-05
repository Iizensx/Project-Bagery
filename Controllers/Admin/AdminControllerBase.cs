using Microsoft.AspNetCore.Mvc;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

// Base Controller สำหรับฝั่ง Admin
// รวมเมธอดที่ใช้ซ้ำร่วมกัน เช่น การเข้าถึง DbContext, การอ่าน UserId จาก Session
// และการตรวจสอบว่าผู้ใช้ปัจจุบันมีสิทธิ์เป็น Admin หรือไม่
public abstract class AdminControllerBase : Controller
{
    protected readonly BakerydbContext Db;
    protected const int AdminRoleId = 1;
    protected const int StaffRoleId = 2;

    // รับ BakerydbContext มาผ่าน Dependency Injection
    protected AdminControllerBase(BakerydbContext db)
    {
        Db = db;
    }

    // ตรวจสอบว่าผู้ใช้ที่ login อยู่มี RoleId = 1 (Admin) หรือไม่
    protected bool IsCurrentUserAdmin()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return false;

        return Db.Users.Any(u => u.UserId == userId && u.RoleId == AdminRoleId);
    }

    protected bool IsCurrentUserStaff()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return false;

        return Db.Users.Any(u => u.UserId == userId && u.RoleId == StaffRoleId);
    }

    protected bool IsCurrentUserAdminOrStaff()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return false;

        return Db.Users.Any(u => u.UserId == userId && (u.RoleId == AdminRoleId || u.RoleId == StaffRoleId));
    }

    // ดึง UserId ของผู้ใช้ปัจจุบันจาก Session
    // ถ้าไม่มีหรือแปลงค่าไม่ได้ จะคืน 0 เพื่อใช้แทน "ยังไม่ได้ login"
    protected int GetCurrentUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        return int.TryParse(userIdString, out var userId) ? userId : 0;
    }

    // ถ้าไม่ใช่ Admin ให้ redirect กลับไปหน้า Login ของระบบหลัก
    protected IActionResult RedirectToAdminLogin()
    {
        if (IsCurrentUserStaff())
            return RedirectToAction("Order", "AdminOrder", new { area = "" });

        if (IsCurrentUserAdmin())
            return RedirectToAction("Dashbordadmin", "AdminDashboard", new { area = "" });

        return RedirectToAction("Login", "Account", new { area = "" });
    }
}
