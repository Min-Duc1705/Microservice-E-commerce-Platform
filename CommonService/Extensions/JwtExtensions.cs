using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CommonService.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddCommonJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var secretKey = config["Jwt:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    // Giá trị mặc định nếu lỡ quên cấu hình trong appsettings.json
                    secretKey = "SuperSecretKeyMinimum32CharactersLong!";
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    
                    // Gateway đã lo phần xác thực cấu trúc này, cho phép Microservice giảm tải
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
