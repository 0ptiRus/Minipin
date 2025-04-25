using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ElectronNET.API;
using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace exam_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IConfiguration config;
    private readonly UserManager<ApplicationUser> user_manager;
    private readonly RoleManager<IdentityRole> role_manager;
    private readonly ILogger logger;
    private readonly AppDbContext context;
    private readonly FileService fileService;
    private readonly MinioService minio_service;
    private readonly RedisService redis_service;
    private readonly IEmailService email_service;

    private readonly string cache_prefix = "Users";

    public AccountController(UserManager<ApplicationUser> userManager, IConfiguration config, 
        ILogger<UserController> logger, AppDbContext context,
        FileService fileService, MinioService minioService, RedisService redisService,
        IEmailService emailService, RoleManager<IdentityRole> roleManager)
    {
        user_manager = userManager;
        this.config = config;
        this.logger = logger;
        this.context = context;
        this.fileService = fileService;
        minio_service = minioService;
        redis_service = redisService;
        email_service = emailService;
        role_manager = roleManager;
    }
    

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        logger.LogInformation($"Trying to find user with email {model.Email}");
        ApplicationUser user = await user_manager.FindByEmailAsync(model.Email);
        
        if (user == null || !await user_manager.CheckPasswordAsync(user, model.Password))
        {
            logger.LogInformation($"User with email {model.Email} not authorized");
            return Unauthorized(new { StatusCode = StatusCodes.Status401Unauthorized, Message = "Couldn't log in with given credentials" });
        }

        if (user.IsBanned)
        {
            logger.LogInformation($"User with email {model.Email} is banned");
            return Unauthorized(new
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Sorry, but you've been banned. Contact us via our company email to appeal."
            });
        }
        
        logger.LogInformation($"Making token for user with email {user.Email}");
        
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "User"),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        
        logger.LogInformation($"Token has been created for user {user.Email}");
        logger.LogDebug($"Jwt token: {tokenHandler.WriteToken(token)}");
        //
        // // Set the token in a cookie
        // var cookieOptions = new CookieOptions
        // {
        //     HttpOnly = false, // Prevent JavaScript access
        //     Secure = true,   // Ensure the cookie is only sent over HTTPS
        //     SameSite = SameSiteMode.Lax, // Protect against CSRF
        //     Expires = DateTime.UtcNow.AddHours(1), // Set expiration time
        //     Path = "/",
        //     IsEssential = true
        // };
        //
        // Response.Cookies.Append("jwt", tokenHandler.WriteToken(token), cookieOptions);
        //
        return Ok(new { StatusCode = StatusCodes.Status200OK, Message = tokenHandler.WriteToken(token) });
    }
    
    [HttpPost("login_admin")]
    public async Task<IActionResult> LoginAdmin([FromBody] LoginModel model)
    {
        logger.LogInformation($"Trying to find admin with email {model.Email}");
        ApplicationUser user = await user_manager.FindByEmailAsync(model.Email);
        
        if (user == null || !await user_manager.CheckPasswordAsync(user, model.Password))
        { 
            logger.LogInformation($"Admin with email {model.Email} not authorized");
            return Unauthorized(new { StatusCode = StatusCodes.Status401Unauthorized, Message = "Couldn't log in with given credentials" });
        }
        
        var roles = await user_manager.GetRolesAsync(user);

        if (!roles.Contains("Admin"))
        {
            logger.LogInformation($"User with email {model.Email} is not an admin");
            return Unauthorized(new
                { StatusCode = StatusCodes.Status401Unauthorized, Message = "Couldn't log in with given credentials" });
        }
        
        logger.LogInformation($"Making token for admin with email {user.Email}");
        
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Admin"),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        
        logger.LogInformation($"Token has been created for user {user.Email}");
        logger.LogDebug($"Jwt token: {tokenHandler.WriteToken(token)}");
        
        return Ok(new { StatusCode = StatusCodes.Status200OK, Message = $"{tokenHandler.WriteToken(token)}" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        ApplicationUser user = new ApplicationUser { UserName = model.Username, Email = model.Email };
        logger.LogInformation($"Trying to create user..");
        IdentityResult result = await user_manager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            logger.LogWarning($"Failed to create user: {result.Errors}");
            return BadRequest(new { IsCreated = false, Message = result.Errors.ToString()});   
        }

        if (model.Pfp is not null)
        {
            string object_name = $"{Guid.NewGuid()}_{model.Pfp.FileName}";
            UploadedFile file = new UploadedFile
            {
                ObjectName = object_name,
                ContentType = model.Pfp.ContentType,
                UserId = user.Id
            };

            UploadedFile? creation_result = await fileService.CreateFile(file, model.Pfp, minio_service.GetBucketNameForFile(file.ContentType));
        
            if (creation_result is null)
            {
                logger.LogWarning($"Failed to create user: {result.Errors}");
                return BadRequest(new { IsCreated = false, Message = "Couldn't upload profile picture. Try again later"});   
            }            
        }

        logger.LogInformation($"Created user: {user.Email}");
        await redis_service.RemoveAllKeysAsync($"{cache_prefix}");
        return Ok(new { IsCreated = true, Message = "OK" });
    }
    
    [HttpPost("register_admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        if (!await role_manager.RoleExistsAsync("Admin"))
        {
            await role_manager.CreateAsync(new IdentityRole("Admin"));
        }
        
        ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        
        logger.LogInformation($"Trying to create admin..");
        IdentityResult result = await user_manager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            logger.LogWarning($"Failed to create admin:");
            foreach (var error in result.Errors)
                logger.LogError(error.Description, error);
            return BadRequest(new { IsCreated = false, Message = result.Errors.Select(error => error.Description)
                .Aggregate((current, next) => $"{current}, {next}")});   
        }
        
        await user_manager.AddToRoleAsync(user, "Admin");
        
        logger.LogInformation($"Created admin: {user.Email}");
        await redis_service.RemoveAllKeysAsync($"{cache_prefix}");
        return Ok(new { IsCreated = true, Message = "OK" });
    }
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        ApplicationUser? user = await user_manager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Ok(new { Message = "If the email exists, a reset link will be sent." });
        }

        string resetToken = await user_manager.GeneratePasswordResetTokenAsync(user);
        
        var frontend_url = config["FrontendSettings:BaseUrl"];
        string reset_link = $"{frontend_url}/Account/ResetPassword?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email)}";
        
        await email_service.SendEmailAsync(user.Email, "Password Reset", $"Click the following link to reset your password: {reset_link}");

        return Ok(new { Message = "If the email exists, a reset link will be sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        ApplicationUser? user = await user_manager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest(new { Message = "Invalid email or token." });
        }

        IdentityResult result = await user_manager.ResetPasswordAsync(user, request.ResetCode, request.NewPassword);


        if (!result.Succeeded)
        {
            logger.LogWarning($"Failed to reset password: {result.Errors}");
            return BadRequest(new { Message = "Password reset failed.", Errors = result.Errors });
        }

        // Password reset successful
        return Ok(new { Message = "Password has been reset successfully." });
    }

}