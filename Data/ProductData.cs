using ShopUI.Models;

namespace ShopUI.Data;

public static class ProductData
{
    // Ảnh Unsplash: id ảnh + kích thước 4:3 cho khớp giao diện
    private static string Img(string photoId) =>
        $"https://images.unsplash.com/{photoId}?auto=format&fit=crop&w=600&h=450&q=80";

    public static readonly List<Product> All = new()
    {
        new() { Id = 1,  Name = "Giày sneaker trắng basic", Category = "Giày", Price = 890000, OldPrice = 1190000, ImageUrl = Img("photo-1595950653106-6c9ebd614d3a"), Rating = 4.8, Description = "Giày sneaker phong cách tối giản, đế cao su êm ái, phù hợp đi học, đi làm và dạo phố." },
        new() { Id = 2,  Name = "Giày chạy bộ Runner Pro", Category = "Giày", Price = 1450000, OldPrice = 1790000, ImageUrl = Img("photo-1542291026-7eec264c27ff"), Rating = 4.6, Description = "Thiết kế siêu nhẹ, thoáng khí, đệm êm giúp bạn chạy bền hơn." },
        new() { Id = 3,  Name = "Giày da nam công sở", Category = "Giày", Price = 1290000, ImageUrl = Img("photo-1614252369475-531eba835eb1"), Rating = 4.5, Description = "Da bò thật, đường may tinh tế, mang lại vẻ lịch lãm cho quý ông." },
        new() { Id = 4,  Name = "Áo thun cotton unisex", Category = "Áo", Price = 249000, OldPrice = 320000, ImageUrl = Img("photo-1521572163474-6864f9cf17ab"), Rating = 4.7, Description = "Chất liệu cotton 100% mềm mịn, thấm hút mồ hôi tốt, form rộng thoải mái." },
        new() { Id = 5,  Name = "Áo sơ mi trắng oxford", Category = "Áo", Price = 429000, ImageUrl = Img("photo-1596755094514-f87e34085b2c"), Rating = 4.4, Description = "Sơ mi oxford dày dặn, ít nhăn, dễ phối đồ với quần jeans hoặc quần tây." },
        new() { Id = 6,  Name = "Áo hoodie nỉ bông", Category = "Áo", Price = 590000, OldPrice = 750000, ImageUrl = Img("photo-1556821840-3a63f95609a7"), Rating = 4.9, Description = "Nỉ bông dày ấm áp, mũ 2 lớp, túi kangaroo tiện lợi." },
        new() { Id = 7,  Name = "Áo khoác bomber", Category = "Áo", Price = 690000, ImageUrl = Img("photo-1551028719-00167b16eac5"), Rating = 4.3, Description = "Áo khoác bomber chống gió nhẹ, phong cách trẻ trung, năng động." },
        new() { Id = 8,  Name = "Túi tote canvas", Category = "Túi", Price = 189000, ImageUrl = Img("photo-1590874103328-eac38a683ce7"), Rating = 4.5, Description = "Túi vải canvas dày, đựng vừa laptop 14 inch và đồ dùng hằng ngày." },
        new() { Id = 9,  Name = "Balo laptop chống nước", Category = "Túi", Price = 550000, OldPrice = 690000, ImageUrl = Img("photo-1553062407-98eeb64c6a62"), Rating = 4.8, Description = "Ngăn laptop có đệm chống sốc, vải chống nước, nhiều ngăn tiện dụng." },
        new() { Id = 10, Name = "Túi đeo chéo mini", Category = "Túi", Price = 320000, ImageUrl = Img("photo-1584917865442-de89df76afd3"), Rating = 4.2, Description = "Nhỏ gọn, đựng vừa điện thoại, ví và chìa khóa khi ra ngoài." },
        new() { Id = 11, Name = "Đồng hồ nam dây da", Category = "Đồng hồ", Price = 1890000, OldPrice = 2390000, ImageUrl = Img("photo-1524592094714-0f0654e20314"), Rating = 4.7, Description = "Mặt kính sapphire, chống nước 3ATM, dây da thật cao cấp." },
        new() { Id = 12, Name = "Đồng hồ thể thao điện tử", Category = "Đồng hồ", Price = 790000, ImageUrl = Img("photo-1523275335684-37898b6baf30"), Rating = 4.1, Description = "Đèn nền LED, bấm giờ, báo thức, chống nước 5ATM." },
    };

    public static Product? GetById(int id) => All.FirstOrDefault(p => p.Id == id);
}