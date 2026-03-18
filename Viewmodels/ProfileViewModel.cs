using _66022380.Models.Db;

namespace _66022380.Models;

public class ProfileViewModel
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // เปลี่ยนรหัสผ่าน
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }

    // ที่อยู่
    public List<Address> Addresses { get; set; } = new();
}