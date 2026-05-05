# Hướng Dẫn Test Toàn Diện (End-To-End) Hệ Thống MicroserviceShop

Tài liệu này hướng dẫn chi tiết các bước để khởi động hệ thống, test thông qua API Gateway, cơ chế cấp phát Token, và xem luồng sự kiện truyền tải qua RabbitMQ.

---

## Phần 1. Chuẩn Bị & Khởi Động

### Bước 1: Khởi động Hạ tầng (Infrastructure)

Hệ thống yêu cầu **PostgreSQL** và **RabbitMQ** chạy nền. Khởi động các container Docker:

```bash
# Bật Docker Desktop và chạy lệnh sau (hoặc cấu hình docker-compose nếu có)
docker start <tên_container_postgres>
docker start <tên_container_rabbitmq>

# Đảm bảo PostgreSQL chạy ở port 5433 (tuỳ cấu hình hệ thống của bạn).
# Đảm bảo RabbitMQ chạy ở port 5673 và Management UI ở port 15673.
```

### Bước 2: Cập Nhật Database (Migrations)

Nếu chưa có DB, hãy khởi tạo:

- **Tạo DB cho AuthService:**
  ```bash
  cd t:\TryHard_IT_Project\Intern\MicroserviceShop\AuthService
  dotnet ef database update
  ```
- **Tạo DB cho CustomerService:**
  ```bash
  cd t:\TryHard_IT_Project\Intern\MicroserviceShop\CustomerService
  dotnet ef database update
  ```

### Bước 3: Chạy các Service (Cần Mở Các Cửa Sổ Terminal Khác Nhau)

Mở **3 Terminal riêng biệt** để chạy lần lượt các service:

- **Terminal 1 — ApiGateway**

  ```bash
  cd t:\TryHard_IT_Project\Intern\MicroserviceShop\ApiGateway
  dotnet run
  ```

  _Dấu hiệu nhận biết:_ `Now listening on: http://localhost:5000`

- **Terminal 2 — AuthService**

  ```bash
  cd t:\TryHard_IT_Project\Intern\MicroserviceShop\AuthService
  dotnet run
  ```

  _Dấu hiệu nhận biết:_ `Now listening on: http://localhost:5017`

- **Terminal 3 — CustomerService**
  ```bash
  cd t:\TryHard_IT_Project\Intern\MicroserviceShop\CustomerService
  dotnet run
  ```
  _Dấu hiệu nhận biết:_ `Now listening on: http://localhost:5123`

---

## Phần 2. Hướng Dẫn Test Bằng Postman

**Lưu ý quan trọng:** Giao tiếp với toàn bộ hệ thống PHẢI đi qua 1 cổng duy nhất là **ApiGateway (Port 5000)**. Đừng gọi trực tiếp vào Port của Service nhỏ (vd 5017, 5123).

### Bước 1: Import Collection Có Sẵn

Chúng ta đã tạo sẵn file Collection.

1. Khởi động phần mềm **Postman**.
2. Nhấn nút **Import** ở góc trái trên cùng.
3. Kéo thả file `MicroserviceShop.postman_collection.json` (nằm ở thư mục gốc của project) vào Postman.

### Bước 2: Test Luồng Đăng Ký – Phát Sự Kiện

1. Tìm đến thư mục `Auth Service` > `1. Đăng ký`.
2. Mở request và xem tab **Body**. Ví dụ:
   ```json
   {
     "username": "nguyenvan",
     "email": "nguyenvan@gmail.com",
     "password": "Pass@1234"
   }
   ```
3. Nhấn **Send**.
   - **Kỳ vọng:** Postman trả về Http Status `201 Created`.
   - **Lưu ý:** Ngay khi này, AuthService đã bắn 1 sự kiện `UserRegisteredEvent` lên hệ thống RabbitMQ.

### Bước 3: Đăng Nhập & Cấp Token Tự Động

1. Mở request `Auth Service > 2. Đăng nhập`.
2. Dùng tài khoản bạn vừa đăng ký:
   ```json
   {
     "username": "nguyenvan@gmail.com",
     "password": "Pass@1234"
   }
   ```
3. Nhấn **Send**.
   - **Kỳ vọng:** Http Status `200 OK`.
   - **Đặc biệt:** Trong request Postman này đã được thiết lập 1 đoạn code "Test script". Khi nhận được Token, nó sẽ TỰ ĐỘNG lưu `accessToken` vào trong biến Global/Biến Environment (lấy tên biến là `{{jwt_token}}`).

### Bước 4: Test Lấy Dữ Liệu Có Bảo Mật

Bây giờ Postman đã có Token lưu ở biến, bạn có thể gọi mọi API khác!

