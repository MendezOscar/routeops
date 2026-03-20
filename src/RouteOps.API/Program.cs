// src/RouteOps.API/Program.cs
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;
using RouteOps.Infrastructure.Persistence;
using RouteOps.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ──────────────────────────────────────────
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<RouteOpsDbContext>(opt =>
    opt.UseNpgsql(conn, npg =>
        npg.MigrationsAssembly("RouteOps.Infrastructure")));

builder.Services.AddScoped<IRouteOpsDbContext>(sp =>
    sp.GetRequiredService<RouteOpsDbContext>());

// ── MediatR (CQRS) ────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(RouteOps.Application.Commands.Orders.CreateOrderCommand).Assembly));

// ── FluentValidation ──────────────────────────────────────
builder.Services.AddValidatorsFromAssembly(
    typeof(RouteOps.Application.Commands.Orders.CreateOrderCommand).Assembly);

// ── Servicios de dominio ──────────────────────────────────
builder.Services.AddScoped<IRouteOptimizer, RouteOptimizer2Opt>();
builder.Services.AddScoped<INotificationService, WhatsAppNotificationService>();

// ── Hangfire (jobs nocturnos) ─────────────────────────────
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(conn)));

builder.Services.AddHangfireServer();

// ── JWT Auth ──────────────────────────────────────────────
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", opt =>
    {
        opt.Authority = builder.Configuration["Auth:Authority"];
        opt.Audience  = builder.Configuration["Auth:Audience"];
    });

builder.Services.AddAuthorization();

// ── CORS (para Flutter web en desarrollo) ─────────────────
builder.Services.AddCors(opt => opt.AddPolicy("dev", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("dev");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Hangfire dashboard ─────────────────────────────────────
app.UseHangfireDashboard("/jobs");

// ── Registrar jobs recurrentes ─────────────────────────────
RecurringJob.AddOrUpdate<CreditCheckJob>(
    recurringJobId: "credit-check-nightly",
    methodCall: job => job.RunAsync(),
    cronExpression: Cron.Daily(hour: 8));  // 8 AM UTC

// ── Aplicar migraciones al iniciar ────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbCtx = scope.ServiceProvider.GetRequiredService<RouteOpsDbContext>();
    await dbCtx.Database.MigrateAsync();
}

app.Run();

// ─────────────────────────────────────────────────────────
// src/RouteOps.API/Middleware/ExceptionHandlingMiddleware.cs
// ─────────────────────────────────────────────────────────
namespace RouteOps.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode = 422;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Error interno del servidor." });
        }
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.API/Controllers/OrdersController.cs
// ─────────────────────────────────────────────────────────
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteOps.Application.Commands.Orders;

namespace RouteOps.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IMediator mediator) : ControllerBase
{
    // POST /api/orders
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // PATCH /api/orders/{id}/reception
    [HttpPatch("{id:guid}/reception")]
    public async Task<IActionResult> SendToReception(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SendToReceptionCommand(id), ct);
        return NoContent();
    }

    // PATCH /api/orders/{id}/approve
    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id, [FromBody] ApproveOrderRequest req, CancellationToken ct)
    {
        await mediator.Send(new ApproveOrderCommand(id, req.DriverId, req.PayMethod), ct);
        return NoContent();
    }

    // PATCH /api/orders/{id}/reject
    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id, [FromBody] RejectOrderRequest req, CancellationToken ct)
    {
        await mediator.Send(new RejectOrderCommand(id, req.Reason), ct);
        return NoContent();
    }

    // PATCH /api/orders/{id}/delivered
    [HttpPatch("{id:guid}/delivered")]
    public async Task<IActionResult> MarkDelivered(Guid id, CancellationToken ct)
    {
        await mediator.Send(new MarkDeliveredCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

public record ApproveOrderRequest(Guid DriverId, PayMethod PayMethod);
public record RejectOrderRequest(string Reason);

// ─────────────────────────────────────────────────────────
// src/RouteOps.API/Controllers/CreditsController.cs
// ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/credits")]
[Authorize]
public class CreditsController(IMediator mediator) : ControllerBase
{
    // GET /api/credits/upcoming-dues?daysAhead=7
    [HttpGet("upcoming-dues")]
    public async Task<IActionResult> UpcomingDues(
        [FromQuery] int daysAhead = 7, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetUpcomingDuesQuery(daysAhead), ct);
        return Ok(result);
    }

    // POST /api/credits/{id}/payments
    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> ApplyPayment(
        Guid id, [FromBody] ApplyPaymentCommand cmd, CancellationToken ct)
    {
        var paymentId = await mediator.Send(cmd with { CreditId = id }, ct);
        return Ok(new { paymentId });
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.API/Controllers/RoutesController.cs
// ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/routes")]
[Authorize]
public class RoutesController(IMediator mediator) : ControllerBase
{
    // GET /api/routes?optimize=true
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] bool optimize = false, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRoutesByDriverQuery(optimize), ct);
        return Ok(result);
    }
}

// ─────────────────────────────────────────────────────────
// src/RouteOps.API/Controllers/DashboardController.cs
// ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await mediator.Send(new GetDashboardQuery(), ct);
        return Ok(result);
    }
}
