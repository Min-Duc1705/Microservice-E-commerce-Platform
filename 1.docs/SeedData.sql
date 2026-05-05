-- =========================================================================================
-- SCRIPT SEED DATA NHẤT QUÁN XUYÊN SUỐT 3 DATABASE (AuthDb, CustomerDb, OrderDb)
-- =========================================================================================
-- Cập nhật: 2026-02-25  —  Khớp với các Model hiện tại (Guid PK, enum int)
-- Hướng dẫn:
--   Chạy từng phần trong TỪNG DATABASE tương ứng trên pgAdmin / DBeaver.
--   Mật khẩu chung: Pass@123  (bcrypt hash sẵn bên dưới)
-- =========================================================================================
-- QUY ƯỚC GUID:
--   Role:       11111111-1111-4111-8111-1111111111xx
--   Permission: 22222222-2222-4222-8222-2222222222xx
--   User:       aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa01..20
--   Customer:   aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa01..20  (cùng User)
--   Order:      bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb01..20
--   OrderItem:  cccccccc-cccc-4ccc-8ccc-cccccccccc01..40
--   Product(giả):dddddddd-dddd-4ddd-8ddd-dddddddddd01..10
-- =========================================================================================


-- ==========================================
-- 🔴 PHẦN 1: CHẠY TRONG DATABASE "AuthDb" 🔴
-- ==========================================

-- 1. Xoá dữ liệu cũ
TRUNCATE TABLE "RolePermissions" CASCADE;
TRUNCATE TABLE "Permissions" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Users" CASCADE;
TRUNCATE TABLE "Roles" RESTART IDENTITY CASCADE;

-- ─── 2. ROLES (4 roles) ───
INSERT INTO "Roles" ("Id", "Name", "Description", "IsActive", "CreatedAt", "UpdatedAt") VALUES
('11111111-1111-4111-8111-111111111101', 'SUPER_ADMIN', 'Quản trị viên tối cao',      true, NOW(), NOW()),
('11111111-1111-4111-8111-111111111102', 'ADMIN',       'Quản trị viên hệ thống',     true, NOW(), NOW()),
('11111111-1111-4111-8111-111111111103', 'STAFF',       'Nhân viên bán hàng',         true, NOW(), NOW()),
('11111111-1111-4111-8111-111111111104', 'USER',        'Khách hàng thông thường',    true, NOW(), NOW());

