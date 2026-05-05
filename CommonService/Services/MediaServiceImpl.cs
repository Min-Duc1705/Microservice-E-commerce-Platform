using CommonService.Interface;
using CommonService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace CommonService.Services;

/// <summary>
/// Implementation của IMediaService — upload/commit/xóa file lên MinIO.
///
/// Chiến lược dọn rác (Orphan Cleanup):
///   1. Mọi file upload đều vào thư mục "temp/" trước.
///   2. MinIO Lifecycle Policy tự xóa "temp/" sau 24h.
///   3. Khi lưu entity vào DB (Product/Customer), gọi CommitFileAsync()
///      để copy file từ temp/ sang folder thật (products/ hoặc customers/).
///   => Ảnh rác (user upload rồi không bấm submit) bị MinIO tự dọn sau 24h.
/// </summary>
public class MediaServiceImpl : IMediaService
{
    private const string TempFolder = "temp";

    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly ILogger<MediaServiceImpl> _logger;

    public MediaServiceImpl(
        IMinioClient minioClient,
        IOptions<MinioSettings> settings,
        ILogger<MediaServiceImpl> logger)
    {
        _minioClient = minioClient;
        _settings = settings.Value;
        _logger = logger;
    }

    // =====================================================================
    // Upload vào temp/
    // =====================================================================

    /// <summary>
    /// Upload file vào thư mục "temp/" — sẽ bị MinIO Lifecycle tự xóa sau 24h nếu không commit.
    /// Tham số <paramref name="folder"/> bị bỏ qua — luôn upload vào temp/.
    /// </summary>
    public async Task<string> UploadFileAsync(IFormFile file, string? folder = null)
    {
        await EnsureBucketExistsAsync(_settings.BucketName);

        // Luôn lưu vào temp/ bất kể caller truyền folder gì
        var objectName = BuildObjectName(file.FileName, TempFolder);

        using var stream = file.OpenReadStream();

        var putArgs = new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(file.ContentType);

        await _minioClient.PutObjectAsync(putArgs);
        _logger.LogDebug("[Media] Uploaded temp file: {ObjectName}", objectName);

        return BuildPublicUrl(objectName);
    }

    /// <summary>Upload nhiều file vào temp/ cùng lúc.</summary>
    public async Task<List<string>> UploadMultipleAsync(IList<IFormFile> files, string? folder = null)
    {
        var tasks = files.Select(f => UploadFileAsync(f, folder));
        var urls = await Task.WhenAll(tasks);
        return [.. urls];
    }

    // =====================================================================
    // Commit: temp/ → folder thật
    // =====================================================================

    /// <summary>
    /// Copy file từ temp/ sang <paramref name="targetFolder"/>, xóa bản temp.
    /// Trả về URL mới để lưu vào DB.
    ///
    /// Nếu URL không phải temp (ví dụ: ảnh cũ đã commit) → trả về nguyên URL để
    /// hỗ trợ trường hợp Update mà user không đổi ảnh.
    /// </summary>
    public async Task<string> CommitFileAsync(string tempUrl, string targetFolder)
    {
        if (string.IsNullOrWhiteSpace(tempUrl)) return tempUrl;

        var tempKey = ExtractObjectKey(tempUrl);

        // Nếu không phải file temp → trả về nguyên (ảnh cũ, không cần commit)
        if (!tempKey.StartsWith($"{TempFolder}/", StringComparison.OrdinalIgnoreCase))
            return tempUrl;

        // Tạo tên mới ở folder thật (giữ nguyên extension/guid từ temp)
        var fileName = Path.GetFileName(tempKey);
        var targetKey = $"{targetFolder.Trim('/')}/{fileName}";

        // Copy temp → target (MinIO copy trong cùng bucket, không tốn bandwidth)
        var copyArgs = new CopyObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(targetKey)
            .WithCopyObjectSource(new CopySourceObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(tempKey));

        await _minioClient.CopyObjectAsync(copyArgs);

        // Xóa file temp ngay sau khi copy thành công
        await _minioClient.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(tempKey));

        _logger.LogInformation("[Media] Committed: {TempKey} → {TargetKey}", tempKey, targetKey);

