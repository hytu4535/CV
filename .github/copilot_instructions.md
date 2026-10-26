# Quy ước cho dự án ShopMvc (bài tập ASP.NET Core MVC, có vấn đáp)

## Bối cảnh
- ASP.NET Core MVC, .NET 10, EF Core 10 + SQL Server, làm theo hướng Code First (Add-Migration sinh bảng).
- Đây là bài tập chấm điểm có vấn đáp: sinh viên phải giải thích được từng dòng code. Vì vậy code phải NGẮN, DỄ ĐỌC, KHÔNG DƯ THỪA.

## Nguyên tắc bắt buộc
- Trả lời và viết comment bằng tiếng Việt. Tên class, biến, bảng bằng tiếng Anh.
- Mọi thông báo hiển thị cho người dùng (lỗi validation, thông báo thành công/thất bại) phải bằng tiếng Việt. Không để sót câu tiếng Anh.
- Lưu file UTF-8. Sau khi viết file có tiếng Việt thì đọc lại để chắc không bị sai chữ hay lẫn ngôn ngữ khác.
- Không thêm package ngoài EF Core (SqlServer, Design, Tools). Không dùng AutoMapper, MediatR, Repository/Service layer, ASP.NET Core Identity. Controller dùng trực tiếp ShopDbContext.
- Không tạo class, interface, method, file, view không được dùng. Không để using thừa, biến thừa, code bị comment bỏ đi.
- Mỗi class/method có tên rõ nghĩa. Comment ngắn gọn giải thích mục đích, không giải thích điều hiển nhiên.
- Không sửa Program.cs ngoài phần cần thiết cho yêu cầu hiện tại. Giữ nguyên các dòng cấu hình static assets của template.
- Không dùng regex có `\p{L}` cho validation (JavaScript không hiểu). Dùng khoảng ký tự `a-zA-Z\u00C0-\u1EF9` nếu cần.
- Không tự chạy lệnh Add-Migration hoặc Update-Database. Sinh viên tự chạy hai lệnh này và giải thích được chúng.

## Cấu trúc thư mục
- Data/ShopDbContext.cs, Data/DbSeeder.cs
- Models/ (các class bảng), ViewModels/ (class cho form và phân trang)
- Controllers/ (phần người dùng), Areas/Admin/Controllers/ và Areas/Admin/Views/ (phần quản trị)
- Helpers/ (PagedList, HasPermissionAttribute...)

## Xác thực và phân quyền2
- Dùng Cookie Authentication tự viết. Băm mật khẩu bằng PasswordHasher<User>, không lưu mật khẩu thô.
- Phân quyền linh động: quyền lưu trong database (bảng Roles, Permissions, RolePermissions), kiểm tra bằng attribute [HasPermission("Mã.Quyền")] đọc quyền từ database ở MỖI request để thay đổi có hiệu lực ngay, không cần đăng nhập lại.

## Cách làm việc
- Làm đúng phạm vi của prompt hiện tại, không làm lan sang phần khác.
- Trước khi sửa file có sẵn, đọc nội dung hiện tại của file đó.
- Sau mỗi giai đoạn phải chạy `dotnet build` và báo kết quả thật. Không được nói "đã xong" nếu chưa build thành công.
- Cuối mỗi giai đoạn liệt kê file đã tạo/sửa và mỗi file làm gì trong một câu.