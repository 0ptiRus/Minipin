using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace exam_frontend.Models;

public class ResetPasswordModel
{
    [BindProperty]
    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
    
}