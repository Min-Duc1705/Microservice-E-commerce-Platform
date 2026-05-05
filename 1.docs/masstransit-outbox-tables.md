# MassTransit Outbox Tables — Giải thích chi tiết

> **Ví dụ xuyên suốt:** Người dùng đăng ký tài khoản → AuthService publish `UserRegisteredEvent` → CustomerService & OrderService nhận và lưu data.

---

## Tại sao cần Outbox?

### Kịch bản KHÔNG có Outbox (nguy hiểm)

```
1. AuthService: INSERT User vào DB          ✅ OK
2. AuthService: SaveChangesAsync()          ✅ OK  ← User đã trong DB
3. AuthService: Publish(UserRegisteredEvent) ← RabbitMQ đột ngột DOWN
   → ❌ Event mất vĩnh viễn!
   → Customer/CustomerProfile không bao giờ được tạo
   → DB bị inconsistent với các service khác
```

### Kịch bản CÓ Outbox (an toàn)

```
1. AuthService: INSERT User vào DB
2. AuthService: Publish(event)              ← Ghi vào OutboxMessages (chưa gửi RabbitMQ)
3. AuthService: SaveChangesAsync()          ← 1 transaction: User + OutboxMessage cùng lưu
    ↓ (Background Worker mỗi 10s hoặc ngay lập tức)
4. Worker đọc OutboxMessages → gửi lên RabbitMQ
5. Gửi thành công → Xóa bản ghi khỏi OutboxMessages
```

Nếu RabbitMQ DOWN ở bước 4 → OutboxMessages vẫn còn → Worker thử lại → **Không bao giờ mất event!**

---

## Bảng 1: `OutboxMessages`

**Nhiệm vụ:** Kho chứa tạm event đang chờ gửi lên RabbitMQ.

### Các cột quan trọng

| Cột              | Kiểu                | Ý nghĩa                                                                                |
| ---------------- | ------------------- | -------------------------------------------------------------------------------------- |
| `SequenceNumber` | `bigint` (PK, auto) | Số thứ tự tăng dần — Worker xử lý theo thứ tự này (FIFO)                               |
| `MessageId`      | `uuid`              | ID duy nhất của message — tránh gửi trùng                                              |
| `MessageType`    | `text`              | Tên đầy đủ của event class. VD: `urn:message:CommonService.Events:UserRegisteredEvent` |
| `Body`           | `text` (JSON)       | Toàn bộ nội dung event được serialize thành JSON                                       |
| `Destination`    | `text` (URI)        | Exchange/queue trên RabbitMQ sẽ nhận message này                                       |
| `SentTime`       | `timestamp`         | Thời điểm tạo message                                                                  |
| `EnqueueTime`    | `timestamp?`        | Nếu có → delay delivery (gửi sau X phút)                                               |
| `Headers`        | `text?` (JSON)      | Metadata bổ sung (correlation ID, trace ID...)                                         |
| `ContentType`    | `text`              | Định dạng serialize. Thường là `application/vnd.masstransit+json`                      |
| `OutboxId`       | `uuid?`             | FK liên kết với `OutboxStates`                                                         |
| `ConversationId` | `uuid?`             | Dùng để trace toàn bộ luồng của 1 request                                              |
| `CorrelationId`  | `uuid?`             | Liên kết các event trong 1 business workflow                                           |

### Ví dụ bản ghi thực tế

```json
SequenceNumber : 1
MessageId      : "a1b2c3d4-..."
MessageType    : "urn:message:CommonService.Events:UserRegisteredEvent"
Body           : {
    "userId": "abc123",
    "username": "nguyenvana",
    "email": "a@gmail.com",
    "registeredAt": "2025-02-27T08:00:00Z"
  }
Destination    : "rabbitmq://localhost/CommonService.Events:UserRegisteredEvent"
SentTime       : "2025-02-27T08:00:00Z"
```

> **Lưu ý:** Bảng này thường **TRỐNG** trong điều kiện bình thường vì Worker xử lý rất nhanh (< 1 giây). Chỉ thấy data khi RabbitMQ bị down.

---

## Bảng 2: `OutboxStates`

**Nhiệm vụ:** Theo dõi trạng thái xử lý để tránh gửi trùng khi chạy nhiều instance.

### Vấn đề nó giải quyết

Nếu chạy 2 instance `AuthService` song song, cả 2 Worker đều đọc OutboxMessages → gửi event 2 lần → Consumer xử lý 2 lần → data trùng!

`OutboxStates` dùng cơ chế **Pessimistic Lock** (khóa DB row) để chỉ 1 Worker được xử lý tại 1 thời điểm.

### Các cột quan trọng

