using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

public class AdminMemberController : AdminControllerBase
{
    public AdminMemberController(BakerydbContext db) : base(db)
    {
    }

    public IActionResult Member()
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        ViewBag.Roles = Db.Roles.OrderBy(r => r.RoleId).ToList();
        var users = Db.Users
            .Include(u => u.Role)
            .OrderBy(u => u.UserId)
            .ToList();

        return View("~/Views/admin/Member.cshtml", users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveMember(int userId, string username, string? email, string? phone, string? password, int? roleId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        if (string.IsNullOrWhiteSpace(username))
        {
            TempData["MemberError"] = "กรุณากรอกชื่อผู้ใช้งาน";
            return RedirectToAction("Member");
        }

        if (roleId == null || !Db.Roles.Any(r => r.RoleId == roleId))
        {
            TempData["MemberError"] = "กรุณาเลือกสิทธิ์ผู้ใช้งานให้ถูกต้อง";
            return RedirectToAction("Member");
        }

        if (userId > 0)
        {
            var user = Db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                TempData["MemberError"] = "ไม่พบข้อมูลผู้ใช้งาน";
                return RedirectToAction("Member");
            }

            user.Username = username.Trim();
            user.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            user.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            user.RoleId = roleId;

            if (!string.IsNullOrWhiteSpace(password))
                user.Password = password.Trim();

            Db.SaveChanges();
            TempData["MemberSuccess"] = "อัปเดตข้อมูลผู้ใช้งานเรียบร้อย";
            return RedirectToAction("Member");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            TempData["MemberError"] = "การเพิ่มผู้ใช้งานใหม่ต้องกำหนดรหัสผ่าน";
            return RedirectToAction("Member");
        }

        Db.Users.Add(new User
        {
            Username = username.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Password = password.Trim(),
            RoleId = roleId
        });
        Db.SaveChanges();

        TempData["MemberSuccess"] = "เพิ่มผู้ใช้งานใหม่เรียบร้อย";
        return RedirectToAction("Member");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteMember(int userId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        var currentAdminId = GetCurrentUserId();
        if (currentAdminId == userId)
        {
            TempData["MemberError"] = "ไม่สามารถลบบัญชีแอดมินที่กำลังใช้งานอยู่ได้";
            return RedirectToAction("Member");
        }

        var user = Db.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
        {
            TempData["MemberError"] = "ไม่พบข้อมูลผู้ใช้งาน";
            return RedirectToAction("Member");
        }

        Db.Users.Remove(user);
        Db.SaveChanges();

        TempData["MemberSuccess"] = "ลบผู้ใช้งานเรียบร้อย";
        return RedirectToAction("Member");
    }
}
