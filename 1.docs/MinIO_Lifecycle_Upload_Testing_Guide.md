# Hướng dẫn Test: Chiến lược Upload Ảnh với MinIO Lifecycle Policy

## Tổng quan luồng hoạt động

```
[FE Upload ảnh]
      │
      ▼
POST /api/v1/media/upload
      │  (Upload vào temp/)
      ▼
MinIO: shop-media/temp/{guid}.jpg   ← sẽ tự xóa sau 24h nếu không commit
      │
      ▼ (Khi user submit form Tạo/Sửa Product)
POST /api/v1/products
      │  (BeforeSave: CommitFileAsync)
      ▼
MinIO: shop-media/products/{guid}.jpg  ← file thật, bền vững
      │
      ▼ (Lưu URL này vào DB)
PostgreSQL: products.thumbnail_url = "http://localhost:9000/shop-media/products/..."
```

> **Lợi ích chính:** Ảnh upload rồi bỏ (user thoát form, không submit) sẽ bị **MinIO tự dọn sau 24h** — không cần cron job hay xử lý thủ công.

---

## Điều kiện tiên quyết

Trước khi test, đảm bảo các service sau đang chạy:

```bash
# Khởi động MinIO + Database + RabbitMQ qua Docker
docker-compose up -d minio postgres rabbitmq

# Chạy ProductService (cổng 5002 theo launchSettings)
cd ProductService && dotnet run

# Chạy CustomerService (cổng 5003)
cd CustomerService && dotnet run
```

Truy cập **MinIO Console**: http://localhost:9001  
Login: `minioadmin` / `minioadmin`

---

## Bước 1: Kiểm tra Lifecycle Policy đã được cài đặt

Lifecycle Policy được tự động cài khi lần đầu upload file. Sau khi upload lần đầu, vào MinIO Console:

1. Chọn bucket `shop-media`
2. Click tab **Lifecycle**
3. Kiểm tra có rule: **delete-temp-after-24h** với Prefix `temp/`, Expiration `1 day`

Hoặc dùng MinIO CLI (`mc`):

```bash
# Kết nối mc với MinIO local
mc alias set local http://localhost:9000 minioadmin minioadmin

# Xem lifecycle policy của bucket
mc ilm ls local/shop-media
```

**Kết quả mong đợi:**
```
ID                    PREFIX  EXPIRY
delete-temp-after-24h temp/   1 day
```

---

## Bước 2: Test Upload vào thư mục temp/

### 2.1. Dùng Swagger
Truy cập: http://localhost:5002/swagger  
Endpoint: **POST /api/v1/media/upload**  

Form data:
- Key: `file` (type: File)
- Value: chọn file ảnh JPG/PNG

**Kết quả mong đợi (HTTP 200):**
```json
{
  "data": {
    "url": "http://localhost:9000/shop-media/temp/a1b2c3d4e5f6...jpg"
  }
}
```

> Lưu ý: URL trả về có dạng `.../temp/...` — đây là URL tạm thời.

### 2.2. Xác nhận file nằm trong thư mục temp/

Vào MinIO Console → Bucket `shop-media` → thư mục `temp/`  
Hoặc qua CLI:
```bash
mc ls local/shop-media/temp/
```

---

## Bước 3: Test Commit — Tạo Product với ảnh vừa upload

### 3.1. Dùng Swagger / Postman
Endpoint: **POST /api/v1/products**

Body (JSON):
```json
{
  "name": "Áo polo nam xanh",
  "description": "Chất liệu cotton 100%",
  "categoryId": "<uuid-danh-muc>",
  "sku": "APO-001",
  "price": 250000,
  "costPrice": 150000,
  "stockQuantity": 100,
  "unit": "Cái",
  "lowStockThreshold": 10,
  "thumbnailUrl": "http://localhost:9000/shop-media/temp/a1b2c3d4e5f6...jpg",
  "imageUrls": [
    "http://localhost:9000/shop-media/temp/b2c3d4e5f6a1...jpg"
  ]
}
```

**Kết quả mong đợi (HTTP 201):**
```json
{
  "data": {
    "id": "...",
    "thumbnailUrl": "http://localhost:9000/shop-media/products/a1b2c3d4e5f6...jpg",
    "imageUrls": [
      "http://localhost:9000/shop-media/products/b2c3d4e5f6a1...jpg"
    ]
  }
}
```

