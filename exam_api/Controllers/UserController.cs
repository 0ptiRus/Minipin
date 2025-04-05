using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElectronNET.API;
using exam_api.Entities;
using exam_api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace exam_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IConfiguration config;
    private readonly UserManager<ApplicationUser> user_manager;
    private readonly ILogger logger;
    private readonly AppDbContext context;

    public UserController(UserManager<ApplicationUser> userManager, IConfiguration config, 
        ILogger<UserController> logger, AppDbContext context)
    {
        user_manager = userManager;
        this.config = config;
        this.logger = logger;
        this.context = context;
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
        
        return Ok(new { StatusCode = StatusCodes.Status200OK, Message = $"{tokenHandler.WriteToken(token)}" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
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
        
        logger.LogInformation($"Created user: {user.Email}");
        return Ok(new { IsCreated = true, Message = "OK" });
    }

    [HttpGet("profile/{user_id}")]
    public async Task<IActionResult> GetProfile(string user_id)
    {
        var user = await context.Users
            .Include(u => u.Galleries)
            .Include(u => u.Followers)
            .Include(u => u.Followed)
            .FirstOrDefaultAsync(u => u.Id == user_id);

        if (user == null) return NotFound();

        return Ok(new ProfileViewModel
        {
            User = user,
            Galleries = user.Galleries.ToList(),
        });
    }
}