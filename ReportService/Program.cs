using CommonService.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ReportService.Consumers;
using ReportService.Data;
using ReportService.Repository;
using ReportService.Repository.Interface;
using ReportService.Services;
using ReportService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ReportDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

// Repository & Service
builder.Services.AddScoped<IRevenueReportRepository, RevenueReportRepository>();
builder.Services.AddScoped<IStockSnapshotRepository, StockSnapshotRepository>();
builder.Services.AddScoped<IReportService, ReportServiceImpl>();

// MassTransit & RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCompletedConsumer>();
    x.AddConsumer<ProductStockChangedConsumer>();

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"] ?? "localhost",
            ushort.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5600"),
            "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });

        cfg.ConfigureEndpoints(context);
    });
});

// Global Exception Handler + 404 Fallback (tập trung trong CommonService)
builder.Services.AddCommonApiServices();

builder.Services.AddCommonJwtAuthentication(builder.Configuration);

// Controllers + FormatResponseFilter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<CommonService.Filters.FormatResponseFilter>();
    options.Filters.Add<CommonService.Filters.RequiresPermissionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ReportService API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCommonErrorHandling("ReportService");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Tự động apply Migration khi khởi chạy
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReportService.Data.ReportDbContext>();
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
}

app.Run();
