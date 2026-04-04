namespace _66022380.Models;

public class ForgotPasswordRequestViewModel
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordVerifyOtpViewModel
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}

public class ForgotPasswordResetViewModel
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
