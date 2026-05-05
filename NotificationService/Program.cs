using MassTransit;
using NotificationService.Consumers;
using NotificationService.Services;
using NotificationService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// ── Email Service ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService, EmailService>();

// ── MassTransit & RabbitMQ ────────────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Đăng ký Consumer
    x.AddConsumer<OtpRequestedConsumer>();

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

        // Tự động cấu hình queue cho tất cả consumer
        cfg.ConfigureEndpoints(context);
    });
});

// ── Swagger (tuỳ chọn, chỉ dùng để xem thông tin) ───────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

