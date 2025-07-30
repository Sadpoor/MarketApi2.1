using MarketApi.Data.MarketDb;
using MarketApi.Mappers;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<UserMapper>();
builder.Services.AddScoped<ProductMapper>();
builder.Services.AddScoped<DiscoutCodeMapper>();
builder.Services.AddScoped<MarketApi.Service.IServices, MarketApi.Service.Service>();
builder.Services.AddDbContext<MarketDb>(options =>
    options.UseSqlServer("Server=.;Database=MarketDb;Trusted_Connection=True;Encrypt=False"));

//// seriLog configuration and add to builder
//Log.Logger = new LoggerConfiguration()
//    .WriteTo.Console()
//    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();


//builder.Logging.AddSerilog();
////builder.Host.UseSerilog();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // حداقل سطح لاگ کلی
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // فیلتر لاگ‌های Microsoft
    .MinimumLevel.Override("System", LogEventLevel.Warning)    // فیلتر لاگ‌های System
    .WriteTo.Console()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders(); // حذف لاگ‌های پیش‌فرض
builder.Logging.AddSerilog();     // اضافه کردن Serilog


// JWT Authentication configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer",options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"])),

        ClockSkew = TimeSpan.Zero
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
