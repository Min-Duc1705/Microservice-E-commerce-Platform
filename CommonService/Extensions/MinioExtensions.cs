using CommonService.Interface;
using CommonService.Models;
using CommonService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace CommonService.Extensions;

/// <summary>
/// Extension method để đăng ký MinIO + IMediaService vào DI.
/// Dùng chung cho mọi microservice cần upload file.
///
/// Cách dùng trong Program.cs của service:
///   builder.Services.AddCommonMediaService(builder.Configuration);
/// </summary>
public static class MinioExtensions
{
    public static IServiceCollection AddCommonMediaService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind config section "MinioSettings" → MinioSettings class
        services.Configure<MinioSettings>(
            configuration.GetSection("MinioSettings"));

        var settings = configuration.GetSection("MinioSettings").Get<MinioSettings>()
            ?? throw new InvalidOperationException(
                "Thiếu section 'MinioSettings' trong appsettings.json. " +
                "Vui lòng thêm Endpoint, AccessKey, SecretKey, BucketName.");

        // Đăng ký IMinioClient
        services.AddMinio(configureClient => configureClient
            .WithEndpoint(settings.Endpoint)
            .WithCredentials(settings.AccessKey, settings.SecretKey)
            .WithSSL(settings.UseSSL)
            .Build());

        // Đăng ký IMediaService
        services.AddScoped<IMediaService, MediaServiceImpl>();

        return services;
    }
}
