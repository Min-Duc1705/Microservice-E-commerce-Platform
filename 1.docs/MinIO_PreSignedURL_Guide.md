# Hướng dẫn Pre-Signed URL với MinIO

## Pre-Signed URL là gì?

**Pre-Signed URL** là một URL tạm thời có chứa **chữ ký điện tử (signature)** được tạo bởi server. URL này cho phép client truy cập vào một file **private** trên MinIO trong một khoảng thời gian nhất định, sau đó tự động hết hạn.

> Hãy hình dung như một **vé vào cửa có thời hạn**: Server cấp vé, client dùng vé đó vào thẳng MinIO lấy file mà không cần hỏi backend thêm lần nào.

---

## 1. Cách hoạt động chi tiết

### Bước 1 — Client yêu cầu xem file
```
Client (Angular)  ──────────────────▶  Backend
                   GET /api/v1/media/presign?fileUrl=...
```

### Bước 2 — Backend tạo Pre-Signed URL từ MinIO SDK
```
Backend  ──────────────────────────▶  MinIO
          "Tạo URL tạm thời cho file X, hết hạn sau 1 giờ"
          ◀──────────────────────────
          "URL đã ký: http://localhost:9000/shop-media/...?X-Amz-Signature=abc123..."
```

### Bước 3 — Backend trả URL về Client
```
Backend  ──────────────────────────▶  Client (Angular)
          { "url": "http://localhost:9000/shop-media/xyz.jpg?X-Amz-Signature=abc&X-Amz-Expires=3600" }
```

### Bước 4 — Client truy cập file trực tiếp từ MinIO qua URL tạm thời
```
Client (Angular)  ──────────────────▶  MinIO
                   GET http://localhost:9000/shop-media/xyz.jpg?X-Amz-Signature=abc...
                   ◀──────────────────
                   File (ảnh/video)
```

### Toàn bộ luồng:
```
Client                     Backend                    MinIO
  │                           │                          │
  │─── 1. Yêu cầu presign ──▶│                          │
  │       ?fileUrl=xyz.jpg    │                          │
  │                           │── 2. Ký URL (SDK) ─────▶│
  │                           │◀── 3. Signed URL ───────│
  │◀── 4. Trả Signed URL ────│                          │
  │                           │                          │
  │─── 5. GET Signed URL ──────────────────────────────▶│
  │◀── 6. File (ảnh/video) ────────────────────────────│
  │                           │                          │
  │ [Sau 1 giờ: URL hết hạn]  │                          │
  │─── 7. GET Signed URL (cũ) ─────────────────────────▶│
  │◀── 403 Forbidden ──────────────────────────────────│
```

---

## 2. Cấu trúc của một Pre-Signed URL

```
http://localhost:9000/shop-media/products/abc.jpg
  ?X-Amz-Algorithm=AWS4-HMAC-SHA256        ← Thuật toán ký
  &X-Amz-Credential=minioadmin/...         ← Thông tin người ký (accessKey)
  &X-Amz-Date=20260316T010000Z             ← Thời điểm ký
  &X-Amz-Expires=3600                      ← Hết hạn sau 3600 giây (1 giờ)
  &X-Amz-SignedHeaders=host                ← Headers đưa vào chữ ký
  &X-Amz-Signature=a1b2c3d4e5f6...         ← Chữ ký điện tử (HMAC-SHA256)
```

MinIO sẽ **verify chữ ký** khi nhận request:
- Nếu chữ ký hợp lệ + chưa hết hạn → ✅ Trả file
- Nếu hết hạn → ❌ `403 Forbidden`
- Nếu chữ ký bị giả mạo → ❌ `403 Forbidden`

---

## 3. So sánh Public URL vs Pre-Signed URL

| | Public URL | Pre-Signed URL |
|---|---|---|
| **Bucket policy** | public-read | private (không ai xem được nếu không có URL ký) |
| **URL dùng mãi được không?** | ✅ Vĩnh viễn | ❌ Hết hạn theo thời gian đặt |
| **Client tải thẳng từ MinIO?** | ✅ Có | ✅ Có (không qua backend) |
| **Có thể share URL?** | ✅ Share thoải mái | ⚠️ Share được nhưng sẽ hết hạn |
| **Bảo mật** | ❌ Thấp | ✅ Cao |
| **Phù hợp với** | Ảnh sản phẩm công khai | Ảnh cá nhân, video, tài liệu nội bộ |

