namespace exam_api.Models;

public class ResetPasswordViewModel
{
    public string Token { get; set; }
    public string Email { get; set; }
    public string NewPassword { get; set; }
}