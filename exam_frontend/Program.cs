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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Set the login page path
        options.AccessDeniedPath = "/Account/AccessDenied"; // Optional: Redirect for unauthorized access
        options.Cookie.Name = "GalleryCookie";
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://localhost:5135")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});



builder.Services.AddHttpClient();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiService, ApiService>();

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
app.UseCors();
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
app.UseStaticFiles();


app.MapRazorPages();

app.Run();