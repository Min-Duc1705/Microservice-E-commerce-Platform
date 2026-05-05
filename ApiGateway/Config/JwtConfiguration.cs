using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ApiGateway.Config;

public static class JwtConfiguration
{
    public static IServiceCollection AddAppJwt(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!)),
                    // Mặc định 5 phút — khi test token ngắn (AccessTokenExpireSeconds < 300s)
                    // token sẽ vẫn được chấp nhận thêm 5 phút sau khi hết hạn.
                    ClockSkew = TimeSpan.Zero,
                };

                // Lưu ý: Không dùng OnChallenge/OnForbidden ở đây vì Ocelot
                // xử lý auth bằng pipeline riêng → bypass JWT events.
                // Framework sẽ ném ra 401/403 raw, sau đó OcelotErrorResponseMiddleware 
                // (đã cấu hình trong Program.cs) sẽ format lại thành ApiResponse JSON.
            });

        return services;
    }
}
