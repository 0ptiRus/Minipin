using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace exam_frontend.Models;

public class UserLoginModel
{
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}
 
public class UserRegisterModel
{
    [Required, Length(1, 100)]
    public string Username { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [ValidateNever]
    public IFormFile Pfp { get; set; }
}