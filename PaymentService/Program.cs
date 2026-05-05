using CommonService.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Consumers;
using PaymentService.Data;
using PaymentService.Repository;
using PaymentService.Repository.Interface;
using PaymentService.Services;
using PaymentService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

// Repository & Service
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentServiceImpl>();

// MassTransit & RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

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

// Controllers + FormatResponseFilter tự động bọc response
builder.Services.AddControllers(options =>
{
    options.Filters.Add<CommonService.Filters.FormatResponseFilter>();
    options.Filters.Add<CommonService.Filters.RequiresPermissionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PaymentService API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCommonErrorHandling("PaymentService");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Tự động apply Migration khi khởi chạy
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentService.Data.PaymentDbContext>();
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
}

app.Run();
