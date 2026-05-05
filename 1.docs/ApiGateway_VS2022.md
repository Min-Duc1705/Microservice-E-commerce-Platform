# Hướng Dẫn: Tái Tạo (Recreate) API Gateway bằng Visual Studio 2022

Bài hướng dẫn này dành cho trường hợp bạn cần xóa thư mục cũ hoặc muốn tạo lại mới tinh hoàn toàn API Gateway `.NET 8` trực tiếp trên công cụ **Visual Studio 2022**. Cấu trúc của project này sẽ được giữ nguyên theo chuẩn có sẵn (đã cấu hình sẵn thư mục `Config/` và dùng `ocelot.json`).

---

## Bước 1: Khởi tạo Project trong Visual Studio

1. Mở phần mềm **Visual Studio 2022**.
2. Click **Create a new project**.
3. Tại ô tìm kiếm, gõ từ khóa `ASP.NET Core Empty` và chọn mẫu này (Mẫu *Empty* tối ưu nhất vì nó không phát sinh các file controller dư thừa, làm project nhẹ như `dotnet new web`). Nhấn **Next**. 
4. Cấu hình chi tiết Project: 
   - **Project Name**: `ApiGateway`
   - **Location**: Trỏ đến đường dẫn `t:\TryHard_IT_Project\Intern\MicroserviceShop\`.
     > 💡 *Lưu ý: Nếu thư mục ApiGateway cũ vẫn còn, bạn cần đổi tên thành `ApiGateway_Old` để Visual Studio không báo lỗi trùng thư mục chưa xóa.*
5. Chọn Framework là **.NET 8.0 (Long Term Support)** (hoặc version ứng với file config cũ).
6. Bỏ chọn ô **Configure for HTTPS** và **Do not use top-level statements**. Cấu hình này giúp cho file `Program.cs` cực kỳ sạch sẽ và đơn giản.
7. Nhấn **Create**.

---

## Bước 2: Cài đặt các thư viện lõi (NuGet Packages)

1. Click chuột phải lên chữ **Dependencies** (bên trong project `ApiGateway` tại màn hình góc phải *Solution Explorer*).
2. Chọn dòng **Manage NuGet Packages...**
3. Chuyển sang thẻ **Browse** và cài đặt **lần lượt** 2 gói cài đặt chính sau đây:
   - `Ocelot` (Phiên bản mới nhất nhưng phải hỗ trợ từ .NET 8, ví dụ `23.4.2` là các phiên bản mới) - **Khung cấu hình Routing**.
   - `Microsoft.AspNetCore.Authentication.JwtBearer` - **Xác thực Authentication ở phía Gateway**.
4. Nhấn **Install**, sau đó nhấn **I Accept** ở cửa sổ điều khoản để tiến hành tải thư viện.

---

## Bước 3: Phục hồi cấu hình Routing Ocelot.json

1. Tạo file cấu hình cho Ocelot:
   - Sau khi các thư viện tải xong, chuột phải phần vùng trống của `ApiGateway` trong Solution Explorer > Chọn **Add** > **New Item...**
   - Click nhánh định dạng **Web** (hoặc search `json`), chọn mục **JSON File**. Đặt tên bắt buộc là `ocelot.json`.
   - Copy nội dung nội quy (*routes list và up/downstrem path*) từ file `ocelot.json` cũ dán sang.
2. 🚨 **[CỰC KỲ QUAN TRỌNG - THƯỜNG HAY BỊ MISS LỖI NÀY]**: 
   - Tiếp tục click **chuột phải vào file `ocelot.json`** vừa tạo > Chọn cuối phần dropdown là **Properties**.
   - Quan sát ô bên dưới sẽ thấy property tên là **Copy to Output Directory**. Đổi nó từ `Do not copy` biến thành **`Copy if newer`** (hoặc `Copy always`). 
   - *Việc này lệnh cho compiler lấy file này đẩy xuống khi app chạy, nếu không thì Ocelot ko đọc được Routes.*

---

## Bước 4: Chuyển dữ liệu Cấu hình Helper

API Gateway này có cấu trúc tương đối đẹp vì đã tách biệt `Jwt`, `Cors`, `Error Middleware` vào trong một thư mục là **Config**. Làm lại các bước tay:

1. Thêm folder: Click chuột phải project `ApiGateway` > **Add** > **New Folder**, đổi tên thư mục mới thành `Config`.
2. Copy các file code trước đó (Từ Explorer máy bạn): `CorsConfiguration.cs`, `JwtConfiguration.cs`, và đoạn `OcelotErrorResponseMiddleware.cs` bỏ vào thư mục `Config` này.
3. Thay đổi nội dung `appsettings.json` cũ dán đè file của project mới cho chứa thông tin chuỗi Token, Base Logging:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": { ... các thông tin Issuer/Audience và SecretKey ...}
}
```

---

## Bước 5: Cập nhật file Pipeline `Program.cs`

Mở file chính của cổng chạy là `Program.cs` ra, xoá phần code default (`app.MapGet("/", () => "Hello World")`), rồi điền khung source code cho đúng mẫu hiện tại:

```csharp
using ApiGateway.Config;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Tải file cấu hình JSON của Ocelot (sẽ load đan xen trên Host)
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2. Chèn JWT Token Verify qua File Helper của bạn - thư mục Config/JwtConfiguration.cs
builder.Services.AddAppJwt(builder.Configuration);

// 3. Chèn mở CORS Policy File Helper của bạn - thư mục Config/CorsConfiguration.cs
builder.Services.AddAppCors();

// 4. Bật chế độ API cho Controller và Http Factory Call
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// 5. Cắm dịch vụ Ocelot gốc vào Project Module này
builder.Services.AddOcelot();

var app = builder.Build();

/* ================ MODULE PIPELINES XỬ LÝ ================ */

// Thứ Tự UseCors -> UseAuth là RẤT QUAN TRỌNG
app.UseCors(CorsConfiguration.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

// Middleware chặn mã Ocelot rỗng thành JSON đẹp của Project
app.UseMiddleware<OcelotErrorResponseMiddleware>();

// Route các cấu trúc Controller tự tạo (Ví dụ: Thống kê Profile, Test Route tại local của con Gateway này)
app.MapControllers();

// Endpoint cuối cùng mà Pipeline đón là gửi về dịch vụ Proxy của Thư Viện Ocelot Route
await app.UseOcelot();

// Bắt đầu Listening Ports Server
app.Run();
```

---

## Bước 6: Thử nghiệm (Test & Run)

1. Click chuột phải vào `ApiGateway` chọn **Set as Startup Project**.
2. Phía thanh ngang trên cao, sẽ có nút Icon chạy Web hình ▶ xanh lá, lúc bung dropdown ra, bạn nhấn chọn `http` thay đổi từ môi trường `IIS Express` sang tên **`ApiGateway (http)`** để Console Panel đen Kestrel tự mở tách rời cho dễ quan sát Log.
3. Nhấn **F5** để Build code và chạy. Nếu Ocelot hiển thị dòng _"Ocelot is running"_, xin chúc mừng, API Gateway của bạn đã được tái tạo lại bằng Visual Studio thành công!
