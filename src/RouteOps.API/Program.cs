// src/RouteOps.API/Program.cs
// Reemplaza la sección de auth existente con esta configuración JWT propia

// ── REEMPLAZA ESTO: ──────────────────────────────────────────
// builder.Services.AddAuthentication("Bearer")
//     .AddJwtBearer("Bearer", opt =>
//     {
//         opt.Authority = builder.Configuration["Auth:Authority"];
//         opt.Audience  = builder.Configuration["Auth:Audience"];
//     });

// ── POR ESTO: ────────────────────────────────────────────────
/*
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key requerido.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer   = true,
            ValidIssuer      = builder.Configuration["Jwt:Issuer"] ?? "routeops",
            ValidateAudience = true,
            ValidAudience    = builder.Configuration["Jwt:Audience"] ?? "routeops-app",
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.Zero,
        };
    });
*/

// ARCHIVO COMPLETO ACTUALIZADO:
using Hangfire;
using Hangfire.PostgreSql;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RouteOps.Application.Interfaces;
using RouteOps.Infrastructure.Persistence;
using RouteOps.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key requerido.");

// ── Base de datos ─────────────────────────────────────────────
builder.Services.AddDbContext<RouteOpsDbContext>(opt =>
    opt.UseNpgsql(conn, npg =>
        npg.MigrationsAssembly("RouteOps.Infrastructure")));

builder.Services.AddScoped<IRouteOpsDbContext>(sp =>
    sp.GetRequiredService<RouteOpsDbContext>());

// ── MediatR ───────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(RouteOps.Application.Commands.Orders.CreateOrderCommand).Assembly));

// ── Servicios ─────────────────────────────────────────────────
builder.Services.AddScoped<IRouteOptimizer, RouteOptimizer2Opt>();
builder.Services.AddScoped<INotificationService, WhatsAppNotificationService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ── Hangfire ──────────────────────────────────────────────────
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(conn)));

builder.Services.AddHangfireServer();

// ── JWT Auth propio ───────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "routeops",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "routeops-app",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(opt => opt.AddPolicy("dev", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("dev");

app.UseMiddleware<RouteOps.API.Middleware.ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/jobs");

RecurringJob.AddOrUpdate<CreditCheckJob>(
    "credit-check-nightly",
    job => job.RunAsync(),
    Cron.Daily(hour: 8));

app.Run();
