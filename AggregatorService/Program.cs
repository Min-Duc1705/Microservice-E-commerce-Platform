using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AggregatorService.Services;
using AggregatorService.Services.Interfaces;
using System.Text;
using CommonService.Extensions;
using CommonService.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddScoped<IProfileAggregatorService, ProfileAggregatorService>();
builder.Services.AddScoped<IProductPageAggregatorService, ProductPageAggregatorService>();
builder.Services.AddScoped<IAccountPageAggregatorService, AccountPageAggregatorService>();
builder.Services.AddScoped<IRolePageAggregatorService, RolePageAggregatorService>();

// ── Redis Cache (dùng ICacheService trong RequiresPermissionFilter) ────────────
// Instance name phải khớp với AuthService để đọc đúng key "perm:{email}"
builder.Services.AddCommonRedisCache(builder.Configuration, "MicroserviceShop_Auth_");

// ── Controllers + Filters ─────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<RequiresPermissionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSetting:securityKey"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

