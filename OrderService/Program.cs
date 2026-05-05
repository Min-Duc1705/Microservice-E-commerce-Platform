using CommonService.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Repository;
using OrderService.Repository.Interface;
using OrderService.Services;
using OrderService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderServiceImpl>();

// 3. Cấu hình MassTransit & RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<OrderService.Consumers.OrderUserRegisteredConsumer>();
    x.AddConsumer<OrderService.Consumers.OrderUserEmailUpdatedConsumer>();
    x.AddConsumer<OrderService.Consumers.OrderPaymentSucceededConsumer>();

    // ── Transactional Outbox Pattern ──────────────────────────────────────────
    // Event được lưu vào DB cùng 1 Transaction với Order → không bao giờ mất Event
    // Background Worker tự động chuyển Event từ DB lên RabbitMQ sau đó
    x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

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

builder.Services.AddControllers(options =>
{
    // Tự động bọc mọi response vào ApiResponse<T> chuẩn
    options.Filters.Add<CommonService.Filters.FormatResponseFilter>();
    options.Filters.Add<CommonService.Filters.RequiresPermissionFilter>();
})
.AddJsonOptions(options =>
{
    // Cho phép enum serialization/deserialization bằng tên string ("New", "Processing"...)
    // thay vì integer (0, 1, 2...) mặc định của System.Text.Json
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()
    );
});

builder.Services.AddCommonJwtAuthentication(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCommonErrorHandling("OrderService");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Tự động apply Migration khi khởi chạy (Dev only hoặc dùng thận trọng trong Prod)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
}

app.Run();
