using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using RouteOps.Application.Interfaces;
using RouteOps.Infrastructure.Persistence;
using RouteOps.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<RouteOpsDbContext>(opt =>
    opt.UseNpgsql(conn, npg =>
        npg.MigrationsAssembly("RouteOps.Infrastructure")));

builder.Services.AddScoped<IRouteOpsDbContext>(sp =>
    sp.GetRequiredService<RouteOpsDbContext>());

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(RouteOps.Application.Commands.Orders.CreateOrderCommand).Assembly));

builder.Services.AddScoped<IRouteOptimizer, RouteOptimizer2Opt>();
builder.Services.AddScoped<INotificationService, WhatsAppNotificationService>();

builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(conn)));

builder.Services.AddHangfireServer();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", opt =>
    {
        opt.Authority = builder.Configuration["Auth:Authority"];
        opt.Audience  = builder.Configuration["Auth:Audience"];
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(opt => opt.AddPolicy("dev", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("dev");
}

app.UseMiddleware<RouteOps.API.Middleware.ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/jobs");

RecurringJob.AddOrUpdate<CreditCheckJob>(
    recurringJobId: "credit-check-nightly",
    methodCall: job => job.RunAsync(),
    cronExpression: Cron.Daily(hour: 8));

using (var scope = app.Services.CreateScope())
{
    var dbCtx = scope.ServiceProvider.GetRequiredService<RouteOpsDbContext>();
    await dbCtx.Database.MigrateAsync();
}

app.Run();