-- ─── 3. PERMISSIONS (20 quyền) ───
INSERT INTO "Permissions" ("Id", "Name", "ApiPath", "Method", "Module", "CreatedAt", "UpdatedAt") VALUES
-- AUTH module
('22222222-2222-4222-8222-222222222201', 'Xem profile',            '/api/v1/auth/account',      'GET',    'AUTH',      NOW(), NOW()),
('22222222-2222-4222-8222-222222222202', 'Đổi mật khẩu',           '/api/v1/auth/password',      'PUT',    'AUTH',      NOW(), NOW()),
-- ROLE module
('22222222-2222-4222-8222-222222222203', 'Xem danh sách Role',     '/api/v1/roles',              'GET',    'ROLE',      NOW(), NOW()),
('22222222-2222-4222-8222-222222222204', 'Tạo Role',               '/api/v1/roles',              'POST',   'ROLE',      NOW(), NOW()),
('22222222-2222-4222-8222-222222222205', 'Sửa Role',               '/api/v1/roles/{id}',         'PUT',    'ROLE',      NOW(), NOW()),
('22222222-2222-4222-8222-222222222206', 'Xoá Role',               '/api/v1/roles/{id}',         'DELETE', 'ROLE',      NOW(), NOW()),
-- PERMISSION module
('22222222-2222-4222-8222-222222222207', 'Xem danh sách Permission', '/api/v1/permissions',      'GET',    'PERMISSION', NOW(), NOW()),
('22222222-2222-4222-8222-222222222208', 'Tạo Permission',         '/api/v1/permissions',        'POST',   'PERMISSION', NOW(), NOW()),
('22222222-2222-4222-8222-222222222209', 'Sửa Permission',         '/api/v1/permissions/{id}',   'PUT',    'PERMISSION', NOW(), NOW()),
('22222222-2222-4222-8222-222222222210', 'Xoá Permission',         '/api/v1/permissions/{id}',   'DELETE', 'PERMISSION', NOW(), NOW()),
-- USER module
('22222222-2222-4222-8222-222222222211', 'Xem danh sách User',     '/api/v1/users',              'GET',    'USER',      NOW(), NOW()),
('22222222-2222-4222-8222-222222222212', 'Cập nhật User',          '/api/v1/users/{id}',         'PUT',    'USER',      NOW(), NOW()),
('22222222-2222-4222-8222-222222222213', 'Xoá User',              '/api/v1/users/{id}',         'DELETE', 'USER',      NOW(), NOW()),
-- CUSTOMER module
('22222222-2222-4222-8222-222222222214', 'Xem danh sách Customer', '/api/customers',             'GET',    'CUSTOMER',  NOW(), NOW()),
('22222222-2222-4222-8222-222222222215', 'Cập nhật Customer',      '/api/customers/{id}',        'PUT',    'CUSTOMER',  NOW(), NOW()),
('22222222-2222-4222-8222-222222222216', 'Xoá Customer',           '/api/customers/{id}',        'DELETE', 'CUSTOMER',  NOW(), NOW()),
-- ORDER module
('22222222-2222-4222-8222-222222222217', 'Xem danh sách Order',    '/api/orders',                'GET',    'ORDER',     NOW(), NOW()),
('22222222-2222-4222-8222-222222222218', 'Tạo Order',              '/api/orders',                'POST',   'ORDER',     NOW(), NOW()),
('22222222-2222-4222-8222-222222222219', 'Cập nhật Order',         '/api/orders/{id}',           'PUT',    'ORDER',     NOW(), NOW()),
('22222222-2222-4222-8222-222222222220', 'Xoá/Huỷ Order',         '/api/orders/{id}',           'DELETE', 'ORDER',     NOW(), NOW());

-- ─── 4. ROLE – PERMISSION (mapping) ───
-- SUPER_ADMIN: TẤT CẢ 20 quyền
INSERT INTO "RolePermissions" ("PermissionsId", "RolesId") VALUES
('22222222-2222-4222-8222-222222222201', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222202', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222203', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222204', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222205', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222206', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222207', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222208', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222209', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222210', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222211', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222212', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222213', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222214', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222215', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222216', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222217', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222218', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222219', '11111111-1111-4111-8111-111111111101'),
('22222222-2222-4222-8222-222222222220', '11111111-1111-4111-8111-111111111101');

-- ADMIN: Quản lý User, Customer, Order + xem Role/Permission + Auth
INSERT INTO "RolePermissions" ("PermissionsId", "RolesId") VALUES
('22222222-2222-4222-8222-222222222201', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222202', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222203', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222207', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222211', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222212', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222213', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222214', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222215', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222216', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222217', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222218', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222219', '11111111-1111-4111-8111-111111111102'),
('22222222-2222-4222-8222-222222222220', '11111111-1111-4111-8111-111111111102');

-- STAFF: Xem + tạo Order, xem Customer, Auth
INSERT INTO "RolePermissions" ("PermissionsId", "RolesId") VALUES
('22222222-2222-4222-8222-222222222201', '11111111-1111-4111-8111-111111111103'),
('22222222-2222-4222-8222-222222222202', '11111111-1111-4111-8111-111111111103'),
('22222222-2222-4222-8222-222222222214', '11111111-1111-4111-8111-111111111103'),
('22222222-2222-4222-8222-222222222217', '11111111-1111-4111-8111-111111111103'),
('22222222-2222-4222-8222-222222222218', '11111111-1111-4111-8111-111111111103'),
('22222222-2222-4222-8222-222222222219', '11111111-1111-4111-8111-111111111103');

-- USER: Chỉ xem profile, đổi pass, xem Order của mình, tạo Order
INSERT INTO "RolePermissions" ("PermissionsId", "RolesId") VALUES
('22222222-2222-4222-8222-222222222201', '11111111-1111-4111-8111-111111111104'),
('22222222-2222-4222-8222-222222222202', '11111111-1111-4111-8111-111111111104'),
('22222222-2222-4222-8222-222222222217', '11111111-1111-4111-8111-111111111104'),
('22222222-2222-4222-8222-222222222218', '11111111-1111-4111-8111-111111111104');

