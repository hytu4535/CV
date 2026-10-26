VIẾT LẠI project ShopMvc (.NET 10, EF Core 10, SQL Server, Code First) theo đúng thiết kế dưới đây. Code hiện tại sai logic: được phép xóa/viết lại model, controller, view, thư mục Migrations. Ngoại lệ cho lần này: ĐƯỢC chạy `dotnet ef database drop -f`, xóa thư mục Migrations, `dotnet ef migrations add InitialCreate`, `dotnet ef database update`. Không dùng ASP.NET Core Identity, Repository, AutoMapper. Controller dùng thẳng ShopDbContext. Mọi chữ hiển thị bằng tiếng Việt, lỗi validation đỏ ngay dưới từng ô, form có novalidate, trang form nạp _ValidationScriptsPartial. Không báo "xong" nếu chưa chạy hết phần KIỂM CHỨNG ở cuối và in kết quả thật.

## 1. HAI HỆ ĐĂNG NHẬP TÁCH BIỆT HOÀN TOÀN
Hai bảng người dùng riêng, hai cookie scheme riêng, hai trang đăng nhập riêng:
| | Khách hàng (storefront) | Quản trị (admin) |
|---|---|---|
| Bảng | Customers | AdminUsers |
| Scheme / tên cookie | "CustomerCookie" / ".Shop.Customer" | "AdminCookie" / ".Shop.Admin" |
| Đăng nhập | /account/login | /admin/login |
| Đăng xuất (POST) | /account/logout | /admin/logout |
| Chưa đăng nhập vào trang bảo vệ | chuyển về /account/login | chuyển về /admin/login |
Cookie của bên này KHÔNG có tác dụng bên kia. Tài khoản khách không đăng nhập được ở /admin/login và ngược lại. Mọi thứ dưới /admin dùng [Authorize(AuthenticationSchemes="AdminCookie")]; trang tài khoản khách dùng "CustomerCookie". Cho phép đăng nhập cả hai cùng lúc ở hai tab. Trang /admin/login có giao diện riêng (không dùng layout storefront). Chỉ chấp nhận returnUrl cục bộ.
Quy tắc đăng nhập (cả hai bên): băm bằng PasswordHasher; sai thông tin báo chung "Email hoặc mật khẩu không đúng"; sai 5 lần liên tiếp thì khóa 15 phút (FailedLoginCount, LockoutEnd) với thông báo rõ; tài khoản IsActive=false không đăng nhập được; ghi LastLoginAt.

## 2. BẢNG DỮ LIỆU (Migration tự tạo)
- Customers: Id, FullName, Email (duy nhất), PasswordHash, Phone, IsActive, FailedLoginCount, LockoutEnd, CreatedAt, LastLoginAt
- AdminUsers: Id, FullName, Email (duy nhất), PasswordHash, RoleId, IsActive, FailedLoginCount, LockoutEnd, CreatedAt, LastLoginAt
- Roles: Id, Name (duy nhất), Description, IsSystem
- Permissions: Id, Code (duy nhất, ví dụ "Product.Edit"), Name (tiếng Việt), Group
- RolePermissions: khóa kép (RoleId, PermissionId)
- Categories: Id, Name, Slug (duy nhất), Description, ParentId (nullable, tối đa 2 cấp), DisplayOrder, IsActive
- Products: Id, Sku (duy nhất), Name, Slug (duy nhất), CategoryId, Price decimal(18,0), SalePrice (nullable, phải nhỏ hơn Price), Stock, ImageUrl, Description, IsActive, IsDeleted, CreatedAt, UpdatedAt, RowVersion (concurrency token)
- AuditLogs: Id, AdminUserId (nullable), AdminName, Action, EntityName, EntityId, Detail, IpAddress, CreatedAt
Quan hệ xóa: Category->Product và Role->AdminUser là Restrict; RolePermission cascade.

## 3. PHÂN QUYỀN LINH ĐỘNG
Quyền (Permission) lưu trong database, mã dạng: Dashboard.View; Category.View/Create/Edit/Delete; Product.View/Create/Edit/Delete; AdminUser.View/Create/Edit/Delete/ResetPassword; Customer.View/Lock; Role.View/Manage; AuditLog.View.
[HasPermission("Mã")] là AuthorizationFilter: chưa đăng nhập AdminCookie thì chuyển /admin/login, đăng nhập mà thiếu quyền thì trang 403 tiếng Việt đẹp, TRUY VẤN quyền từ database mỗi request nên đổi quyền có hiệu lực ngay. Sidebar, nút Thêm/Sửa/Xóa chỉ hiện khi có quyền (helper dùng trong view), nhưng server vẫn kiểm tra riêng ở từng action.
Vai trò: "Super Admin" (IsSystem=true, luôn có mọi quyền kể cả quyền thêm mới sau này, không sửa/xóa được), "Quản lý nội dung" (Dashboard, Category, Product đầy đủ), "Nhân viên" (chỉ xem Dashboard, Category.View, Product.View). Trang Phân quyền: ma trận checkbox nhóm theo Group, có nút chọn hết theo nhóm, lưu thay toàn bộ tập quyền của vai trò, ghi AuditLog.

