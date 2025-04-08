using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElectronNET.API;
using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
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
    private readonly FileService fileService;
    private readonly MinioService minio_service;

    public UserController(UserManager<ApplicationUser> userManager, IConfiguration config, 
        ILogger<UserController> logger, AppDbContext context,
        FileService fileService, MinioService minioService)
    {
        user_manager = userManager;
        this.config = config;
        this.logger = logger;
        this.context = context;
        this.fileService = fileService;
        minio_service = minioService;
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
        
        string object_name = $"{Guid.NewGuid()}_{model.Pfp.FileName}";
        UploadedFile file = new UploadedFile
        {
            ObjectName = object_name,
            UserId = user.Id
        };
        if (await fileService.CreateFile(file, model.Pfp) is not null)
        {
            logger.LogWarning($"Failed to create user: {result.Errors}");
            return BadRequest(new { IsCreated = false, Message = "Couldn't upload profile picture. Try again later"});   
        }
        logger.LogInformation($"Created user: {user.Email}");
        return Ok(new { IsCreated = true, Message = "OK" });
    }

    [HttpGet("profile/{user_id}")]
    public async Task<IActionResult> GetProfile(string user_id)
    {
        var user = await context.Users
            .Include(u => u.Pfp)
            .Include(u => u.Galleries)
                .ThenInclude(g => g.Cover)
            .Include(u => u.Posts)
            .Include(u => u.Followers)
            .Include(u => u.Followed)
            .FirstOrDefaultAsync(u => u.Id == user_id);

        if (user == null) return NotFound();
        
        IList<PreviewGalleryModel> galleries = await Task.WhenAll(user.Galleries
            .Select(async g => new PreviewGalleryModel
            {
                Name = g.Name,
                CoverUrl = await minio_service.GetFileUrlAsync(g.Cover.ObjectName)
            }));
        

        return Ok(new ProfileViewModel
        {
            Username = user.UserName,
            PfpUrl = await minio_service.GetFileUrlAsync(user.Pfp.ObjectName),
            FollowerCount = user.Followers.Count(),
            FollowingCount = user.Followed.Count(),
            PinCount = user.Posts.Count(),
            Galleries = galleries
        });
    }
}