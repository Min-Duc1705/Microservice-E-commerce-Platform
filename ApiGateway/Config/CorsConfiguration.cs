namespace ApiGateway.Config;

public static class CorsConfiguration
{
    // Dùng const để tránh magic string — gõ sai sẽ báo lỗi compile ngay
    public const string PolicyName = "AllowSpecificOrigins";

    public static IServiceCollection AddAppCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, builder =>
            {
                builder
                    .WithOrigins(
                        "http://localhost:4200",  // Angular dev
                        "http://localhost:4173",  // Vite preview
                        "http://localhost:3000"   // React dev (nếu có)
                    )
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                    .WithHeaders("Authorization", "Content-Type", "Accept")
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
            });
        });

        return services;
    }
}
