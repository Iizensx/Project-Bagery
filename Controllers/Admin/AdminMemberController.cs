using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

// Controller สำหรับจัดการสมาชิกและบทบาทผู้ใช้
// รองรับการดูรายชื่อผู้ใช้ เพิ่มสมาชิกใหม่ แก้ไขข้อมูลเดิม และลบผู้ใช้
public class AdminMemberController : AdminControllerBase
{
    // รับ BakerydbContext จากคลาสแม่
    public AdminMemberController(BakerydbContext db) : base(db)
    {
    }

    // GET: แสดงหน้าจัดการสมาชิก พร้อมรายชื่อผู้ใช้และข้อมูลบทบาท
    public IActionResult Member()
    {
        // จำกัดสิทธิ์เฉพาะ Admin
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ส่งรายการบทบาทไปให้หน้า View ใช้สร้างตัวเลือกในฟอร์ม
        ViewBag.Roles = Db.Roles.OrderBy(r => r.RoleId).ToList();

        // ดึงผู้ใช้ทั้งหมดพร้อมบทบาท เพื่อแสดงในหน้าจัดการสมาชิก
        var users = Db.Users
            .Include(u => u.Role)
            .OrderBy(u => u.UserId)
            .ToList();

        return View("~/Views/admin/Member.cshtml", users);
    }

    // POST: เพิ่มผู้ใช้ใหม่ หรือแก้ไขข้อมูลผู้ใช้เดิม
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveMember(int userId, string username, string? email, string? phone, string? password, int? roleId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ตรวจสอบข้อมูลก่อนบันทึก
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

        // ถ้ามี userId แสดงว่าเป็นการแก้ไขผู้ใช้เดิม
        if (userId > 0)
        {
            var user = Db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                TempData["MemberError"] = "ไม่พบข้อมูลผู้ใช้งาน";
                return RedirectToAction("Member");
            }

            // อัปเดตข้อมูลพื้นฐาน และเปลี่ยนรหัสผ่านเฉพาะเมื่อมีการกรอกค่าใหม่
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

        // ถ้าเป็นการเพิ่มผู้ใช้ใหม่ ต้องมีรหัสผ่านเสมอ
        if (string.IsNullOrWhiteSpace(password))
        {
            TempData["MemberError"] = "การเพิ่มผู้ใช้งานใหม่ต้องกำหนดรหัสผ่าน";
            return RedirectToAction("Member");
        }

        // สร้างผู้ใช้ใหม่ลงฐานข้อมูล
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

    // POST: ลบผู้ใช้ตาม userId
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteMember(int userId)
    {
        if (!IsCurrentUserAdmin())
            return RedirectToAdminLogin();

        // ป้องกันไม่ให้แอดมินลบบัญชีของตัวเองระหว่างใช้งานระบบ
        var currentAdminId = GetCurrentUserId();
        if (currentAdminId == userId)
        {
            TempData["MemberError"] = "ไม่สามารถลบบัญชีแอดมินที่กำลังใช้งานอยู่ได้";
            return RedirectToAction("Member");
        }

        // ค้นหาผู้ใช้ที่ต้องการลบ
        var user = Db.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
        {
            TempData["MemberError"] = "ไม่พบข้อมูลผู้ใช้งาน";
            return RedirectToAction("Member");
        }

        // ลบผู้ใช้ออกจากระบบแล้วบันทึกลงฐานข้อมูล มีแค่นี้เลยจริงๆมั้ง
        Db.Users.Remove(user);
        Db.SaveChanges();

        TempData["MemberSuccess"] = "ลบผู้ใช้งานเรียบร้อย";
        return RedirectToAction("Member");
    }
}
