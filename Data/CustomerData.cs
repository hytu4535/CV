using ShopUI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ShopUI.Data
{
    public static class CustomerData
    {
        private static readonly Random rng = new Random();
        private static readonly CultureInfo vn = new CultureInfo("vi-VN");

        private static readonly string[] firstNames = new[] { "An", "Bình", "Cường", "Dương", "Em", "Phú", "Quân", "Hải", "Hoàng", "Hùng", "Khánh", "Linh", "Mai", "Nam", "Phúc", "Quỳnh", " Sơn", "Tài", "Thảo", "Tuệ", "Ủi", "Vinh" };
        private static readonly string[] lastNames = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý" };

        private static readonly string[] cities = new[] { "Hà Nội", "TP Hồ Chí Minh", "Đà Nẵng", "Hải Phòng", "Cần Thơ", "Huế", "Nha Trang", "Đà Lạt", "Vũng Tàu", "Quảng Ninh" };

        private static string RandomPhone()
        {
            var prefixes = new[] { "090", "091", "092", "093", "094", "095", "096", "097", "098", "032", "033", "034", "035", "036", "037", "038", "039", "081", "082", "083", "084", "085", "086", "088", "089", "096", "097", "098", "099" };
            var prefix = prefixes[rng.Next(prefixes.Length)];
            var number = rng.Next(1000000, 9999999).ToString("D7");
            return $"{prefix}{number}";
        }

        public static readonly List<Customer> All = GenerateCustomers();

        private static List<Customer> GenerateCustomers()
        {
            var customers = new List<Customer>();
            var today = DateTime.Today;

            for (int i = 1; i <= 10; i++)
            {
                var lastName = lastNames[rng.Next(lastNames.Length)];
                var firstName = firstNames[rng.Next(firstNames.Length)];
                var name = $"{lastName} {firstName}";
                var email = $"{firstName.ToLower()}{lastName.ToLower()}@example.com";
                var phone = RandomPhone();
                var city = cities[rng.Next(cities.Length)];
                var joinedAt = today.AddDays(-rng.Next(30, 730)); // last 2 years
                var ordersCount = rng.Next(0, 10);
                var totalSpent = ordersCount * rng.Next(500000, 3000000); // rough

                customers.Add(new Customer
                {
                    Id = i,
                    Name = name,
                    Email = email,
                    Phone = phone,
                    City = city,
                    JoinedAt = joinedAt,
                    OrdersCount = ordersCount,
                    TotalSpent = totalSpent
                });
            }

            return customers;
        }

        public static Customer? GetById(int id) => All.FirstOrDefault(c => c.Id == id);
    }
}