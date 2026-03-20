using Microsoft.AspNetCore.Mvc;
using _66022380.Models.Db;

namespace _66022380.Controllers.Admin;

public abstract class AdminControllerBase : Controller
{
    protected readonly BakerydbContext Db;

    protected AdminControllerBase(BakerydbContext db)
    {
        Db = db;
    }

    protected bool IsCurrentUserAdmin()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return false;

        return Db.Users.Any(u => u.UserId == userId && u.RoleId == 1);
    }

    protected int GetCurrentUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        return int.TryParse(userIdString, out var userId) ? userId : 0;
    }

    protected IActionResult RedirectToAdminLogin()
    {
        return RedirectToAction("Login", "Account", new { area = "" });
    }
}
