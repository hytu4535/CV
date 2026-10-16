using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Data;
using ShopUI.Models;
using ShopUI;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace ShopUI.Pages
{
    public class CheckoutModel : PageModel
    {
        private const string CartSessionKey = "Cart";
        private readonly ShopDbContext _context;

        public CheckoutModel(ShopDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CheckoutInputModel Input { get; set; } = new();

        public class CheckoutInputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
            [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
            [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
            public string Phone { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
            [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự.")]
            public string Address { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng chọn tỉnh / thành phố.")]
            public string Province { get; set; } = string.Empty;

            public string District { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
            public string Note { get; set; } = string.Empty;

            // Payment method: we can keep as string or enum, but we'll use string for simplicity
            [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
            public string PaymentMethod { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Get cart from session
            var cartItems = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            if (!cartItems.Any())
            {
                TempData["Msg"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToPage("/Cart");
            }

            // Create or get customer from input
            var customer = _context.Customers.FirstOrDefault(c => c.Phone == Input.Phone);
            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = Input.FullName,
                    Email = Input.Email,
                    Phone = Input.Phone,
                    Address = Input.Address
                };
                _context.Customers.Add(customer);
                _context.SaveChanges(); // Save to get Id
            }

            // Calculate total amount
            decimal totalAmount = cartItems.Sum(item => item.Total);

            // Create order
            var order = new Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount
            };
            _context.Orders.Add(order);
            _context.SaveChanges(); // Save to get OrderId

            // Create order details and reduce stock
            foreach (var cartItem in cartItems)
            {
                var product = _context.Products.Find(cartItem.Product.Id);
                if (product != null)
                {
                    // Create order detail
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        UnitPrice = product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // Reduce stock
                    if (product.Stock >= cartItem.Quantity)
                    {
                        product.Stock -= cartItem.Quantity;
                    }
                    else
                    {
                        // Not enough stock; we could show an error, but for simplicity we'll just reduce to zero
                        product.Stock = 0;
                    }
                }
            }

            _context.SaveChanges();

            // Clear cart
            HttpContext.Session.Remove(CartSessionKey);

            TempData["Msg"] = "Đặt hàng thành công! Cảm ơn bạn đã mua sắm tại MyShop 🎉";
            return RedirectToPage("/Index");
        }
    }
}