        return BuildPublicUrl(targetKey);
    }

    /// <summary>Commit nhiều file temp cùng lúc (dùng cho imageUrls).</summary>
    public async Task<List<string>> CommitMultipleAsync(IEnumerable<string> tempUrls, string targetFolder)
    {
        var tasks = tempUrls.Select(url => CommitFileAsync(url, targetFolder));
        var results = await Task.WhenAll(tasks);
        return [.. results];
    }

    // =====================================================================
    // Xóa file theo URL
    // =====================================================================

    public async Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return;

        var objectName = ExtractObjectKey(fileUrl);
        if (string.IsNullOrEmpty(objectName)) return;

        await _minioClient.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName));

        _logger.LogDebug("[Media] Deleted: {ObjectName}", objectName);
    }

    // =====================================================================
    // HELPERS
    // =====================================================================

    /// <summary>Tạo tên object duy nhất: {folder}/{guid}{ext}</summary>
    private static string BuildObjectName(string originalFileName, string folder)
    {
        var ext = Path.GetExtension(originalFileName);
        var safeName = $"{Guid.NewGuid():N}{ext}";
        return $"{folder.Trim('/')}/{safeName}";
    }

    /// <summary>Tạo URL public trỏ vào MinIO: http(s)://{endpoint}/{bucket}/{objectKey}</summary>
    private string BuildPublicUrl(string objectKey)
    {
        var scheme = _settings.UseSSL ? "https" : "http";
        return $"{scheme}://{_settings.Endpoint}/{_settings.BucketName}/{objectKey}";
    }

    /// <summary>Lấy object key (path trên MinIO) từ public URL.</summary>
    private string ExtractObjectKey(string fileUrl)
    {
        var prefix = $"{(_settings.UseSSL ? "https" : "http")}://{_settings.Endpoint}/{_settings.BucketName}/";
        return fileUrl.StartsWith(prefix) ? fileUrl[prefix.Length..] : string.Empty;
    }

    /// <summary>
    /// Kiểm tra bucket tồn tại, nếu chưa thì tạo mới, set public-read policy và
    /// cài MinIO Lifecycle tự xóa thư mục temp/ sau 24h.
    /// </summary>
    private async Task EnsureBucketExistsAsync(string bucketName)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
        bool found = await _minioClient.BucketExistsAsync(existsArgs);

        if (!found)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));

            string policyJson = $@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [{{
                    ""Action"": [""s3:GetObject""],
                    ""Effect"": ""Allow"",
                    ""Principal"": {{""AWS"": [""*""]}},
                    ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                }}]
            }}";

            await _minioClient.SetPolicyAsync(
                new SetPolicyArgs().WithBucket(bucketName).WithPolicy(policyJson));

            _logger.LogInformation("[Media] Created bucket '{BucketName}' with public-read policy.", bucketName);
        }

        // Cài Lifecycle xóa temp/ sau 24h (idempotent — gọi nhiều lần cũng được)
        await SetTempLifecyclePolicyAsync(bucketName);
    }

    /// <summary>
    /// Cài MinIO Lifecycle Policy: tự xóa tất cả objects trong thư mục "temp/" sau 1 ngày.
    /// Sử dụng Object Model của Minio SDK thay vì XML string.
    /// </summary>
    private async Task SetTempLifecyclePolicyAsync(string bucketName)
    {
        try
        {
            var rule = new Minio.DataModel.ILM.LifecycleRule
            {
                ID = "delete-temp-after-24h",
                Status = "Enabled",
                Filter = new Minio.DataModel.ILM.RuleFilter { Prefix = "temp/" },
                Expiration = new Minio.DataModel.ILM.Expiration { Days = 1 }
            };

            var rules = new List<Minio.DataModel.ILM.LifecycleRule> { rule };
            var config = new Minio.DataModel.ILM.LifecycleConfiguration(rules);

            var args = new SetBucketLifecycleArgs()
                .WithBucket(bucketName)
                .WithLifecycleConfiguration(config);

            await _minioClient.SetBucketLifecycleAsync(args);
            _logger.LogDebug("[Media] Lifecycle policy set: temp/ expires after 1 day in bucket '{Bucket}'.", bucketName);
        }
        catch (Exception ex)
        {
            // Không throw — lifecycle chỉ là tối ưu, không ảnh hưởng luồng chính
            _logger.LogWarning(ex, "[Media] Could not set lifecycle policy on bucket '{Bucket}'. Temp files won't auto-expire.", bucketName);
        }
    }
}
