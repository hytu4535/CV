using ShopUI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ShopUI.Data
{
    public static class OrderData
    {
        private static readonly Random rng = new Random();
        private static readonly CultureInfo vn = new CultureInfo("vi-VN");

        private static readonly string[] customerNames = new[]
        {
            "Nguyễn Văn An", "Trần Thị Bình", "Lê Hoàng Cường", "Phạm Diệu Linh",
            "Hoàng Văn Đức", "Ngô Thị Em", "Bùi Văn Phú", "Đỗ Thị Quyên",
            "Vũ Minh Hải", "Hà Thị Hòa", "Đặng Quang Hưng", "Lâm Văn Khang",
            "Trần Thị Lan", "Phạm Văn Minh", "Nguyễn Thị Nga"
        };

        private static readonly string[] phonePrefixes = new[] { "090", "091", "092", "093", "094", "095", "096", "097", "098", "032", "033", "034", "035", "036", "037", "038", "039", "081", "082", "083", "084", "085", "086", "088", "089", "096", "097", "098", "099" };

        private static string RandomPhone()
        {
            var prefix = phonePrefixes[rng.Next(phonePrefixes.Length)];
            var number = rng.Next(1000000, 9999999).ToString("D7");
            return $"{prefix}{number}";
        }

        private static string RandomAddress()
        {
            var streets = new[] { "Ngõ 123 Trần Hưng Đạo", "Đường Láng Hạ", "Phố Huế", "Kp. Trung Yên", "Ấp 4, Xã Tân Lập" };
            var wards = new[] { "Phường Trung Liệt", "Xã Hòa Bình", "Phường Thanh Xuân Trung" };
            var districts = new[] { "Quận Hai Bà Trưng", "Quận Thanh Xuân", "Hà Đông" };
            var cities = new[] { "Hà Nội", "TP Hồ Chí Minh", "Đà Nẵng", "Hải Phòng", "Cần Thơ" };
            return $"{streets[rng.Next(streets.Length)]}, {wards[rng.Next(wards.Length)]}, {districts[rng.Next(districts.Length)]}, {cities[rng.Next(cities.Length)]}";
        }

        public static readonly List<Order> All = GenerateOrders();

        private static List<Order> GenerateOrders()
        {
            var orders = new List<Order>();
            var today = DateTime.Today;

            for (int i = 1; i <= 15; i++)
            {
                var statusIndex = rng.Next(4); // 0-3
                var status = (OrderStatus)statusIndex;
                var daysAgo = rng.Next(0, 30);
                var createdAt = today.AddDays(-daysAgo).AddHours(rng.Next(0, 24)).AddMinutes(rng.Next(0, 60));

                var customerName = customerNames[rng.Next(customerNames.Length)];
                var phone = RandomPhone();
                var address = RandomAddress();

                // Pick 1-3 random products
                var itemCount = rng.Next(1, 4);
                var items = new List<CartItem>();
                var usedIds = new HashSet<int>();
                for (int j = 0; j < itemCount; j++)
                {
                    Product? product;
                    do
                    {
                        product = ProductData.All[rng.Next(ProductData.All.Count)];
                    } while (usedIds.Contains(product.Id));
                    usedIds.Add(product.Id);

                    var quantity = rng.Next(1, 5);
                    items.Add(new CartItem { Product = product, Quantity = quantity });
                }

                var order = new Order
                {
                    Id = i,
                    CustomerName = customerName,
                    Phone = phone,
                    Address = address,
                    CreatedAt = createdAt,
                    Status = status,
                    Items = items
                };

                orders.Add(order);
            }

            return orders;
        }

        public static Order? GetById(int id) => All.FirstOrDefault(o => o.Id == id);
    }
}