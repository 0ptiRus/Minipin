using exam_api.Data;
using exam_api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace exam_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> user_manager;
    private readonly SignInManager<ApplicationUser> signin_manager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        user_manager = userManager;
        signin_manager = signinManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        IdentityResult result = await user_manager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await signin_manager.SignInAsync(user, isPersistent: false);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        SignInResult result = await signin_manager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: true);

        if (result.Succeeded)
            return Ok();

        return Unauthorized();
    }
}