-- ─── 5. USERS (20 tài khoản) ─── Mật khẩu: Pass@123
INSERT INTO "Users" ("Id", "RoleId", "Username", "Email", "PasswordHash", "IsActive", "CreatedAt", "UpdatedAt", "RefreshToken") VALUES
-- 2 Super Admin
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa', '11111111-1111-4111-8111-111111111101', 'superadmin',   'superadmin@shop.vn',  '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
-- 2 Admin
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1', '11111111-1111-4111-8111-111111111102', 'admin01',      'admin01@shop.vn',     '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa2', '11111111-1111-4111-8111-111111111102', 'admin02',      'admin02@shop.vn',     '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
-- 3 Staff
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa3', '11111111-1111-4111-8111-111111111103', 'staff01',      'staff01@shop.vn',     '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa4', '11111111-1111-4111-8111-111111111103', 'staff02',      'staff02@shop.vn',     '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa5', '11111111-1111-4111-8111-111111111103', 'staff03',      'staff03@shop.vn',     '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', false, NOW(), NOW(), NULL),
-- 14 User (khách hàng)
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6', '11111111-1111-4111-8111-111111111104', 'nguyenvanan',  'an.nguyen@gmail.com',    '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa7', '11111111-1111-4111-8111-111111111104', 'tranthihanh',  'hanh.tran@gmail.com',    '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa8', '11111111-1111-4111-8111-111111111104', 'lehoangcuong', 'cuong.le@gmail.com',     '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa9', '11111111-1111-4111-8111-111111111104', 'phamminhthu',  'thu.pham@gmail.com',     '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa10', '11111111-1111-4111-8111-111111111104', 'voquangduc',   'duc.vo@gmail.com',       '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa11', '11111111-1111-4111-8111-111111111104', 'doanmaihuong', 'huong.doan@gmail.com',   '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa12', '11111111-1111-4111-8111-111111111104', 'buivankhoa',   'khoa.bui@gmail.com',     '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa13', '11111111-1111-4111-8111-111111111104', 'hothanhlam',   'lam.ho@gmail.com',       '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa14', '11111111-1111-4111-8111-111111111104', 'dangthimai',   'mai.dang@gmail.com',     '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa15', '11111111-1111-4111-8111-111111111104', 'lythanhson',   'son.ly@gmail.com',       '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa16', '11111111-1111-4111-8111-111111111104', 'ngothiphuong', 'phuong.ngo@gmail.com',   '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', false, NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa17', '11111111-1111-4111-8111-111111111104', 'truongvantai', 'tai.truong@gmail.com',   '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa18', '11111111-1111-4111-8111-111111111104', 'caothiyen',    'yen.cao@gmail.com',      '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa19', '11111111-1111-4111-8111-111111111104', 'luuducbao',    'bao.luu@gmail.com',      '$2a$11$N/4X77aB0YmI7W/yItvV/elIfs.9t1QhP/Yv.kG/0K/x338fLq.36', true,  NOW(), NOW(), NULL);


-- ==============================================
-- 🟢 PHẦN 2: CHẠY TRONG DATABASE "CustomerDb" 🟢
-- ==============================================
TRUNCATE TABLE "Customers" CASCADE;

