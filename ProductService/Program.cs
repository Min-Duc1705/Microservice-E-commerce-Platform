using CommonService.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.Consumers;
using ProductService.Data;
using ProductService.Repository;
using ProductService.Repository.Interface;
using ProductService.Services;
using ProductService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// DATABASE — PostgreSQL + EF Core
// =========================================================
builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

// =========================================================
// REPOSITORY & SERVICE (Dependency Injection)
// =========================================================
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductService, ProductServiceImpl>();
builder.Services.AddScoped<ICategoryService, CategoryServiceImpl>();

// =========================================================
// MINIO — Upload ảnh/file (dùng IMediaService)
// =========================================================
builder.Services.AddCommonMediaService(builder.Configuration);

// =========================================================
// CACHING (Redis)
// =========================================================
builder.Services.AddCommonRedisCache(builder.Configuration, "MicroserviceShop_Product_");

// =========================================================
// MASSTRANSIT & RABBITMQ
// =========================================================
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderStatusChangedConsumer>();

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

// =========================================================
// GLOBAL EXCEPTION HANDLER (.NET 8 IExceptionHandler)
// =========================================================
builder.Services.AddCommonApiServices();

// =========================================================
// CONTROLLERS + GLOBAL RESPONSE FILTER
// =========================================================
builder.Services.AddControllers(options =>
{
    // Tự động bọc mọi response vào ApiResponse<T> chuẩn
    options.Filters.Add<CommonService.Filters.FormatResponseFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ProductService API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCommonErrorHandling("ProductService");

app.UseAuthorization();

app.MapControllers();

app.Run();
