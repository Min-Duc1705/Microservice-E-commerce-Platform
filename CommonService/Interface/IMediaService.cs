using Microsoft.AspNetCore.Http;

namespace CommonService.Interface;

/// <summary>
/// Service upload/xóa file lên MinIO Object Storage.
/// Dùng chung cho mọi microservice (ProductService, CustomerService, ...).
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Upload 1 file lên MinIO vào thư mục TẠM (temp/).
    /// File này sẽ tự động bị xóa sau 24h bởi MinIO Lifecycle Policy nếu không được commit.
    /// </summary>
    /// <param name="file">File từ multipart/form-data</param>
    /// <param name="folder">Thư mục con đích đến (mặc định bị bỏ qua khi dùng logic temp)</param>
    /// <returns>URL public để truy cập file tạm (ví dụ: http://localhost:9000/shop-media/temp/abc.jpg)</returns>
    Task<string> UploadFileAsync(IFormFile file, string? folder = null);

    /// <summary>
    /// Upload nhiều file cùng lúc vào thư mục TẠM (temp/).
    /// </summary>
    Task<List<string>> UploadMultipleAsync(IList<IFormFile> files, string? folder = null);

    /// <summary>
    /// Xác nhận file: Copy từ thư mục temp sang thư mục thật, sau đó xóa file temp.
    /// Gọi khi lưu thông tin product/customer thành công.
    /// </summary>
    /// <param name="tempUrl">URL tạm trả về từ UploadFileAsync</param>
    /// <param name="targetFolder">Thư mục đích (ví dụ: "products", "customers")</param>
    /// <returns>URL cuối cùng sau khi commit</returns>
    Task<string> CommitFileAsync(string tempUrl, string targetFolder);

    /// <summary>
    /// Commit nhiều file cùng lúc (dành cho danh sách ảnh).
    /// </summary>
    Task<List<string>> CommitMultipleAsync(IEnumerable<string> tempUrls, string targetFolder);

    /// <summary>
    /// Xóa file khỏi MinIO theo URL đầy đủ.
    /// </summary>
    /// <param name="fileUrl">URL đầy đủ của file (ví dụ: http://localhost:9000/shop-media/products/abc.jpg)</param>
    Task DeleteFileAsync(string fileUrl);
}