| Cột                  | Kiểu         | Ý nghĩa                                                                     |
| -------------------- | ------------ | --------------------------------------------------------------------------- |
| `OutboxId`           | `uuid` (PK)  | ID của batch xử lý                                                          |
| `LockId`             | `uuid`       | ID instance đang giữ lock — instance khác thấy lock này sẽ bỏ qua           |
| `Created`            | `timestamp`  | Thời điểm tạo OutboxState                                                   |
| `Delivered`          | `timestamp?` | Thời điểm hoàn thành delivery. `null` = đang xử lý hoặc chưa xử lý          |
| `LastSequenceNumber` | `bigint?`    | SequenceNumber cuối đã xử lý — Worker tiếp tục từ đây, không đọc lại từ đầu |
| `RowVersion`         | `bytea?`     | Optimistic concurrency control — tránh race condition khi update            |

---

## Bảng 3: `InboxStates`

**Nhiệm vụ:** Ghi nhớ event đã xử lý để chống Consumer nhận trùng (Idempotency).

### Vấn đề nó giải quyết

RabbitMQ dùng **"At-least-once delivery"** — đảm bảo event đến ít nhất 1 lần, nhưng **có thể đến 2 lần** (do mạng chập chờn, consumer ack fail...).

Không có `InboxStates`:

```
Event UserRegisteredEvent đến lần 1 → Tạo Customer ✅
Event UserRegisteredEvent đến lần 2 → Tạo Customer trùng ❌
```

Có `InboxStates`:

```
Event đến lần 1 → Tạo Customer → Ghi MessageId vào InboxStates ✅
Event đến lần 2 → Kiểm tra InboxStates → MessageId đã có → Bỏ qua ✅
```

### Các cột quan trọng

| Cột                  | Kiểu         | Ý nghĩa                                                       |
| -------------------- | ------------ | ------------------------------------------------------------- |
| `MessageId`          | `uuid`       | ID của event đã nhận (PK kết hợp với ConsumerId)              |
| `ConsumerId`         | `uuid`       | ID của Consumer class nhận event — mỗi Consumer có 1 ID riêng |
| `LockId`             | `uuid`       | Lock để tránh 2 instance xử lý cùng message                   |
| `Received`           | `timestamp`  | Thời điểm nhận message                                        |
| `ReceiveCount`       | `int`        | Đếm số lần thử nhận (tăng lên nếu bị retry)                   |
| `Consumed`           | `timestamp?` | Thời điểm xử lý thành công. `null` = đang xử lý               |
| `Delivered`          | `timestamp?` | Thời điểm ack về RabbitMQ                                     |
| `ExpirationTime`     | `timestamp?` | Sau thời gian này, bản ghi được cleanup tự động               |
| `LastSequenceNumber` | `bigint?`    | Sync với OutboxMessages nếu dùng request/response pattern     |

---

## Tổng quan flow đầy đủ

```
[AuthService]
    Publish(UserRegisteredEvent)
         ↓ SaveChangesAsync() — 1 transaction
    [OutboxMessages]: { Body: {...}, Destination: "rabbitmq://.../user-registered" }
    [OutboxStates]:   { LockId: "instance-1", LastSequenceNumber: null }
         ↓ Background Worker (vài giây)
    [RabbitMQ Exchange: CommonService.Events:UserRegisteredEvent]
         ↓ Fan-out
    ┌────────────────────────────┬────────────────────────────────┐
    ▼                            ▼
[CustomerService queue]    [OrderService queue]
[InboxStates]: MessageId   [InboxStates]: MessageId
→ Tạo Customer             → Tạo CustomerProfile
         ↓ Xóa khỏi OutboxMessages sau khi gửi thành công
    [OutboxMessages]: TRỐNG ← Bình thường!
```

---

## Cách test Outbox

```cmd
# Pause RabbitMQ (PostgreSQL vẫn chạy bình thường)
docker pause rabbitmq

# Đăng ký tài khoản mới
# → Vào DB xem OutboxMessages sẽ có bản ghi đang chờ ✅

# Resume RabbitMQ
docker unpause rabbitmq
# → Worker tự gửi, OutboxMessages tự xóa
# → CustomerService & OrderService nhận event và lưu data ✅
```

---

## Default timing

| Tham số            | Mặc định | Tùy chỉnh                                   |
| ------------------ | -------- | ------------------------------------------- |
| Query interval     | 10 giây  | `o.QueryDelay = TimeSpan.FromSeconds(5)`    |
| Message batch size | 100      | `o.MessageDeliveryLimit = 50`               |
| Query timeout      | 30 giây  | `o.QueryTimeout = TimeSpan.FromSeconds(15)` |
