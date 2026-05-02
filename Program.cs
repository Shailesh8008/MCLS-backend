using MCLS.Data;
using MCLS.IServices;
using MCLS.Models;
using MCLS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(e => e.UseSqlServer(builder
    .Configuration.GetConnectionString("default")));
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGeneralService, GeneralService>();
builder.Services.AddScoped<IVoyageLogService, VoyageLogService>();

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddAuthentication(e =>
{
    e.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    e.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(e =>
{
    e.RequireHttpsMetadata = true;
    e.SaveToken = true;

    e.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = ClaimTypes.Role
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    var adminEmail = builder.Configuration["Admin:Email"];
    var adminPass = builder.Configuration["Admin:Password"];
    await DbSeeder.SeedData(service, adminEmail, adminPass);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
