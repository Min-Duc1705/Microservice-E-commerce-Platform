using CommonService.Extensions;
using CustomerService.Data;
using CustomerService.Repository;
using CustomerService.Repository.Interface;
using MassTransit;
using CustomerService.Services;
using CustomerService.Services.Interface;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

// Repository & Service
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerServiceImpl>();

// MinIO Media Service (IMediaService)
builder.Services.AddCommonMediaService(builder.Configuration);

// MassTransit & RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Đăng ký Consumer
    x.AddConsumer<CustomerService.Consumers.UserRegisteredConsumer>();
    x.AddConsumer<CustomerService.Consumers.UserEmailUpdatedConsumer>();
    x.AddConsumer<CustomerService.Consumers.CustomerOrderStatusChangedConsumer>();

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

// Controllers + GlobalFilter tự động bọc response vào ApiResponse<T>
builder.Services.AddControllers(options =>
{
    options.Filters.Add<CommonService.Filters.FormatResponseFilter>();
    options.Filters.Add<CommonService.Filters.RequiresPermissionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CustomerService API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCommonErrorHandling("CustomerService");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
