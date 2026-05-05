using CommonService.Extensions;
using MassTransit;
using System.Text;
using System.Text.Json;
using AuthService.Config;
using AuthService.Data;
using AuthService.Repository;
using AuthService.Repository.Interface;
using AuthService.Services;
using AuthService.Services.Interface;
using CommonService.Common;
using CommonService.Exceptions;
using CommonService.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));

// ── Repositories ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthServiceImpl>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IUserService, UserService>();

// ── Caching (Redis) ────────────────────────────────────────────────────────
builder.Services.AddCommonRedisCache(builder.Configuration, "MicroserviceShop_Auth_");

// ── MassTransit & RabbitMQ ───────────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    // ── Transactional Outbox Pattern ────────────────────────────────────────
    // Thay vì bắn thẳng vào RabbitMQ, Event sẽ được lưu vào bảng OutboxMessages
    // trong cùng 1 Transaction với DB update → Không bao giờ mất Event!
    x.AddEntityFrameworkOutbox<AuthDbContext>(o =>
    {
        o.UsePostgres();   // Dùng PostgreSQL
        o.UseBusOutbox();  // Kích hoạt Background Worker tự động chuyển Event lên RabbitMQ
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

// ── Exception & Formatting (tập trung trong CommonService) ──────────────────
builder.Services.AddCommonApiServices();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<FormatResponseFilter>();
    options.Filters.Add<PermissionInterceptor>();
});

// ── JWT Authentication ────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ClockSkew = TimeSpan.Zero  // Mặc định là 5 phút — phải set Zero khi test thời gian ngắn
        };

        // ── Trả ApiResponse JSON chuẩn khi JWT 401/403 ────────────────────
        options.Events = new JwtBearerEvents
        {
            // 401 — Chưa đăng nhập / Token hết hạn / Token sai
            OnChallenge = async context =>
            {
                // Chặn response mặc định của JWT middleware
                context.HandleResponse();

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var message = string.IsNullOrEmpty(context.ErrorDescription)
                    ? "Bạn chưa đăng nhập hoặc token đã hết hạn."
                    : context.ErrorDescription;

                var response = new ApiResponse<object>
                {
                    StatusCode = 401,
                    Error = "Unauthorized",
                    Message = message,
                    Data = null
                };

                await context.Response.WriteAsJsonAsync(response);
            },

            // 403 — Token hợp lệ nhưng không đủ quyền (role-based [Authorize])
            OnForbidden = async context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                var response = new ApiResponse<object>
                {
                    StatusCode = 403,
                    Error = "Forbidden",
                    Message = "Bạn không có quyền truy cập tài nguyên này.",
                    Data = null
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseCommonErrorHandling("AuthService");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