---

## 4. Khi nào nên dùng?

| Content | Nên dùng |
|---|---|
| Ảnh sản phẩm (thumbnail, gallery) | Public URL (ai cũng được xem) |
| Avatar khách hàng | Pre-Signed URL |
| Video hướng dẫn nội bộ | Pre-Signed URL |
| Hóa đơn, tài liệu PDF | Pre-Signed URL hoặc Stream qua Backend |
| Ảnh CCCD/căn cước | Stream qua Backend (bảo mật tuyệt đối) |

---

## 5. Cách implement trong dự án này

### Bước 5.1 — Đổi bucket sang private

Trong `MediaServiceImpl.cs`, xóa policy public-read và **không** set policy khi tạo bucket:

```csharp
// Tạo bucket private (không set policy public-read)
if (!found)
{
    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
    // ← Không gọi SetPolicyAsync → bucket mặc định là private
}
```

### Bước 5.2 — Thêm method GeneratePresignedUrlAsync vào IMediaService

```csharp
public interface IMediaService
{
    Task<string> UploadFileAsync(IFormFile file, string? folder = null);
    Task<List<string>> UploadMultipleAsync(IList<IFormFile> files, string? folder = null);
    Task DeleteFileAsync(string fileUrl);

    // Tạo URL tạm thời để client tải file private
    Task<string> GetPresignedUrlAsync(string fileUrl, int expirySeconds = 3600);
}
```

### Bước 5.3 — Implement trong MediaServiceImpl.cs

```csharp
public async Task<string> GetPresignedUrlAsync(string fileUrl, int expirySeconds = 3600)
{
    // Tách object name từ URL đầy đủ
    var prefix = $"{(_settings.UseSSL ? "https" : "http")}://{_settings.Endpoint}/{_settings.BucketName}/";
    var objectName = fileUrl[prefix.Length..];

    var args = new PresignedGetObjectArgs()
        .WithBucket(_settings.BucketName)
        .WithObject(objectName)
        .WithExpiry(expirySeconds);

    return await _minioClient.PresignedGetObjectAsync(args);
}
```

### Bước 5.4 — Thêm endpoint vào MediaController (ProductService)

```csharp
// GET /api/v1/media/presign?fileUrl=http://localhost:9000/shop-media/products/abc.jpg
[HttpGet("presign")]
[ApiMessage("Lấy URL tạm thời thành công")]
public async Task<IActionResult> GetPresignedUrl([FromQuery] string fileUrl, [FromQuery] int expirySeconds = 3600)
{
    if (string.IsNullOrWhiteSpace(fileUrl))
        return BadRequest("fileUrl không được để trống.");

    var signedUrl = await _mediaService.GetPresignedUrlAsync(fileUrl, expirySeconds);
    return Ok(new { url = signedUrl, expiresInSeconds = expirySeconds });
}
```

### Bước 5.5 — Sử dụng ở Angular

```typescript
// Khi cần hiển thị ảnh private:
getImageUrl(fileUrl: string): Observable<string> {
  return this.http
    .get<{ url: string }>(`/api/v1/media/presign?fileUrl=${encodeURIComponent(fileUrl)}`)
    .pipe(map(res => res.url));
}
```

```html
<!-- Template -->
<img [src]="presignedUrl" alt="Avatar khách hàng" />
```

---

## 6. Thời hạn nên đặt bao nhiêu?

| Trường hợp | Thời hạn gợi ý |
|---|---|
| Hiển thị ảnh trong UI | 1–2 giờ (3600–7200s) |
| Download file lớn (video) | 15–30 phút (900–1800s) |
| Link chia sẻ tạm thời | 24 giờ (86400s) |
| Tối đa MinIO hỗ trợ | 7 ngày (604800s) |

---

## 7. Lưu ý quan trọng

- **Không lưu Pre-Signed URL vào DB** — URL thay đổi mỗi lần generate, chỉ lưu **object name** hoặc **URL gốc** (không ký).
- **Cache Pre-Signed URL ở client** trong phạm vi thời hạn để tránh gọi API mỗi lần render ảnh.
- **Không expose MinIO port 9000 ra internet** nếu dùng bucket private — chỉ dùng nội bộ hoặc qua domain riêng với SSL.
