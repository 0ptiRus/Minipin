using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElectronNET.API;
using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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
    private readonly RoleManager<IdentityRole> role_manager;
    private readonly ILogger logger;
    private readonly AppDbContext context;
    private readonly FileService fileService;
    private readonly MinioService minio_service;
    private readonly RedisService redis_service;
    private readonly IEmailService email_service;

    private readonly string cache_prefix = "Users";

    public UserController(UserManager<ApplicationUser> userManager, IConfiguration config, 
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
    public async Task<IActionResult> RegisterAdmin([FromForm] RegisterModel model)
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

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? filter = "", [FromQuery] string? search = "")
    {
            PagedResponse<ViewUserModel> cached_result =
                await redis_service.GetValueAsync<PagedResponse<ViewUserModel>>($"{cache_prefix}:{(filter == "" ? "all" : filter)}" +
                    $":" +
                    $"{(search == "" ? "" : search)}");
            if (cached_result != null)
            {
                logger.LogInformation("Found all users in cache");
                return Ok(cached_result);
            }
        
        logger.LogInformation("Returning all users");
        IList<ApplicationUser> users = await context.Users
            .Include(u => u.Pfp)
            .ToListAsync();

        if (filter != "")
        {
            users = filter.ToLower() switch
            {
                "administrators" => await user_manager.GetUsersInRoleAsync("Admin"),
                "banned" => users.Where(u => u.IsBanned).ToList(),
                _ => users
            };
            logger.LogInformation($"Applied filter {filter}");
        }

        if (search != "")
        {
            users = users.Where(u => u.UserName.ToLower().Contains(search.ToLower()) || u.Email.ToLower().Contains(search.ToLower())).ToList();
        }
        
        IList<ViewUserModel> models = await Task.WhenAll(users
            .Select(async u => new ViewUserModel
            {
                Id = u.Id,
                Username = u.UserName,
                Email = u.Email,
                Pfp = u.Pfp is not null ? await minio_service.GetFileUrlAsync(u.Pfp.ObjectName, minio_service.GetBucketNameForFile(u.Pfp.ContentType)) : "",
                IsBanned = u.IsBanned,
                Role = (await user_manager.GetRolesAsync(u)).FirstOrDefault()
            })
            .ToList());

        PagedResponse<ViewUserModel> response = new PagedResponse<ViewUserModel>
        {
            Items = models.ToList(),
            TotalItems = users.Count
        };
        
        redis_service.SetValueAsync($"{cache_prefix}:{(filter == "" ? "all" : filter)}:{(search == "" ? "" : search)}", response);
        
        return Ok(response);
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

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        logger.LogInformation($"Getting user stats");
        
        int all_users = context.Users.Count();
        int admins = (await user_manager.GetUsersInRoleAsync("Admin")).Count;
        int banned = context.Users.Count(user => user.IsBanned);

        UserStats model = new UserStats
        {
            TotalUsers = all_users,
            Administrators = admins,
            BannedUsers = banned
        };

        return Ok(model);
    }

    [HttpGet("profile/{user_id}")]
    public async Task<IActionResult> GetProfile(string user_id)
    {
        ProfileViewModel cached_user_model = await redis_service.GetValueAsync<ProfileViewModel>($"{cache_prefix}:profile:{user_id}");
        if (cached_user_model != default)
        {
            return Ok(cached_user_model);
        }

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
            .Where(g => !g.IsDeleted)
            .Select(async g => new PreviewGalleryModel
            {
                Id = g.Id,
                UserId = user.Id,
                Name = g.Name,
                CoverUrl = await minio_service.GetFileUrlAsync(g.Cover.ObjectName, minio_service.GetBucketNameForFile(g.Cover.ContentType)),
            }));

        ProfileViewModel model = new ProfileViewModel
        {
            Id = user_id,
            Username = user.UserName,
            PfpUrl = await minio_service.GetFileUrlAsync(user.Pfp.ObjectName, minio_service.GetBucketNameForFile(user.Pfp.ContentType)),
            FollowerCount = user.Followers.Count(),
            FollowingCount = user.Followed.Count(),
            PinCount = user.Posts.Where(p => !p.IsDeleted).Count(),
            Galleries = galleries
        };

        logger.LogInformation($"Profile found for user {user_id}");
        
        await redis_service.SetValueAsync($"{cache_prefix}:profile:{user_id}", model);
        
        return Ok(model);
    }
    
    [HttpPost("ban")]
    public async Task<IActionResult> BanUser([FromBody] BanUserRequest request)
    {
        var user = await user_manager.FindByIdAsync(request.UserId);
        if (user == null) return NotFound();

        user.IsBanned = true;
        await user_manager.UpdateAsync(user);
        
        await redis_service.RemoveAllKeysAsync($"{cache_prefix}");
        logger.LogInformation($"Banned {request.UserId}");
        logger.LogInformation($"Removed cache at {cache_prefix}");

        return Ok();
    }
    
    [HttpPost("unban")]
    public async Task<IActionResult> UnbanUser([FromBody] BanUserRequest request)
    {
        var user = await user_manager.FindByIdAsync(request.UserId);
        if (user == null) return NotFound();

        user.IsBanned = false;
        await user_manager.UpdateAsync(user);
        
        await redis_service.RemoveAllKeysAsync($"{cache_prefix}");

        logger.LogInformation($"Unbanned {request.UserId}");
        logger.LogInformation($"Removed cache at {cache_prefix}");
        return Ok();
    }

}