-- GUID khớp với Users bên AuthDb (mô phỏng UserRegisteredEvent sync)
-- Status enum: 0=Active, 1=Blocked
INSERT INTO "Customers" ("Id", "FullName", "Phone", "Email", "Address", "TotalSpent", "DebtAmount", "Status", "IsDeleted", "CreatedAt", "UpdatedAt") VALUES
-- Admin & Staff (vẫn có record Customer để thống nhất)
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa', 'Super Admin',        '0900000000', 'superadmin@shop.vn',  'Văn phòng công ty',           0,        0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1', 'Admin 01',           '0900000001', 'admin01@shop.vn',     'Văn phòng công ty',           0,        0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa2', 'Admin 02',           '0900000002', 'admin02@shop.vn',     'Văn phòng công ty',           0,        0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa3', 'Nhân viên Hùng',     '0900000003', 'staff01@shop.vn',     'Văn phòng chi nhánh 1',       0,        0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa4', 'Nhân viên Lan',      '0900000004', 'staff02@shop.vn',     'Văn phòng chi nhánh 2',       0,        0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa5', 'Nhân viên Tuấn',     '0900000005', 'staff03@shop.vn',     'Văn phòng chi nhánh 3',       0,        0, 0, false, NOW(), NOW()),
-- 14 Khách hàng thật
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6', 'Nguyễn Văn An',      '0912345601', 'an.nguyen@gmail.com',   '123 Lê Lợi, Q.1, TP.HCM',       4250000,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa7', 'Trần Thị Hạnh',      '0912345602', 'hanh.tran@gmail.com',   '456 Nguyễn Huệ, Q.1, TP.HCM',   2800000,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa8', 'Lê Hoàng Cường',     '0912345603', 'cuong.le@gmail.com',    '789 CMT8, Q.3, TP.HCM',         1620000,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa9', 'Phạm Minh Thư',      '0912345604', 'thu.pham@gmail.com',    '10 Pasteur, Q.3, TP.HCM',        950000, 150000, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa10', 'Võ Quang Đức',        '0912345605', 'duc.vo@gmail.com',      '22 Hai Bà Trưng, Q.1, TP.HCM',  3100000,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa11', 'Đoàn Mai Hương',      '0912345606', 'huong.doan@gmail.com',  '55 Võ Văn Tần, Q.3, TP.HCM',     780000,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa12', 'Bùi Văn Khoa',        '0912345607', 'khoa.bui@gmail.com',    '88 Lý Tự Trọng, Q.1, TP.HCM',   1450000,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa13', 'Hồ Thanh Lâm',        '0912345608', 'lam.ho@gmail.com',      '15 Nguyễn Trãi, Q.5, TP.HCM',         0,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa14', 'Đặng Thị Mai',        '0912345609', 'mai.dang@gmail.com',    '32 An Dương Vương, Q.5, TP.HCM',  890000,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa15', 'Lý Thanh Sơn',        '0912345610', 'son.ly@gmail.com',      '66 Trần Hưng Đạo, Q.1, TP.HCM', 2230000, 200000, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa16', 'Ngô Thị Phương',      '0912345611', 'phuong.ngo@gmail.com',  '100 Phạm Ngũ Lão, Q.1, TP.HCM',       0,      0, 1, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa17', 'Trương Văn Tài',      '0912345612', 'tai.truong@gmail.com',  '8 Điện Biên Phủ, Bình Thạnh',   1100000,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa18', 'Cao Thị Yến',         '0912345613', 'yen.cao@gmail.com',     '77 Nguyễn Thị Minh Khai, Q.3',   560000,      0, 0, false, NOW(), NOW()),
('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa19', 'Lưu Đức Bảo',         '0912345614', 'bao.luu@gmail.com',     '45 Tôn Đức Thắng, Q.1, TP.HCM',  320000,      0, 0, false, NOW(), NOW());


-- ============================================
-- 🔵 PHẦN 3: CHẠY TRONG DATABASE "OrderDb" 🔵
-- ============================================
TRUNCATE TABLE "OrderItems" CASCADE;
TRUNCATE TABLE "Orders" CASCADE;

-- Bảng Product giả định (dùng trong OrderItem):
-- dddddddd-dddd-4ddd-8ddd-dddddddddd01 = Áo thun nam         150,000
-- dddddddd-dddd-4ddd-8ddd-dddddddddd02 = Quần jean            350,000
-- dddddddd-dddd-4ddd-8ddd-dddddddddd03 = Giày thể thao        890,000
-- dddddddd-dddd-4ddd-8ddd-dddddddddd04 = Túi xách nữ          520,000
-- dddddddd-dddd-4ddd-8ddd-dddddddddd05 = Nón lưỡi trai         95,000
-- dddddddd-dddd-4ddd-8ddd-dddddddddd06 = Balo laptop           680,000
-- dddddddd-dddd-4ddd-8ddd-dddddddddd07 = Kính mát              250,000
-- dddddddd-dddd-4ddd-8ddd-dddddddddd08 = Đồng hồ thời trang 1,200,000

