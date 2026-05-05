namespace CommonService.Models;

/// <summary>
/// Cấu hình kết nối MinIO — đọc từ section "MinioSettings" trong appsettings.json
/// </summary>
public class MinioSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Tên bucket mặc định (ví dụ: "shop-media")</summary>
    public string BucketName { get; set; } = "shop-media";

    public bool UseSSL { get; set; } = false;
}