> ⚠️ URL trong response phải chuyển từ `.../temp/...` → `.../products/...`

### 3.2. Xác nhận file đã được di chuyển

```bash
# Không còn file trong temp/ (đã bị xóa sau commit)
mc ls local/shop-media/temp/

# File đã có trong products/
mc ls local/shop-media/products/
```

---

## Bước 4: Test tình huống "Ảnh rác" (Upload rồi không Submit)

Mục đích: Kiểm tra ảnh rác tự được dọn bởi MinIO Lifecycle sau 24h.

**Cách test nhanh (không chờ 24h):**
Dùng `mc` để set expiry check bằng tay:
```bash
# Xem ngày tạo của object temp
mc stat local/shop-media/temp/<filename>

# Liệt kê tất cả objects trong temp/ kèm thời gian
mc ls --recursive local/shop-media/temp/
```

File trong `temp/` sẽ tự bị xóa bởi MinIO sau **24h kể từ khi tạo**. Không cần action gì thêm.

---

## Bước 5: Test Update Product — Ảnh cũ không bị commit lại

Khi update product mà **không đổi ảnh** (gửi lại URL cũ đã trong `products/`):

**Body (JSON):**
```json
{
  "thumbnailUrl": "http://localhost:9000/shop-media/products/a1b2c3d4e5f6...jpg"
}
```

**Kết quả mong đợi:** URL giữ nguyên, không bị thay đổi.  
*(CommitFileAsync tự nhận biết URL không phải `temp/` và trả về nguyên.)*

---

## Bước 6: Test upload ảnh Customer Avatar

Tương tự Product, dùng endpoint của **CustomerService**:

- Upload avatar →  POST `/api/v1/media/upload` (qua CustomerService hoặc ProductService gateway)
- Tạo/Sửa customer với URL temp vừa nhận → avatar được commit sang `customers/`

```json
{
  "fullName": "Nguyễn Văn A",
  "phone": "0901234567",
  "avatarUrl": "http://localhost:9000/shop-media/temp/xxxx.jpg"
}
```

**Kết quả:** `avatarUrl` trong response phải là `.../customers/xxxx.jpg`

---

## Checklist kiểm tra nhanh

| # | Bước kiểm tra | Mong đợi | Đạt |
|---|---|---|---|
| 1 | POST `/api/v1/media/upload` | URL trả về có `temp/` | ☐ |
| 2 | Xem thư mục MinIO `temp/` | File hiện diện | ☐ |
| 3 | Lifecycle policy tồn tại | Rule `delete-temp-after-24h` | ☐ |
| 4 | POST `/api/v1/products` với URL temp | URL response chuyển từ `temp/` → `products/` | ☐ |
| 5 | File trong `temp/` bị xóa sau commit | `mc ls local/shop-media/temp/` rỗng | ☐ |
| 6 | File trong `products/` xuất hiện | `mc ls local/shop-media/products/` có file | ☐ |
| 7 | Update product với URL cũ | URL giữ nguyên không bị thay đổi | ☐ |
| 8 | Update product với URL temp mới | URL chuyển sang `products/` | ☐ |

---

## Xử lý sự cố thường gặp

### Lỗi: `Lifecycle policy could not be set`
- Kiểm tra MinIO version trong docker-compose: phải là `quay.io/minio/minio:RELEASE.2024-11-07T00-52-20Z` trở về trước.
- Kiểm tra MinIO không đang chạy dạng Gateway mode (cần Standalone mode).

### Lỗi: CommitFileAsync không copy được file
- Kiểm tra URL temp truyền vào có đúng format `http://localhost:9000/shop-media/temp/xxx` không.
- Kiểm tra file còn trong `temp/` hay đã bị xóa (ví dụ chạy lại lần thứ 2 với URL cũ).
- Xem log của ProductService/CustomerService: `[Media] Committed: temp/xxx → products/xxx`

### Lỗi: File ảnh không hiển thị trong frontend
- Kiểm tra bucket policy: phải là `public-read` (GetObject cho `*`).
- Kiểm tra URL trả về trong API response có đúng host `localhost:9000` không.
- Nếu chạy qua Docker network, URL có thể khác (cần dùng host mapping).
