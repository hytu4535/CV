# Lệnh demo ShopMvc

Chạy tại thư mục gốc solution:

```powershell
$connection = "Server=localhost;Database=ShopMvcDB;Trusted_Connection=True;TrustServerCertificate=True;"

dotnet ef database drop -f --project .\ShopMvc.csproj
dotnet ef database update --project .\ShopMvc.csproj
dotnet run --project .\ShopMvc.csproj

dotnet ef migrations add TenMigrationMoi --project .\ShopMvc.csproj
dotnet ef dbcontext scaffold $connection Microsoft.EntityFrameworkCore.SqlServer --project .\DbFirstDemo\DbFirstDemo.csproj --startup-project .\DbFirstDemo\DbFirstDemo.csproj --output-dir Models --context ShopDbFirstContext --no-onconfiguring --force

dotnet build .\ShopMvc.slnx
dotnet run --project .\DbFirstDemo\DbFirstDemo.csproj
```

Nếu chỉ xóa một số bảng mà còn `__EFMigrationsHistory`, phải xóa cả bảng này hoặc drop toàn bộ database trước khi update lại.
