using CommonService.Annotations;
using CommonService.Interface;
using CommonService.Interface;
using Microsoft.AspNetCore.Mvc;
using CommonService.Filters;

namespace ProductService.Controllers;

/// <summary>
/// Upload ảnh/file lên MinIO và lấy về URL để gán vào ThumbnailUrl / ImageUrls của sản phẩm.
///
/// Luồng sử dụng:
///   1. Gọi POST /api/v1/media/upload → nhận URL
///   2. Dùng URL đó trong POST /api/v1/products (trường thumbnailUrl hoặc imageUrls)
/// </summary>
[ApiController]
[Route("api/v1/media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    // POST /api/v1/media/upload
    // Body: form-data, key = "file"
    [HttpPost("upload")]
    [RequiresPermission("POST", "/api/v1/media/upload")]
    [ApiMessage("Upload ảnh thành công")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File không được để trống.");

        // Kiểm tra định dạng file được phép
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest("Chỉ chấp nhận file ảnh (jpg, png, webp, gif).");

        // Upload vào temp/ — sẽ tự bị xóa sau 24h nếu không commit
        var tempUrl = await _mediaService.UploadFileAsync(file);
        return Ok(new { url = tempUrl });
    }

    // POST /api/v1/media/upload-multiple
    // Body: form-data, key = "files" (nhiều file)
    [HttpPost("upload-multiple")]
    [RequiresPermission("POST", "/api/v1/media/upload-multiple")]
    [ApiMessage("Upload nhiều ảnh thành công")]
    public async Task<IActionResult> UploadMultiple(IList<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("Danh sách file không được để trống.");

        if (files.Count > 10)
            return BadRequest("Chỉ được upload tối đa 10 file cùng lúc.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        foreach (var f in files)
        {
            if (!allowedTypes.Contains(f.ContentType.ToLower()))
                return BadRequest($"File '{f.FileName}' không đúng định dạng (chỉ chấp nhận jpg, png, webp, gif).");
        }

        var tempUrls = await _mediaService.UploadMultipleAsync(files);
        return Ok(new { urls = tempUrls });
    }
}
