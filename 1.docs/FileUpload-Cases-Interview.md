# File Upload — Cases thực tế & Câu hỏi phỏng vấn

## 1. Các case phải xử lý khi upload file

### 1.1 Validate phía Client (Frontend)

| Case | Xử lý |
|------|--------|
| Sai định dạng (pdf thay vì jpg) | Check `file.type` trước khi gửi lên server |
| Vượt quá dung lượng | Check `file.size < 5 * 1024 * 1024` (5MB) |
| Upload nhiều file vượt giới hạn | Giới hạn `nzLimit="10"` hoặc check `fileList.length` |
| File rỗng (0 byte) | Check `file.size > 0` |
| Tên file có ký tự đặc biệt | Sanitize hoặc rename bằng UUID trên server |

### 1.2 Validate phía Server (Backend)

| Case | Xử lý |
|------|--------|
| Content-Type bị giả mạo | Đọc magic bytes của file để xác nhận thật sự là ảnh |
| Vượt quá dung lượng | Giới hạn `MaxRequestBodySize` trong server config |
| File không có extension | Từ chối hoặc gán extension theo Content-Type |
| Upload file mã độc (script giả là ảnh) | Scan trên server, không chỉ tin vào extension |
| Tên file trùng nhau | Luôn rename bằng GUID: `{Guid.NewGuid():N}{ext}` |

### 1.3 Vấn đề lưu trữ

| Case | Giải pháp |
|------|-----------|
| **Ảnh rác** (upload nhưng không submit form) | Temp folder + MinIO Lifecycle Policy tự xóa sau 24h |
| **Ảnh cũ** sau khi update entity | Gọi `DeleteFileAsync(oldUrl)` trước khi lưu URL mới |
| **Ảnh bị mất** sau khi xóa entity | Soft delete entity, giữ file; hoặc implement cascade delete |
| File quá lớn cho network | Multipart upload / chunked upload (chia nhỏ rồi ghép) |
| CDN invalidation sau khi cập nhật ảnh | Đổi tên file mới thay vì ghi đè để tránh cache cũ |

### 1.4 Bảo mật

| Case | Xử lý |
|------|--------|
| Path traversal (`../../etc/passwd`) | Không dùng tên file gốc, luôn dùng GUID |
| Lộ đường dẫn nội bộ server | Trả về URL public qua CDN/Storage, không expose filesystem |
| IDOR — truy cập file của người khác | Gắn ownerId vào metadata/DB, check quyền khi download |
| Hotlinking (site khác nhúng ảnh của mình) | Signed URL với thời gian hết hạn |
| DDoS qua upload liên tục | Rate limiting trên upload endpoint |

### 1.5 Hiệu năng

| Case | Giải pháp |
|------|-----------|
| Upload chậm do file lớn | Hiển thị progress bar (theo dõi `onProgress` của XHR) |
| Server bị nghẽn khi nhiều người upload | Upload thẳng từ FE lên S3/MinIO (presigned URL), bypass server |
| Ảnh gốc 10MB hiển thị trên thumbnail 100px | Resize/compress trên server hoặc dùng dịch vụ CDN resize |
| Nhiều microservice cùng cần upload | Tách riêng một **MediaService** dùng chung (CommonService) |

---

## 2. Câu hỏi phỏng vấn thường gặp

### 2.1 Câu hỏi cơ bản

**Q: Sự khác nhau giữa upload file lên server filesystem và Object Storage (S3/MinIO)?**
> - **Filesystem**: đơn giản, nhanh, nhưng không scale được (tied to 1 server), mất file khi server chết.
> - **Object Storage**: scale vô hạn, highly available, tách biệt storage khỏi compute, dễ dùng CDN.
> Trong production, luôn ưu tiên Object Storage.

**Q: Làm sao ngăn user upload file độc hại (ví dụ .php giả là .jpg)?**
> 1. Không tin vào `file.name` hay `Content-Type` từ client.
> 2. Đọc **magic bytes** (file signature) để xác định loại file thật.
> 3. Lưu file với tên random (GUID), không preserve extension gốc nếu không cần thiết.
> 4. Không để web server execute file trong thư mục upload.

**Q: Tại sao nên rename file khi lưu thay vì giữ tên gốc?**
> - Tránh trùng tên gây ghi đè.
> - Tránh path traversal attack.
> - Tránh ký tự đặc biệt phá vỡ URL hoặc filesystem.
> - Tránh lộ thông tin qua tên file (ví dụ: `cv_nguyen_van_a.docx`).

