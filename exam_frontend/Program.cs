using System.Text;
using exam_frontend;
using exam_frontend.Middlewares;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlite(config.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApiDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<FollowService>();
builder.Services.AddScoped<GalleryService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<likes>();


builder.Services.AddControllers();

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

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();

app.Run();
