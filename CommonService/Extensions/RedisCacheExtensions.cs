using CommonService.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonService.Extensions;

public static class RedisCacheExtensions
{
    public static IServiceCollection AddCommonRedisCache(this IServiceCollection services, IConfiguration configuration, string instanceName)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = instanceName;
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