1. Mở request `Customer Service > Lấy danh sách Customer`.
2. Chuyển qua tab **Headers**, bạn sẽ thấy có cấu hình: `Authorization : Bearer {{jwt_token}}`
3. Nhấn **Send**.
   - **Kỳ vọng:** Thành công lấy danh sách. ĐỒNG THỜI, bạn sẽ thấy xuất hiện trong danh sách record của `"nguyenvan"` mà bạn vừa đăng ký. Do Event lắng nghe đã thành công!

---

## Phần 3. Cách Xem Các Sự Kiện Gửi Qua Broker (RabbitMQ)

Thao tác này giúp bạn quan sát được thực tế "Messages" đang lưu thông giữa các microservices.

### Mở Management UI Của RabbitMQ

1. Mở trình duyệt (Chrome, Edge...)
2. Truy cập vào trang: **http://localhost:15673** (đây là giao diện Dashboard của RabbitMQ).
3. Đăng nhập:
   - **Username:** `guest`
   - **Password:** `guest`

### Quan Sát Hệ Thống Hàng Đợi (Queues)

1. Ở thanh menu ngang của web, nhấp vào tab **Queues**.
2. Tìm dòng có tên tương tự `user-registered-event` hoặc `customer-service-user-registered-consumer` (Mặc định MassTransit tự động tạo tên queue dựa trên tên Class của Message và Consumer).
3. **Các thông số quan trọng cần nhìn:**
   - Cột **Ready**: Số lượng message đang kẹt lại, chưa có người đọc. (Nếu bằng 0 tức là mọi thứ đã xử lý êm xuôi).
   - Cột **Message rates** => Cột con **In / Out**: Cột In nháy lên thể hiện Publisher (AuthService) đã gửi qua, cột Out nháy lên thể hiện Consumer (CustomerService) đã bắt được để xử lý.

### Test Thử RabbitMQ Bằng Tay

Muốn thấy rõ hoạt động của Queue, bạn hãy:

1. Vào Terminal của **CustomerService**, bấm `Ctrl + C` để tắt nó đi (giả lập CustomerService bị sập mạng).
2. Trở lại Postman, gọi lại API **Đăng ký** (Tạo thêm 1 user bâng quơ khác, vd `userb@gmail.com`).
3. Xem lại trang RabbitMQ. Bây giờ Cột **Ready** sẽ hiện số `1`. Bởi vì message được bắn ra nhưng không có CustomerService nào để bắt nó. Nó sẽ nằm an toàn trong Queue.
4. Cuối cùng, bật lại **CustomerService**. Bạn sẽ thấy Terminal CustomerService lập tức báo log: `"Đã tạo thành công Profile"`, và số **Ready** ở RabbitMQ tụt về 0. Điều này làm nên sức mạnh của tư duy Asynchronous Microservices!

---

### Cách Đọc Nội Dung Message (Event Payload) Dễ Hiểu Nhất

Mặc định khi thông báo bay qua Queue quá nhanh, bạn sẽ không kịp nhìn Payload. Để nhìn được nội dung Event, ta làm thế này:

1. Trạng thái tiên quyết: **Tắt bớt 1 Consumer Service đi** (ở ví dụ này là tắt CustomerService). Để message bị kẹt lại thành số `1` ở cột **Ready**.
2. Click thẳng vào chữ `user-registered` (link màu xanh) ở cột Name trên RabbitMQ.
3. Trong giao diện mới hiện ra, bạn kéo xuống mục **"Get messages"**.
4. Chọn **Ack Mode** là **`Nack message requeue true`** (Nếch-tin-nhắn-và-đưa-lại-vào-hàng-đợi). Tuỳ chọn này giúp ta lấy một bản copy ra xem, sau đó trả message gốc về chỗ cũ để Consumer sau này xử lý tiếp, không bị mất đi data!
5. Bấm nút **"Get Message(s)"**.
6. Lúc này màn hình sẽ sổ ra một phần thông tin, bạn kéo xuống phần **Payload (bytes)**. Trong này chính là cục chuỗi JSON mà AuthService vừa nhét vào, đại loại như:
   ```json
   {
     "messageId": "1b5...b9e",
     "conversationId": "1b5...b9f",
     "sourceAddress": "rabbitmq://localhost:5673/.../...",
     ...
     "message": {
       "userId": "1234abcd-...",
       "username": "nguyenvan",
       "email": "nguyenvan@gmail.com",
       "registeredAt": "2026-02-25T14:40:00.00Z"
     }
   }
   ```
   _=> Đây chính là Data thật 100% được luân chuyển giữa các Server!_
