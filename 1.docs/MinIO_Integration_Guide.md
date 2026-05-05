# Hướng dẫn tích hợp MinIO (Lưu trữ Video, Ảnh)

MinIO là một hệ thống object storage mã nguồn mở, tương thích hoàn toàn với API của Amazon S3. Đây là giải pháp lý tưởng để lưu trữ các file tĩnh như hình ảnh, video cho hệ thống Microservices.

## 1. Cài đặt MinIO bằng Docker

Thêm cấu hình sau vào file `docker-compose.yml` của hệ thống để khởi chạy MinIO cùng với dự án của bạn:

```yaml
version: "3.8"
services:
  minio:
    image: minio/minio:latest
    container_name: shop_minio
    environment:
      MINIO_ROOT_USER: minioadmin
      MINIO_ROOT_PASSWORD: minioadmin
    ports:
      - "9000:9000" # API Port
      - "9001:9001" # Web UI Console Port
    volumes:
      - minio_data:/data
    command: server /data --console-address ":9001"

volumes:
  minio_data:
```

_Truy cập Web UI tại: `http://localhost:9001` (User/Pass: `minioadmin` / `minioadmin`)_

## 2. Tích hợp Backend (.NET)

### Bước 2.1: Cài đặt NuGet Packages

Cài đặt thư viện Minio SDK cho service cần gọi đến MinIO (ví dụ: `ProductService` hoặc một `MediaService` riêng).

```bash
dotnet add package Minio
```

### Bước 2.2: Cấu hình `appsettings.json`

```json
{
  "MinioSettings": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "BucketName": "shop-media",
    "UseSSL": false
  }
}
```

### Bước 2.3: Đăng ký DI trong `Program.cs`

```csharp
using Minio;

builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(builder.Configuration["MinioSettings:Endpoint"])
    .WithCredentials(
        builder.Configuration["MinioSettings:AccessKey"],
        builder.Configuration["MinioSettings:SecretKey"])
    .WithSSL(builder.Configuration.GetValue<bool>("MinioSettings:UseSSL"))
    .Build());
```

### Bước 2.4: Viết Service Upload File

```csharp
using Minio;
using Minio.DataModel.Args;

public interface IMediaService
{
    Task<string> UploadFileAsync(IFormFile file);
}

public class MediaService : IMediaService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName = "shop-media";

    public MediaService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        // 1. Kiểm tra bucket tồn tại chưa, nếu chưa thì tạo mới
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
        var found = await _minioClient.BucketExistsAsync(bucketExistsArgs);
        if (!found)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs);

            // Set bucket policy public read nếu cần hiển thị ảnh trực tiếp
            string policyJson = $@"{{""Version"":""2012-10-17"",""Statement"":[{{""Action"":[""s3:GetObject""],""Effect"":""Allow"",""Principal"":{""AWS"":[""*""]},""Resource"":[""arn:aws:s3:::{_bucketName}/*""]}}]}}";
            await _minioClient.SetPolicyAsync(new SetPolicyArgs().WithBucket(_bucketName).WithPolicy(policyJson));
        }

        // 2. Upload file
        var objectName = $"{Guid.NewGuid()}_{file.FileName}";
        using var stream = file.OpenReadStream();

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(file.ContentType);

        await _minioClient.PutObjectAsync(putObjectArgs);

        return $"http://localhost:9000/{_bucketName}/{objectName}";
    }
}
```

### Bước 2.5: Viết Controller xử lý Upload

```csharp
[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");

        var fileUrl = await _mediaService.UploadFileAsync(file);
        return Ok(new { Url = fileUrl });
    }
}
```

## 3. Tích hợp Frontend (Angular)

### HTML Template (`upload.component.html`)

```html
<div class="upload-container">
  <input
    type="file"
    (change)="onFileSelected($event)"
    accept="image/*,video/*"
  />
  <button (click)="onUpload()" [disabled]="!selectedFile">Upload</button>

  <div *ngIf="uploadedUrl">
    <p>Upload thành công!</p>
    <img [src]="uploadedUrl" alt="Uploaded Image" style="max-width: 300px;" />
  </div>
</div>
```

### Typescript (`upload.component.ts`)

```typescript
import { Component } from "@angular/core";
import { HttpClient } from "@angular/common/http";

@Component({
  selector: "app-upload",
  templateUrl: "./upload.component.html",
})
export class UploadComponent {
  selectedFile: File | null = null;
  uploadedUrl: string | null = null;

  constructor(private http: HttpClient) {}

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
  }

  onUpload(): void {
    if (!this.selectedFile) return;

    const formData = new FormData();
    formData.append("file", this.selectedFile);

    // Điền URL backend Gateway hoặc Service tương ứng
    this.http
      .post<{
        url: string;
      }>("https://localhost:5001/api/media/upload", formData)
      .subscribe({
        next: (response) => {
          this.uploadedUrl = response.url;
        },
        error: (err) => {
          console.error("Lỗi khi upload:", err);
        },
      });
  }
}
```

## 4. Lưu ý khi thực tế triển khai (Production)

- Đổi thông tin `Endpoint`, `AccessKey`, `SecretKey`.
- Không lưu credentials trong client app (Angular) mà luôn uỷ quyền qua Backend.
- Sử dụng Pre-Signed URL nếu video/ảnh mang tính riêng tư.
- Mount volume bền vững khi sử dụng Docker để tránh mất dữ liệu khi restart container.
