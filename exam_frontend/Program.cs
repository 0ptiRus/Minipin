using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using exam_frontend;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.IdentityModel.Tokens;
using exam_frontend.Entities;
using exam_frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json")
    .Build();

// if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
// {
//     builder.Services.AddDbContext<AppDbContext>(options =>
//         options.UseSqlite(config.GetConnectionString("Default")));   
// }
// else
// {
//     builder.Services.AddDbContext<AppDbContext>(options =>
//         options.UseSqlite(Environment.GetEnvironmentVariable("ConnectionString")));
// }

// builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//     .AddEntityFrameworkStores<AppDbContext>()
//     .AddDefaultTokenProviders()
//     .AddRoles<IdentityRole>();


// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options =>
//     {
//         options.LoginPath = "/Account/Login";
//         options.LogoutPath = "/Account/Logout";
//         options.AccessDeniedPath = "/Account/AccessDenied";
//     });

// builder.Services.Configure<IdentityOptions>(options =>
// {
//     //...
//     options.SignIn.RequireConfirmedEmail = false;
//     //...
// });
//builder.Services.AddAuthorization();

// builder.Services.AddControllers().AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
// });;
//
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
// });

// builder.Services.AddDistributedMemoryCache(); // Use in-memory cache for sessions
// builder.Services.AddSession(options =>
// {
//     options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout as needed
//     options.Cookie.HttpOnly = true;
//     options.Cookie.SameSite = SameSiteMode.None;
//     options.Cookie.IsEssential = true; // Make sure the session cookie is always sent
//     options.Cookie.Name = "frontend.session";
// });

// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options =>
//     {
//         options.LoginPath = "/Account/Login"; // Указываем путь для перенаправления на форму входа
//     })
//     .AddJwtBearer(options =>
//     {
//         options.RequireHttpsMetadata = true;
//         options.SaveToken = true;
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
//             ValidateIssuer = false,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             ValidateAudience = false,
//             ValidAudience = builder.Configuration["Jwt:Audience"],
//             ValidateLifetime = true
//         };
//     });


builder.Services.AddAuthentication(options =>
    {
        // ✅ Set cookie as default for Razor Pages authorization
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
     {
         options.LoginPath = "/Account/Login"; // Указываем путь для перенаправления на форму входа
     })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = false,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = false,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // existing settings ...
            NameClaimType = "nameid", // 👈 tell ASP.NET what claim to treat as NameIdentifier
            RoleClaimType = "role"
        };

    });

builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();

builder.Services.AddSingleton<MinioService>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(100);
    client.BaseAddress = new Uri(builder.Configuration["BaseUrl"]);
});


builder.Services.AddServerSideBlazor();
builder.Services.AddRazorPages();


var app = builder.Build();

// app.UseStatusCodePages(async context =>
// {
//     var response = context.HttpContext.Response;
//
//     if (response.StatusCode == StatusCodes.Status401Unauthorized)
//     {
//         response.Redirect("/Account/Login");
//     }
// });


// app.UseMiddleware<AuthMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();



app.Run();