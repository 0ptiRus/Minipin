using System;
using System.Text;
using exam_api.Entities;
using exam_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Development.json"); // ✅ Load it into builder.Configuration

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddCors();
builder.Services.AddControllers();

// builder.Services.AddIdentityCore<ApplicationUser>()
//     .AddEntityFrameworkStores<AppDbContext>()
//     .AddDefaultTokenProviders()
//     .AddRoles<IdentityRole>()
//     .AddRoleManager<RoleManager<IdentityRole>>() // Добавьте эту строку
//     .AddRoleStore<RoleStore<IdentityRole, AppDbContext, string>>(); // Укажите тип ключа (например, string)

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));   
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionString")));
}

// builder.Services.AddDistributedMemoryCache();
// // API (if needed)
// builder.Services.AddSession(options =>
// {
//     options.Cookie.Name = "api.session";
//     options.Cookie.SameSite = SameSiteMode.None;
//     options.Cookie.HttpOnly = true;
//     options.Cookie.IsEssential = true;
// });


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // options.Events.OnAuthenticationFailed = context =>
        // {
        //     context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //     return Task.CompletedTask;
        // };
        // options.Events.OnForbidden = context =>
        // {
        //     context.Response.StatusCode = StatusCodes.Status403Forbidden;
        //     return Task.CompletedTask;
        // };
        // options.Events.OnChallenge = context =>
        // {
        //     context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //     return Task.CompletedTask;
        // };
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
            ValidateLifetime = true,
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpLogging(o => { });

builder.Services.AddLogging();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<MinioService>();
builder.Services.AddSingleton<RedisService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();

app.UseRouting();
app.UseCors(builder => builder
    .WithOrigins("https://localhost:5135", "https://localhost:7113", "https://localhost:7279")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
);
app.UseHttpsRedirection();
//app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