-- PaymentMethod enum: 0=COD, 1=BankTransfer, 2=VNPay, 3=Momo
-- OrderStatus enum:   0=New, 1=Processing, 2=Shipping, 3=Completed, 4=Cancelled

INSERT INTO "Orders" ("Id", "CustomerId", "CustomerName", "CustomerPhone", "ShippingAddress", "PaymentMethod", "ShippingFee", "Note", "Status", "CreatedAt") VALUES
-- Nguyễn Văn An (5 đơn)
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb01', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6', 'Nguyễn Văn An',   '0912345601', '123 Lê Lợi, Q.1, TP.HCM',       0, 30000, 'Giao giờ hành chính',        3, '2025-01-05 10:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb02', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6', 'Nguyễn Văn An',   '0912345601', '123 Lê Lợi, Q.1, TP.HCM',       1,     0, '',                           3, '2025-02-10 14:30:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb03', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6', 'Nguyễn Văn An',   '0912345601', '123 Lê Lợi, Q.1, TP.HCM',       3, 15000, '',                           2, '2025-03-05 09:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb04', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6', 'Nguyễn Văn An',   '0912345601', '123 Lê Lợi, Q.1, TP.HCM',       2, 10000, 'Giao cuối tuần',             1, '2025-03-20 14:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb05', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6', 'Nguyễn Văn An',   '0912345601', '123 Lê Lợi, Q.1, TP.HCM',       0, 30000, 'Ưu tiên giao sớm',           0, '2025-04-05 16:00:00'),
-- Trần Thị Hạnh (4 đơn)
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb06', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa7', 'Trần Thị Hạnh',   '0912345602', '456 Nguyễn Huệ, Q.1, TP.HCM',   2, 15000, 'Gọi trước khi giao',         3, '2025-01-15 09:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb07', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa7', 'Trần Thị Hạnh',   '0912345602', '456 Nguyễn Huệ, Q.1, TP.HCM',   0, 30000, 'Đóng gói kỹ',                1, '2025-03-01 10:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb08', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa7', 'Trần Thị Hạnh',   '0912345602', '789 CMT8, Q.3, TP.HCM',         0, 20000, '',                           0, '2025-03-15 08:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb09', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa7', 'Trần Thị Hạnh',   '0912345602', '456 Nguyễn Huệ, Q.1, TP.HCM',   1,     0, '',                           3, '2025-04-01 11:00:00'),
-- Lê Hoàng Cường (3 đơn)
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb10', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa8', 'Lê Hoàng Cường',  '0912345603', '789 CMT8, Q.3, TP.HCM',         3, 20000, '',                           2, '2025-02-01 08:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb11', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa8', 'Lê Hoàng Cường',  '0912345603', '10 Pasteur, Q.3, TP.HCM',        0, 25000, 'Hàng dễ vỡ',                 4, '2025-02-05 16:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb12', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa8', 'Lê Hoàng Cường',  '0912345603', '10 Pasteur, Q.3, TP.HCM',        1,     0, '',                           3, '2025-03-10 15:00:00'),
-- Phạm Minh Thư (2 đơn)
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb13', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa9', 'Phạm Minh Thư',   '0912345604', '10 Pasteur, Q.3, TP.HCM',        2, 10000, 'Giao trong buổi sáng',        3, '2025-02-20 10:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb14', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa9', 'Phạm Minh Thư',   '0912345604', '10 Pasteur, Q.3, TP.HCM',        0, 25000, '',                           0, '2025-04-10 14:00:00'),
-- Võ Quang Đức (2 đơn)
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb15', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa10', 'Võ Quang Đức',    '0912345605', '22 Hai Bà Trưng, Q.1, TP.HCM',  1,     0, '',                           3, '2025-01-20 11:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb16', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa10', 'Võ Quang Đức',    '0912345605', '22 Hai Bà Trưng, Q.1, TP.HCM',  3, 15000, '',                           1, '2025-03-25 09:00:00'),
-- Lý Thanh Sơn (2 đơn)
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb17', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa15', 'Lý Thanh Sơn',    '0912345610', '66 Trần Hưng Đạo, Q.1, TP.HCM', 0, 30000, 'Cẩn thận hàng dễ vỡ',        3, '2025-01-25 13:00:00'),
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb18', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa15', 'Lý Thanh Sơn',    '0912345610', '66 Trần Hưng Đạo, Q.1, TP.HCM', 2, 10000, '',                           0, '2025-04-15 10:00:00'),
-- Đoàn Mai Hương (1 đơn)
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb19', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa11', 'Đoàn Mai Hương',  '0912345606', '55 Võ Văn Tần, Q.3, TP.HCM',    3, 20000, '',                           3, '2025-02-15 10:00:00'),
-- Bùi Văn Khoa (1 đơn)
('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb20', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa12', 'Bùi Văn Khoa',    '0912345607', '88 Lý Tự Trọng, Q.1, TP.HCM',   1,     0, 'Giao tận nơi',               3, '2025-03-08 11:00:00');


-- ─── ORDER ITEMS (mỗi Order 1-3 items) ───
INSERT INTO "OrderItems" ("Id", "OrderId", "ProductId", "ProductName", "Quantity", "UnitPrice") VALUES
-- Order 01 (An): Áo thun x2 + Quần jean x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc01', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb01', 'dddddddd-dddd-4ddd-8ddd-dddddddddd01', 'Áo thun nam',          2, 150000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc02', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb01', 'dddddddd-dddd-4ddd-8ddd-dddddddddd02', 'Quần jean',            1, 350000),
-- Order 02 (An): Giày thể thao x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc03', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb02', 'dddddddd-dddd-4ddd-8ddd-dddddddddd03', 'Giày thể thao',        1, 890000),
-- Order 03 (An): Nón x3
('cccccccc-cccc-4ccc-8ccc-cccccccccc04', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb03', 'dddddddd-dddd-4ddd-8ddd-dddddddddd05', 'Nón lưỡi trai',        3,  95000),
-- Order 04 (An): Balo laptop x1 + Kính mát x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc05', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb04', 'dddddddd-dddd-4ddd-8ddd-dddddddddd06', 'Balo laptop',          1, 680000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc06', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb04', 'dddddddd-dddd-4ddd-8ddd-dddddddddd07', 'Kính mát',             1, 250000),
-- Order 05 (An): Áo thun x1 + Giày x1 + Túi xách x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc07', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb05', 'dddddddd-dddd-4ddd-8ddd-dddddddddd01', 'Áo thun nam',          1, 150000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc08', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb05', 'dddddddd-dddd-4ddd-8ddd-dddddddddd03', 'Giày thể thao',        1, 890000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc09', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb05', 'dddddddd-dddd-4ddd-8ddd-dddddddddd04', 'Túi xách nữ',          1, 520000),
-- Order 06 (Hạnh): Túi xách x1 + Nón x2
('cccccccc-cccc-4ccc-8ccc-cccccccccc10', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb06', 'dddddddd-dddd-4ddd-8ddd-dddddddddd04', 'Túi xách nữ',          1, 520000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc11', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb06', 'dddddddd-dddd-4ddd-8ddd-dddddddddd05', 'Nón lưỡi trai',        2,  95000),
-- Order 07 (Hạnh): Quần jean x2
('cccccccc-cccc-4ccc-8ccc-cccccccccc12', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb07', 'dddddddd-dddd-4ddd-8ddd-dddddddddd02', 'Quần jean',            2, 350000),
-- Order 08 (Hạnh): Áo thun x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc13', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb08', 'dddddddd-dddd-4ddd-8ddd-dddddddddd01', 'Áo thun nam',          1, 150000),
-- Order 09 (Hạnh): Đồng hồ x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc14', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb09', 'dddddddd-dddd-4ddd-8ddd-dddddddddd08', 'Đồng hồ thời trang',   1, 1200000),
-- Order 10 (Cường): Quần jean x2
('cccccccc-cccc-4ccc-8ccc-cccccccccc15', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb10', 'dddddddd-dddd-4ddd-8ddd-dddddddddd02', 'Quần jean',            2, 350000),
-- Order 11 (Cường): Giày x1 + Nón x1 - Cancelled
('cccccccc-cccc-4ccc-8ccc-cccccccccc16', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb11', 'dddddddd-dddd-4ddd-8ddd-dddddddddd03', 'Giày thể thao',        1, 890000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc17', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb11', 'dddddddd-dddd-4ddd-8ddd-dddddddddd05', 'Nón lưỡi trai',        1,  95000),
-- Order 12 (Cường): Áo thun x2 + Kính mát x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc18', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb12', 'dddddddd-dddd-4ddd-8ddd-dddddddddd01', 'Áo thun nam',          2, 150000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc19', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb12', 'dddddddd-dddd-4ddd-8ddd-dddddddddd07', 'Kính mát',             1, 250000),
-- Order 13 (Thư): Túi xách x1 + Áo thun x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc20', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb13', 'dddddddd-dddd-4ddd-8ddd-dddddddddd04', 'Túi xách nữ',          1, 520000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc21', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb13', 'dddddddd-dddd-4ddd-8ddd-dddddddddd01', 'Áo thun nam',          1, 150000),
-- Order 14 (Thư): Balo laptop x1  (đơn mới, chưa xử lý)
('cccccccc-cccc-4ccc-8ccc-cccccccccc22', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb14', 'dddddddd-dddd-4ddd-8ddd-dddddddddd06', 'Balo laptop',          1, 680000),
-- Order 15 (Đức): Đồng hồ x1 + Quần jean x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc23', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb15', 'dddddddd-dddd-4ddd-8ddd-dddddddddd08', 'Đồng hồ thời trang',   1, 1200000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc24', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb15', 'dddddddd-dddd-4ddd-8ddd-dddddddddd02', 'Quần jean',            1, 350000),
-- Order 16 (Đức): Giày x1 - Processing
('cccccccc-cccc-4ccc-8ccc-cccccccccc25', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb16', 'dddddddd-dddd-4ddd-8ddd-dddddddddd03', 'Giày thể thao',        1, 890000),
-- Order 17 (Sơn): Áo thun x3 + Nón x2
('cccccccc-cccc-4ccc-8ccc-cccccccccc26', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb17', 'dddddddd-dddd-4ddd-8ddd-dddddddddd01', 'Áo thun nam',          3, 150000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc27', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb17', 'dddddddd-dddd-4ddd-8ddd-dddddddddd05', 'Nón lưỡi trai',        2,  95000),
-- Order 18 (Sơn): Kính mát x2 - New
('cccccccc-cccc-4ccc-8ccc-cccccccccc28', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb18', 'dddddddd-dddd-4ddd-8ddd-dddddddddd07', 'Kính mát',             2, 250000),
-- Order 19 (Hương): Túi xách x1 + Nón x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc29', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb19', 'dddddddd-dddd-4ddd-8ddd-dddddddddd04', 'Túi xách nữ',          1, 520000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc30', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb19', 'dddddddd-dddd-4ddd-8ddd-dddddddddd05', 'Nón lưỡi trai',        1,  95000),
-- Order 20 (Khoa): Balo x1 + Đồng hồ x1
('cccccccc-cccc-4ccc-8ccc-cccccccccc31', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb20', 'dddddddd-dddd-4ddd-8ddd-dddddddddd06', 'Balo laptop',          1, 680000),
('cccccccc-cccc-4ccc-8ccc-cccccccccc32', 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb20', 'dddddddd-dddd-4ddd-8ddd-dddddddddd08', 'Đồng hồ thời trang',   1, 1200000);


-- =========================================================================================
-- ✅ TỔNG KẾT SEED DATA
-- =========================================================================================
-- AuthDb:     4 Roles, 20 Permissions, 44 RolePermissions, 20 Users
-- CustomerDb: 20 Customers (ID khớp Users)
-- OrderDb:    20 Orders, 32 OrderItems
-- Mật khẩu:   Pass@123  (bcrypt)
-- TotalSpent của Customer đã tính sẵn từ các đơn Completed
-- 2 user bị Blocked/Inactive: staff03 (IsActive=false), ngothiphuong (Status=Blocked)
-- =========================================================================================
