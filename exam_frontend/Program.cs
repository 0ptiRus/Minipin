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
using exam_frontend.Controllers;
using exam_frontend.Services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(config.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.Configure<IdentityOptions>(options =>
{
    //...
    options.SignIn.RequireConfirmedEmail = false;
    //...
});
builder.Services.AddAuthorization();

// builder.Services.AddControllers().AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
// });;
//
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
// });


builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<FollowService>();
builder.Services.AddScoped<GalleryService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<LikeService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["BaseUrl"]) });

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpClient();

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
