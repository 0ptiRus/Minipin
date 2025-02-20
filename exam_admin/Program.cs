using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using exam_frontend.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddRazorPages();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapRazorPages();

using (IServiceScope scope = app.Services.CreateScope())
{
    UserManager<ApplicationUser> user_manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    RoleManager<IdentityRole> role_manager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await role_manager.RoleExistsAsync("Admin"))
    {
        await role_manager.CreateAsync(new IdentityRole("Admin"));
    }

    string admin_email = "admin@admin.com";
    ApplicationUser? admin_user = await user_manager.FindByEmailAsync(admin_email);

    if (admin_user == null)
    {
        ApplicationUser new_admin = new() { UserName = "admin@admin.com", Email = admin_email };
        IdentityResult result = await user_manager.CreateAsync(new_admin, "Timeiswater1!"); 

        if (result.Succeeded)
        {
            await user_manager.AddToRoleAsync(new_admin, "Admin"); 
        }
    }
}

app.Run();
