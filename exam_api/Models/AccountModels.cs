using Microsoft.AspNetCore.Http;

namespace exam_api.Models;

public class RegisterModel
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public IFormFile Pfp { get; set; }
}

public class LoginModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}