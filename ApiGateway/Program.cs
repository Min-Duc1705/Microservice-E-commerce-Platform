using ApiGateway.Config;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Load file ocelot.json
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2. JWT — Config/JwtConfiguration.cs
builder.Services.AddAppJwt(builder.Configuration);

// 3. CORS — Config/CorsConfiguration.cs
builder.Services.AddAppCors();

// 4. Các dịch vụ cho BFF
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// 5. Ocelot
builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors(CorsConfiguration.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

// Bọc response 401/403 rỗng từ Ocelot thành JSON ApiResponse
app.UseMiddleware<OcelotErrorResponseMiddleware>();

// Map các Controller riêng của Gateway (như ProfileController)
app.MapControllers();

// PHẢI đặt cuối cùng
await app.UseOcelot();

app.Run();

