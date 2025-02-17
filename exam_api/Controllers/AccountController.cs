using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using exam_api.Data;
using exam_api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace exam_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> user_manager;
    private readonly SignInManager<ApplicationUser> signin_manager;
    private readonly IConfiguration config;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager, IConfiguration config)
    {
        user_manager = userManager;
        signin_manager = signinManager;
        this.config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        ApplicationUser user = new ApplicationUser { UserName = model.Username, Email = model.Email };
        IdentityResult result = await user_manager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(new { IsCreated = false, Message = result.Errors.ToString()});

        return Ok(new {IsCreated = true, Message = "OK" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // if (model.Password == "testpass" && model.Email == "testemail@mail.com")
        // {
        //     Claim[] claims = new[]
        //     {
        //         new Claim(ClaimTypes.NameIdentifier, model.Email),
        //         new Claim(ClaimTypes.Name, model.Email)
        //     };
        //
        //     ClaimsIdentity claims_identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //     ClaimsPrincipal claims_principal = new(claims_identity);
        //
        //     await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims_principal);
        //
        //     return Ok(new { Message = "OK" });
        // }

        SignInResult result = await signin_manager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            Claim[] claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, model.Email),
                new Claim(ClaimTypes.Name, model.Email)
            };

            ClaimsIdentity claims_identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claims_principal = new(claims_identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims_principal);

            return Ok(new { Message = "OK" });
        }

        return Unauthorized();
    }
}