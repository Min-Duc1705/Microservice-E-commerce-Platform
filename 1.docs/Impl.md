Hệ thống bán hàng trực tuyến.
________________________________________
I. Phân hệ quản trị (Admin Panel)
Đây là khu vực dành cho chủ cửa hàng và nhân viên quản lý hoạt động kinh doanh.
1. Quản lý hàng hóa
•	Danh sách hàng hóa:
o	Hiển thị dạng bảng (table) các thông tin: Ảnh đại diện, Tên hàng hóa, Mã SKU, Loại hàng hóa, Giá bán, Số lượng tồn kho, Trạng thái (Đang bán / Ngừng bán / Hết hàng).
•	Thêm mới hàng hóa:
o	Form nhập liệu bao gồm: Tên hàng hóa, Mã SKU (có thể tự động tạo), Mô tả chi tiết (dùng text editor), Giá nhập (để tính lãi), Giá bán, Số lượng tồn kho, Đơn vị tính (cái, kg, hộp...).
o	Cho phép upload nhiều hình ảnh sản phẩm.
o	Cho phép chọn Loại hàng hóa (danh mục) từ danh sách thả xuống.
•	Sửa hàng hóa:
o	Chỉnh sửa tất cả thông tin đã nhập khi thêm mới.
•	Xóa hàng hóa:
o	Nên dùng "Xóa mềm" (Soft delete) - tức là chỉ ẩn đi, không xóa vĩnh viễn khỏi CSDL để giữ liệu lịch sử bán hàng.
•	Tìm kiếm và Lọc:
o	Tìm kiếm theo Tên, Mã SKU.
o	Lọc theo Loại hàng hóa, Trạng thái.
•	Khóa (Ngừng bán):
o	Chuyển trạng thái hàng hóa sang "Ngừng bán". Hàng hóa này sẽ không hiển thị trên website nhưng vẫn lưu trong hệ thống.
•	Quản lý Loại hàng hóa (Danh mục):
o	Chức năng Thêm / Sửa / Xóa các loại hàng hóa (ví dụ: Áo sơ mi, Quần Jean, Phụ kiện). Đây là mục cha để nhóm các sản phẩm.
2. Quản lý khách hàng
•	Danh sách khách hàng:
o	Hiển thị: Tên khách hàng, Số điện thoại, Email, Địa chỉ, Tổng chi tiêu, Công nợ hiện tại.
•	Thêm mới khách hàng:
o	Form nhập tay thông tin khách hàng (dùng khi khách mua tại cửa hàng hoặc đặt qua điện thoại).
•	Xem chi tiết khách hàng:
o	Hiển thị thông tin cá nhân.
o	Hiển thị lịch sử mua hàng (danh sách các đơn hàng đã mua).
o	Hiển thị lịch sử công nợ (danh sách các lần mua nợ và các lần thanh toán).
•	Tìm kiếm:
o	Tìm theo Tên, SĐT, Email.
3. Quản lý đơn hàng mua (Đơn hàng từ khách)
•	Danh sách đơn hàng:
o	Hiển thị: Mã đơn, Tên khách hàng, Ngày đặt, Tổng tiền, Trạng thái (Mới / Đang xử lý / Đang giao / Hoàn thành / Đã hủy).
•	Xem chi tiết đơn hàng:
o	Hiển thị thông tin khách hàng, địa chỉ giao hàng.
o	Danh sách các sản phẩm trong đơn (tên, số lượng, đơn giá, thành tiền).
o	Tổng tiền hàng, Phí vận chuyển (nếu có), Tổng cộng thanh toán.
o	Phương thức thanh toán.
•	Cập nhật trạng thái đơn hàng:
o	Cho phép Admin chuyển trạng thái đơn hàng (ví dụ: từ "Mới" sang "Đang xử lý").
o	Khi chuyển sang "Đang xử lý" hoặc "Hoàn thành", hệ thống phải tự động trừ số lượng tồn kho của các sản phẩm tương ứng.
•	Hủy đơn hàng:
o	Cho phép hủy đơn. Khi hủy, hệ thống phải tự động hoàn trả số lượng tồn kho.
•	Tạo đơn hàng (Offline):
o	Giao diện cho phép Admin tự tạo đơn hàng mới (cho khách mua qua điện thoại/trực tiếp), tìm kiếm sản phẩm và thêm vào giỏ, chọn khách hàng (hoặc tạo mới).
•	In hóa đơn:
o	Tạo ra một trang in (hoặc PDF) tóm tắt thông tin đơn hàng.
4. Quản lý công nợ khách hàng
•	Danh sách công nợ:
o	Liệt kê tất cả khách hàng đang có công nợ (số nợ > 0).
•	Ghi nhận thanh toán (Tạo phiếu thu):
o	Khi khách hàng trả nợ, Admin vào mục này, chọn khách hàng, nhập số tiền thanh toán.
o	Hệ thống tự động cập nhật lại số công nợ còn lại của khách.
•	Lịch sử thanh toán nợ:
o	Xem lại chi tiết các lần khách hàng thanh toán nợ.
5. Thống kê hàng hóa
•	Báo cáo tồn kho:
o	Danh sách tất cả hàng hóa và số lượng tồn kho hiện tại.
o	Cảnh báo các mặt hàng sắp hết hàng (dưới một ngưỡng nhất định, ví dụ: < 5).
•	Báo cáo hàng bán chạy:
o	Top 10 (hoặc 20) sản phẩm bán chạy nhất theo số lượng hoặc doanh thu.
o	Lọc theo khoảng thời gian (hôm nay, tuần này, tháng này).
•	Báo cáo hàng tồn kho lâu:
o	Các sản phẩm không phát sinh đơn hàng trong X ngày (ví dụ: 30 ngày, 60 ngày).
6. Thống kê lợi nhuận (Lãi/Lỗ)
•	Báo cáo Doanh thu:
o	Tổng giá trị các đơn hàng đã bán (trạng thái "Hoàn thành").
o	Lọc theo thời gian.
•	Báo cáo Giá vốn hàng bán (COGS):
o	Tổng giá trị nhập kho của các sản phẩm đã bán. (Tính bằng: $\sum (\text{Giá nhập} \times \text{Số lượng đã bán})$ ).
•	Báo cáo Lợi nhuận gộp:
o	Công thức: Lợi nhuận = Doanh thu - Giá vốn hàng bán.
•	Biểu đồ:
o	Hiển thị biểu đồ cột hoặc đường biểu diễn doanh thu và lợi nhuận theo ngày, tháng.
7. Quản lý người dùng (Tài khoản Admin/Nhân viên)
•	Danh sách người dùng:
o	Hiển thị các tài khoản có quyền truy cập vào Phân hệ quản trị.
•	Tạo tài khoản nhân viên:
o	Tạo tài khoản mới (Tên đăng nhập, Mật khẩu).
•	Phân quyền (cho hệ thống lớn hơn, nhưng cơ bản):
o	Gán vai trò cho tài khoản (ví dụ: Admin - toàn quyền; Nhân viên bán hàng - chỉ được xem và xử lý đơn hàng).
•	Khóa tài khoản:
o	Vô hiệu hóa quyền truy cập của một nhân viên đã nghỉ việc.
________________________________________
II. Website bán hàng (Frontend)
Đây là giao diện mà người mua hàng truy cập và tương tác.
1. Trang chủ
•	Slider/Banner: Hiển thị các ảnh quảng cáo, khuyến mãi nổi bật.
•	Danh sách sản phẩm nhóm theo loại:
o	Hiển thị các khối (section) sản phẩm, ví dụ: "Hàng mới về", "Bán chạy nhất".
o	Hiển thị các khối theo Loại hàng hóa (ví dụ: "Áo sơ mi", "Quần Jean").
•	Card sản phẩm:
o	Mỗi sản phẩm hiển thị: Hình ảnh, Tên, Giá bán.
o	Nút "Thêm vào giỏ" hoặc "Xem chi tiết".
2. Trang danh mục hàng hóa
•	Khi người dùng bấm vào một loại hàng hóa (ví dụ: "Áo sơ mi").
•	Hiển thị tên danh mục.
•	Hiển thị lưới sản phẩm (grid) tất cả các sản phẩm thuộc danh mục đó.
•	Bộ lọc cơ bản: Lọc theo giá (Thấp đến cao, Cao đến thấp), Mới nhất.
•	Phân trang: Hiển thị (Trang 1, 2, 3...) nếu có quá nhiều sản phẩm.
3. Trang chi tiết hàng hóa
•	Thư viện ảnh: Hiển thị ảnh lớn và các ảnh thu nhỏ (thumbnails) của sản phẩm.
•	Thông tin chính: Tên sản phẩm, Giá bán, Mã SKU.
•	Trạng thái tồn kho: Hiển thị "Còn hàng" hoặc "Hết hàng".
•	Chọn số lượng: Ô cho phép người dùng nhập hoặc +/- số lượng muốn mua.
•	Nút "Thêm vào giỏ hàng".
•	Mô tả chi tiết: Hiển thị phần mô tả dài của sản phẩm.
•	Sản phẩm liên quan: Hiển thị các sản phẩm khác cùng loại.
4. Giỏ hàng (Cart)
•	Hiển thị khi người dùng bấm vào biểu tượng giỏ hàng.
•	Danh sách sản phẩm trong giỏ:
o	Hiển thị từng mục: Ảnh, Tên, Đơn giá, Số lượng (cho phép cập nhật), Thành tiền.
o	Nút "Xóa" để bỏ sản phẩm khỏi giỏ.
•	Tóm tắt đơn hàng:
o	Hiển thị Tạm tính (tổng tiền hàng).
o	Nút "Tiếp tục mua hàng" (quay lại trang chủ/danh mục).
o	Nút "Tiến hành thanh toán" (chuyển đến trang 5).
5. Trang thanh toán đơn hàng (Checkout)
•	Nếu chưa đăng nhập: Yêu cầu đăng nhập hoặc cho phép "Mua hàng không cần tài khoản".
•	Thông tin người nhận:
o	Form nhập: Họ tên, Số điện thoại, Email.
o	Form nhập: Địa chỉ (Tỉnh/Thành, Quận/Huyện, Phường/Xã, Số nhà).
•	Phương thức thanh toán:
o	Cho phép chọn, ví dụ: "Thanh toán khi nhận hàng (COD)" hoặc "Chuyển khoản ngân hàng" (hiển thị thông tin STK).
•	Kiểm tra lại đơn hàng:
o	Hiển thị lại danh sách sản phẩm, tổng tiền hàng, phí vận chuyển (nếu có).
o	Hiển thị Tổng cộng cuối cùng.
•	Nút "Đặt hàng":
o	Sau khi bấm, hệ thống tạo đơn hàng trong Admin (với trạng thái "Mới") và chuyển người dùng đến trang "Đặt hàng thành công".
6. Trang đăng ký tài khoản
•	Form đơn giản: Họ tên, Số điện thoại (hoặc Email), Mật khẩu, Nhập lại mật khẩu.
•	Nút "Đăng ký".
7. Trang đăng nhập
•	Form: Số điện thoại (hoặc Email), Mật khẩu.
•	Nút "Đăng nhập".
•	Link "Quên mật khẩu?" (Chức năng nâng cao: Gửi link reset qua email/SĐT).
8. Trang cập nhật thông tin người mua hàng (Trang cá nhân)
•	Chỉ truy cập được khi đã đăng nhập.
•	Tab Thông tin tài khoản:
o	Cho phép xem và chỉnh sửa: Họ tên, Email, SĐT.
o	Chức năng "Đổi mật khẩu".
•	Tab Sổ địa chỉ:
o	Cho phép Thêm / Sửa / Xóa các địa chỉ giao hàng để dùng cho các lần mua sau.
9. Trang quản lý hàng hóa đã mua (Lịch sử đơn hàng)
•	Chỉ truy cập được khi đã đăng nhập.
•	Hiển thị danh sách tất cả các đơn hàng người dùng đã đặt.
•	Mỗi đơn hàng hiển thị: Mã đơn, Ngày đặt, Tổng tiền, Trạng thái (Đang xử lý, Đã giao...).
•	Cho phép bấm vào "Xem chi tiết" để xem lại các sản phẩm trong đơn hàng đó.

 
Thành phần công nghệ:
🏗️ 1. Kiến trúc tổng quan
Hệ thống của bạn sẽ bao gồm:
•	Backend (.NET Web API): Một máy chủ cung cấp các "cửa" (gọi là API - Giao diện lập trình ứng dụng) để Frontend có thể lấy dữ liệu (ví dụ: GET /api/products) hoặc gửi dữ liệu (ví dụ: POST /api/orders).
•	Frontend (Angular): Một ứng dụng Single Page Application (SPA) chạy trên trình duyệt của người dùng. Nó sẽ gọi các API của Backend để hiển thị giao diện và thực hiện chức năng.
•	Database (PostgreSQL): Nơi lưu trữ toàn bộ dữ liệu (người dùng, sản phẩm, đơn hàng...).
________________________________________
🖥️ 2. Kiến thức Frontend (Angular)
Đây là những gì bạn cần để xây dựng giao diện người dùng.
•	Cơ bản về Angular:
o	Components: "Gạch" xây dựng nên giao diện (ví dụ: LoginComponent, ProductListComponent, CartComponent).
o	Modules (NgModule): Cách tổ chức và nhóm các Component (ví dụ: AuthModule, ProductModule).
o	Templates & Data Binding: Cách hiển thị dữ liệu (biến {{ ten_san_pham }}) và xử lý sự kiện (nút (click)="themVaoGio()").
•	Form (Rất quan trọng cho Đăng nhập, Đăng ký, Thanh toán):
o	Reactive Forms: Cách xây dựng các form động, mạnh mẽ. Bạn sẽ dùng FormGroup để quản lý form đăng nhập và FormBuilder để tạo form thanh toán phức tạp.
o	Validators: Cách kiểm tra dữ liệu (ví dụ: email có đúng định dạng, mật khẩu có đủ dài).
•	Giao tiếp với Backend:
o	HttpClientModule: Dịch vụ của Angular để gửi yêu cầu HTTP (GET, POST, PUT, DELETE) đến .NET API.
o	Services: Nơi bạn nên viết logic gọi API (ví dụ: tạo một AuthService chứa hàm login(), ProductService chứa hàm getProducts()).
•	Quản lý trạng thái (State Management):
o	Giỏ hàng (Cart): Bạn cần một nơi để lưu trữ giỏ hàng khi người dùng duyệt web. Cách đơn giản nhất là dùng một Service (ví dụ: CartService) kết hợp với BehaviorSubject (từ RxJS) để thông báo cho các component khác (như icon giỏ hàng trên header) khi có thay đổi.
o	localStorage: Dùng để lưu giỏ hàng ngay cả khi người dùng F5 lại trang.
•	Điều hướng (Routing):
o	RouterModule: Quyết định component nào được hiển thị dựa trên URL (ví dụ: /login -> LoginComponent, /products/:id -> ProductDetailComponent).
•	Bảo mật (Security):
o	AuthGuards: Dùng để bảo vệ các trang (ví dụ: CanActivate). Người dùng chưa đăng nhập sẽ không thể vào trang /checkout hay /profile.
o	HttpInterceptor: "Người gác cổng" cho mọi yêu cầu HTTP. Bạn sẽ dùng nó để tự động đính kèm Token xác thực (JWT) vào header của mỗi yêu cầu gửi lên .NET API sau khi người dùng đăng nhập.
________________________________________
🔧 3. Kiến thức Backend (.NET + PostgreSQL)
Đây là những gì bạn cần để xây dựng "bộ não" của hệ thống.
•	Nền tảng .NET:
o	.NET Web API: Khung (framework) chính để xây dựng các RESTful API.
o	Controllers: Các lớp nhận yêu cầu HTTP từ Angular (ví dụ: ProductsController, OrdersController).
o	Actions: Các phương thức trong Controller tương ứng với các hành động (ví dụ: [HttpGet("id")], [HttpPost]).
o	Dependency Injection (DI): Tính năng cốt lõi của .NET. Bạn sẽ "tiêm" (inject) các Service vào Controller, "tiêm" DbContext vào Service.
•	Giao tiếp với Database (PostgreSQL):
o	Entity Framework Core (EF Core): Đây là một ORM (Object-Relational Mapper). Nó cho phép bạn viết code C# để tương tác với PostgreSQL thay vì viết SQL thô.
o	Npgsql (Provider): Driver cụ thể để EF Core "nói chuyện" được với PostgreSQL.
o	DbContext: Lớp đại diện cho phiên kết nối với CSDL, quản lý các tập dữ liệu (ví dụ: DbSet<Product> Products).
o	LINQ: Ngôn ngữ truy vấn (giống SQL nhưng viết bằng C#) để bạn lấy dữ liệu (ví dụ: context.Products.Where(p => p.Price > 100).ToList()).
o	Migrations: Công cụ của EF Core để tự động tạo hoặc cập nhật các bảng trong PostgreSQL dựa trên các lớp Model (POCO) của bạn.
•	Bảo mật & Xác thực (Quan trọng nhất cho Đăng nhập):
o	ASP.NET Core Identity: (Khuyến khích dùng) Một hệ thống có sẵn của .NET để quản lý người dùng, vai trò, băm mật khẩu, và các quy trình đăng nhập/đăng ký.
o	Password Hashing: Không bao giờ lưu mật khẩu dạng văn bản (plaintext). ASP.NET Identity sẽ tự động băm (hash) và kiểm tra mật khẩu cho bạn (dùng thuật toán an toàn như BCrypt).
o	JWT (JSON Web Tokens): Cơ chế xác thực phổ biến nhất cho API.
1.	Khi người dùng đăng nhập thành công, .NET sẽ tạo ra một chuỗi Token (JWT) chứa thông tin người dùng (như UserID, Role) và gửi về cho Angular.
2.	Angular lưu Token này (trong localStorage).
3.	Với mọi yêu cầu sau đó (như mua hàng), Angular đính kèm Token này vào Header.
4.	.NET sẽ xác thực Token này để biết "ai" đang gửi yêu cầu mà không cần hỏi lại mật khẩu.
o	Attribute [Authorize]: Bạn chỉ cần thêm [Authorize] lên trên một Action (ví dụ: [HttpPost] public IActionResult CreateOrder(...)) để .NET tự động kiểm tra xem yêu cầu có đính kèm Token hợp lệ không.
•	Xử lý nghiệp vụ (Business Logic):
o	Service Layer (Tầng Dịch vụ): Nơi bạn đặt logic chính (ví dụ: OrderService chứa hàm CreateOrder). Controller chỉ nên gọi Service này.
o	Validation: Kiểm tra dữ liệu đầu vào (ví dụ: dùng thư viện FluentValidation để đảm bảo số lượng mua > 0, địa chỉ không được trống).
o	DTO (Data Transfer Objects): Tạo các lớp riêng để truyền dữ liệu giữa Angular và .NET (ví dụ: LoginRequestDto, CreateOrderDto) thay vì dùng trực tiếp Model của CSDL.
•	CORS (Cross-Origin Resource Sharing):
o	Vì Angular (ví dụ: localhost:4200) và .NET (ví dụ: localhost:5000) chạy ở 2 "nguồn" khác nhau, bạn bắt buộc phải cấu hình CORS trong .NET để cho phép trình duyệt gọi API.
________________________________________
🚀 4. Luồng chi tiết: Từ Đăng nhập đến Mua hàng
Đây là cách các công nghệ trên phối hợp với nhau.
A. Luồng Đăng nhập
1.	Frontend (Angular):
o	Người dùng nhập email/mật khẩu vào một ReactiveForm trong LoginComponent.
o	Khi nhấn "Đăng nhập", Component gọi hàm login() trong AuthService.
o	AuthService dùng HttpClient gửi yêu cầu POST đến http://localhost:5000/api/auth/login cùng với LoginRequestDto (chứa email/pass).
2.	Backend (.NET):
o	AuthController nhận yêu cầu tại Action [HttpPost("login")].
o	Action gọi Identity.SignInManager.PasswordSignInAsync() (hoặc logic kiểm tra mật khẩu đã hash bằng tay).
o	Nếu thành công, Backend dùng thư viện JWT để tạo ra một chuỗi JWT Token.
o	Backend trả về một JSON chứa Token đó (ví dụ: { "token": "..." }).
3.	Frontend (Angular):
o	AuthService nhận được Token.
o	Lưu Token này vào localStorage.
o	Điều hướng người dùng đến trang chủ (/home).
o	HttpInterceptor được cấu hình: Kể từ giây phút này, mọi yêu cầu HttpClient (như getProducts, postOrder) sẽ tự động lấy Token từ localStorage và thêm vào Header: Authorization: Bearer <token_cua_ban>.
B. Luồng Thêm vào giỏ (Client-side)
1.	Frontend (Angular):
o	Người dùng ở trang chi tiết sản phẩm, nhấn "Thêm vào giỏ".
o	Sự kiện (click) gọi hàm addToCart(product) trong ProductDetailComponent.
o	Component gọi CartService.addItem(product).
o	CartService (một Service client-side) thêm sản phẩm vào một mảng (array) hoặc BehaviorSubject, sau đó cập nhật lại localStorage để lưu giỏ hàng.
o	(Không cần gọi Backend ở bước này).
C. Luồng Mua hàng (Thanh toán)
1.	Frontend (Angular):
o	Người dùng vào trang /checkout. AuthGuard sẽ kiểm tra localStorage xem có Token không. Nếu không, chuyển hướng về /login.
o	Người dùng điền thông tin giao hàng vào ReactiveForm trong CheckoutComponent.
o	Nhấn "Đặt hàng", Component gọi OrderService.createOrder().
o	OrderService (Angular) lấy giỏ hàng từ CartService và thông tin giao hàng từ Form, đóng gói thành một CreateOrderDto.
o	OrderService dùng HttpClient gửi POST đến http://localhost:5000/api/orders (lúc này, HttpInterceptor sẽ tự động đính kèm Token).
2.	Backend (.NET):
o	OrdersController nhận yêu cầu tại Action [HttpPost].
o	Action này có [Authorize]. .NET tự động đọc Token từ Header, xác thực nó. Nếu hợp lệ, nó biết UserID của người đang đặt hàng.
o	Controller gọi IOrderService.CreateNewOrderAsync(orderDto, userId).
o	Đây là nghiệp vụ quan trọng nhất: a. Bắt đầu một Database Transaction (EF Core). (Rất quan trọng!) b. Kiểm tra logic: Lấy thông tin các sản phẩm trong giỏ (từ CSDL, không tin giá tiền từ client gửi lên). c. Kiểm tra tồn kho: SELECT và khóa (lock) các dòng sản phẩm để kiểm tra số lượng. d. Nếu đủ hàng: INSERT một dòng mới vào bảng Orders. e. Lấy OrderID vừa tạo, INSERT nhiều dòng vào bảng OrderItems (tương ứng với các sản phẩm trong giỏ). f. UPDATE bảng Products để trừ số lượng tồn kho. g. (Tùy chọn) INSERT vào bảng CustomerDebt (Công nợ) nếu thanh toán COD. h. Commit Transaction. Nếu có bất kỳ lỗi nào ở các bước trên, toàn bộ sẽ được Rollback (hủy bỏ).
o	Backend trả về Ok() (HTTP 200) hoặc BadRequest() (HTTP 400) nếu hết hàng.
3.	Frontend (Angular):
o	OrderService nhận kết quả.
o	Nếu thành công: Xóa giỏ hàng (trong CartService và localStorage), thông báo thành công và chuyển người dùng đến trang "Cảm ơn".
o	Nếu thất bại (ví dụ: "Hết hàng"): Hiển thị lỗi cho người dùng.
