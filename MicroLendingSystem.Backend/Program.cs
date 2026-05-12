using System.IdentityModel.Tokens.Jwt;
using System.Text;
using MicroLendingSystem.Backend.Features.Permissions;
using MicroLendingSystem.Backend.Features.Roles;
using MicroLendingSystem.Backend.Features.Users;
using MicroLendingSystem.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using microlending_API.Features.Loans;
using microlending_API.Features.LoanSettings;
using microlending_API.Features.Borrowers;
using microlending_API.Features.Transactions;
using MicroLendingSystem.Database.AppDbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MicroLendingSystem.Backend.Authorization;
using MicroLendingSystem.Backend.Features.Dashboard;
using MicroLendingSystem.Backend.Infrastructure;
using MicroLendingSystem.Backend.Options;
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<IBorrowerService, BorrowerService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ILoanSettingService, LoanSettingService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// HasPermission resolves PermissionAuthorizationFilter instances via ActivatorUtilities (merged with Arguments + DI).

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

var jwtConfigured = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                   ?? throw new InvalidOperationException("Jwt configuration section is required.");
var signingKeyBytes = Encoding.UTF8.GetBytes(jwtConfigured.Secret);
if (signingKeyBytes.Length < 32)
{
    throw new InvalidOperationException("Jwt:Secret must be UTF-8 length >= 32 for HMAC SHA-256.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !(builder.Environment.IsDevelopment());
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
            ValidateIssuer = true,
            ValidIssuer = string.IsNullOrWhiteSpace(jwtConfigured.Issuer) ? "micro-lending" : jwtConfigured.Issuer,
            ValidateAudience = true,
            ValidAudience =
                string.IsNullOrWhiteSpace(jwtConfigured.Audience) ? "micro-lending-api" : jwtConfigured.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Micro Lending API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };

    c.AddSecurityDefinition("Bearer", scheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    context.Database.EnsureCreated();

    try
    {
        RbacPermissionBootstrap.EnsureAsync(context).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--- RBAC permission bootstrap: {ex.Message} ---");
    }

    if (!context.Users.Any(u => u.Email == "admin@gmail.com"))
    {
        var admin = new User
        {
            Name = "System Admin",
            Email = "admin@gmail.com",
            RoleId = 1,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = ""
        };

        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        context.Users.Add(admin);

        try
        {
            context.SaveChanges();
            Console.WriteLine("--- Admin User Seeded Successfully! ---");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--- Error Seeding: {ex.Message} ---");
        }
    }
}

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