## 4. TRANG ADMIN (Area Admin, URL dưới /admin)
Layout riêng: sidebar (theo quyền) + thanh trên (tên admin, vai trò, Đổi mật khẩu, Đăng xuất) + breadcrumb + thông báo flash (TempData xanh/đỏ). Bảng có thanh tìm kiếm, bộ lọc, sắp xếp, phân trang (tự viết PagedList<T>, chọn 10/20/50 dòng, giữ nguyên bộ lọc trên URL), badge trạng thái, thông báo "Chưa có dữ liệu" khi rỗng. Xóa/khóa bằng modal Bootstrap xác nhận (POST + chống CSRF), không dùng confirm() của trình duyệt.
- /admin Dashboard: số sản phẩm (đang bán/ẩn), sản phẩm sắp hết hàng (Stock<=5, liệt kê), số danh mục, số tài khoản quản trị, số khách hàng, 10 hoạt động gần nhất từ AuditLogs.
- /admin/categories: CRUD. Slug tự sinh từ tên (bỏ dấu tiếng Việt), kiểm tra trùng. Chọn danh mục cha (không được chọn chính nó hay con của nó). Không xóa nếu còn sản phẩm hoặc danh mục con. Bật/tắt IsActive; danh mục tắt thì ẩn ở storefront.
- /admin/products: CRUD. Sku bắt buộc duy nhất (regex ^[A-Z0-9\-]{3,30}$), tên 3-150, giá 1.000-1.000.000.000, SalePrice nhỏ hơn Price (kiểm tra cả client lẫn server), tồn kho 0-100.000. Ảnh: input file (jpg/png/webp, tối đa 2MB, kiểm tra phía client và server), lưu wwwroot/uploads/products/<guid>.<đuôi>, xem trước khi chọn, giữ ảnh cũ nếu không chọn mới, xóa file cũ khi thay. "Xóa" là xóa mềm (IsDeleted=true, có thể khôi phục ở bộ lọc "Đã xóa"); có nút Ẩn/Hiện bán. Bắt DbUpdateConcurrencyException và báo "Sản phẩm vừa được người khác chỉnh sửa, vui lòng tải lại". Lọc theo từ khóa, danh mục, trạng thái (đang bán/ẩn/đã xóa); sắp xếp theo tên/giá/tồn kho/ngày tạo.
- /admin/admin-users (Quản lý tài khoản quản trị): CRUD, gán vai trò, khóa/mở khóa (IsActive), đặt lại mật khẩu (admin nhập mật khẩu mới), mở khóa lockout. Luật: không tự xóa/khóa chính mình, không xóa hay hạ vai trò Super Admin cuối cùng còn hoạt động, dùng ViewModel (không lộ PasswordHash), email không trùng. Mật khẩu: tối thiểu 8 ký tự có chữ hoa, chữ thường, số.
- /admin/customers: xem danh sách và chi tiết, tìm kiếm, khóa/mở khóa tài khoản khách (không sửa mật khẩu khách).
- /admin/roles: CRUD vai trò, trang phân quyền, không sửa/xóa vai trò IsSystem, không xóa vai trò còn tài khoản.
- /admin/audit-logs: danh sách phân trang có lọc theo người thực hiện, hành động, khoảng ngày.
- /admin/account/change-password: tự đổi mật khẩu (nhập mật khẩu cũ).
Ghi AuditLog cho: đăng nhập/đăng xuất admin, mọi thêm/sửa/xóa/khóa, đổi quyền (ghi rõ thêm/bớt quyền nào).

## 5. TRANG KHÁCH (storefront, ngoài Area)
/ (sản phẩm đang bán và chưa xóa, phân trang 8/trang, lọc danh mục và tìm kiếm, sắp xếp giá, hiện giá gạch ngang + % giảm khi có SalePrice), /products/{slug}, /account/register, /account/login, /account/logout, /account/profile (sửa họ tên/điện thoại, đổi mật khẩu). Ảnh trống dùng ảnh mặc định bằng SVG nhúng. Giá định dạng "N0" vi-VN + " ₫". Hết hàng thì hiện nhãn "Hết hàng".

## 6. DỮ LIỆU MẪU (DbSeeder, chỉ chạy khi Roles trống, bọc try/catch)
AdminUsers: admin@shop.com/Admin@123 (Super Admin), content@shop.com/Content@123 (Quản lý nội dung), staff@shop.com/Staff@123 (Nhân viên). Customers: khach@shop.com/Khach@123 và 4 khách khác. 6 danh mục (một vài có danh mục con), 24 sản phẩm phụ kiện (ví, túi, balo, thắt lưng, mũ...), một số có SalePrice, một số Stock thấp.

## KIỂM CHỨNG BẮT BUỘC (curl/Invoke-WebRequest có giữ cookie, in kết quả từng dòng)
1. `dotnet build` 0 lỗi; migration tạo đủ bảng.
2. GET /admin khi chưa đăng nhập -> 302 tới /admin/login (KHÔNG phải /account/login).
3. GET /account/profile khi chưa đăng nhập -> 302 tới /account/login.
4. Đăng nhập khach@shop.com ở /account/login rồi GET /admin -> vẫn 302 tới /admin/login.
5. Đăng nhập khach@shop.com ở /admin/login -> thất bại. Đăng nhập admin@shop.com ở /account/login -> thất bại.
6. Đăng nhập admin@shop.com ở /admin/login rồi GET /admin, /admin/products, /admin/admin-users, /admin/roles -> 200.
7. Đăng nhập staff@shop.com ở /admin/login: GET /admin/products -> 200; POST xóa sản phẩm -> 403.
8. Đổi quyền của vai trò "Nhân viên" (thêm Product.Delete) bằng admin, rồi staff POST xóa lại -> được, KHÔNG cần đăng nhập lại.
9. Đăng nhập sai 5 lần -> bị khóa tạm thời với thông báo tiếng Việt.
10. POST form sản phẩm với SalePrice >= Price và Sku sai định dạng -> có lỗi tiếng Việt đúng ô, không lưu DB.
Cuối cùng liệt kê file đã tạo/sửa, mỗi file một câu. Nếu một phép thử thất bại, sửa rồi chạy lại từ đầu, không được bỏ qua.