---

### 2.2 Câu hỏi trung cấp

**Q: Làm thế nào xử lý ảnh rác (orphan files) trên Object Storage?**
> Có 3 cách phổ biến:
> 1. **Lifecycle Policy (tốt nhất)**: Upload vào `temp/`, Object Storage tự xóa sau N giờ. Khi commit entity thì copy sang folder thật.
> 2. **Cron Job đối soát**: định kỳ so sánh danh sách file trên Storage vs URL trong DB, xóa những file thừa.
> 3. **Event-Driven**: Lắng nghe sự kiện tạo/xóa entity, trigger xóa file tương ứng qua message queue.

**Q: Presigned URL là gì? Khi nào dùng?**
> Là URL tạm thời có chữ ký mà Object Storage cấp, cho phép client **upload/download thẳng** mà không qua server backend.
> - **Dùng khi**: File lớn, muốn giảm tải server, hoặc muốn bảo mật (URL hết hạn sau N phút).
> - **Luồng**: FE xin Presigned URL từ Backend → Backend tạo và trả → FE dùng URL đó upload thẳng lên S3.

**Q: Làm sao theo dõi tiến trình upload (progress bar)?**
> - Phía FE: dùng `XMLHttpRequest.upload.onprogress` hoặc `axios` với `onUploadProgress`.
> - Phía thư viện (Angular + ng-zorro): dùng `nzCustomRequest` kết hợp `item.onProgress?.({ percent })`.
> - Phải dùng streaming/chunked upload nếu file > vài trăm MB.

**Q: Khi user update ảnh (đổi thumbnail), ảnh cũ có bị xóa không? Làm thế nào xử lý?**
> Ảnh cũ **không tự xóa** — phải xử lý thủ công:
> 1. Trước khi save URL mới, lấy `oldUrl` từ entity trong DB.
> 2. Gọi `DeleteFileAsync(oldUrl)` để xóa ảnh cũ khỏi Storage.
> 3. Lưu URL mới vào DB.
> Chú ý: Nếu có CDN, cần invalidate cache của URL cũ.

---

### 2.3 Câu hỏi nâng cao

**Q: Thiết kế hệ thống upload file cho 1 triệu user đồng thời như thế nào?**
> 1. **Presigned URL**: FE upload thẳng lên S3/MinIO, không tốn bandwidth server.
> 2. **CDN** (CloudFront, Cloudflare): phân phối file tới user gần nhất theo địa lý.
> 3. **Resize on-the-fly**: Dùng service như Imgix hoặc Lambda@Edge để resize ảnh theo yêu cầu.
> 4. **Rate limiting**: Giới hạn số lần upload per user per minute.
> 5. **Async processing**: Sau khi upload, đưa vào queue để xử lý nặng (resize, virus scan) bất đồng bộ.

**Q: Chunked upload là gì? Khi nào cần?**
> Chia file lớn thành nhiều phần nhỏ (chunk), upload từng chunk, server ghép lại.
> - **Ưu điểm**: Có thể resume nếu mất kết nối, tránh timeout với file GB.
> - **Khi cần**: File > 100MB, hoặc môi trường mạng không ổn định.
> - **Thực hiện**: Dùng `tus` protocol, AWS S3 Multipart Upload, hoặc tự implement.

**Q: Tại sao không nên lưu file vào database (BLOB)?**
> - DB không được tối ưu cho binary large objects → chậm, tốn RAM.
> - Backup DB trở nên khổng lồ.
> - Không thể dùng CDN cho file lưu trong DB.
> - Khó scale horizontally.
> ✅ Chỉ lưu **URL/path** vào DB, file thật lưu trên Object Storage hoặc filesystem riêng.

---

## 3. Pattern trong dự án này (MicroserviceShop)

```
FE Upload ảnh
    → POST /api/v1/media/upload
    → MediaServiceImpl.UploadFileAsync(file, folder:"products")
    → MinIO: shop-media/products/{guid}.jpg
    → Trả về URL

FE Submit form tạo/cập nhật sản phẩm
    → POST /api/v1/products { thumbnailUrl: "https://..." }
    → ProductServiceImpl lưu URL vào DB

Ảnh cũ (khi update):
    → Lấy oldUrl từ entity
    → Gọi DeleteFileAsync(oldUrl) trước khi save mới
```
