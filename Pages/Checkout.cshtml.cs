using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Data;
using ShopUI.Models;
using ShopUI;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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

        public decimal SubTotal { get; private set; }
        public decimal Shipping => SubTotal >= 500000 ? 0 : 30000;
        public decimal GrandTotal => SubTotal + Shipping;

        public class CheckoutInputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
            [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
            [RegularExpression(@"^(0|\+84)(3|5|7|8|9)\d{8}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ.")]
            [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
            public string Phone { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [RegularExpression(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", ErrorMessage = "Email không hợp lệ.")]
            [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
            [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự.")]
            public string Address { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng chọn tỉnh / thành phố.")]
            public string Province { get; set; } = string.Empty;

            public string? District { get; set; }

            [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
            public string? Note { get; set; }

            // Payment method: we can keep as string or enum, but we'll use string for simplicity
            [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
            [RegularExpression(@"^(cod|bank|momo)$", ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
            public string PaymentMethod { get; set; } = "cod";
        }

        public void OnGet()
        {
            LoadCartTotals();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            LoadCartTotals(cartItems);
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!cartItems.Any())
            {
                TempData["Msg"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToPage("/Cart");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var createdOrderId = 0;
            try
            {
                var products = new Dictionary<int, Product>();
                foreach (var cartItem in cartItems)
                {
                    var product = await _context.Products.FindAsync(cartItem.Product.Id);
                    if (product == null || product.Stock < cartItem.Quantity)
                    {
                        ModelState.AddModelError(string.Empty, $"Sản phẩm '{cartItem.Product.Name}' không đủ tồn kho.");
                        return Page();
                    }
                    products[product.Id] = product;
                }

                Customer? customer = null;
                var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (User.Identity?.IsAuthenticated == true && int.TryParse(customerIdClaim, out var customerId))
                {
                    customer = await _context.Customers.FindAsync(customerId);
                }

                customer ??= await _context.Customers.FirstOrDefaultAsync(c => c.Phone == Input.Phone);
                if (customer == null)
                {
                    customer = new Customer { FullName = Input.FullName, Email = Input.Email, Phone = Input.Phone, Address = Input.Address };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    customer.FullName = Input.FullName;
                    customer.Email = Input.Email;
                    customer.Address = Input.Address;
                }

                var order = new Order
                {
                    CustomerId = customer.Id,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    TotalAmount = GrandTotal,
                    PaymentMethod = Input.PaymentMethod,
                    Province = Input.Province,
                    District = Input.District,
                    Note = Input.Note
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                createdOrderId = order.Id;

                foreach (var cartItem in cartItems)
                {
                    var product = products[cartItem.Product.Id];
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        UnitPrice = product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);
                    product.Stock -= cartItem.Quantity;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Không thể lưu đơn hàng. Vui lòng thử lại.");
                return Page();
            }

            // Clear cart
            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.SetInt32("LastOrderId", createdOrderId);

            return RedirectToPage("/OrderSuccess", new { id = createdOrderId });
        }

        private void LoadCartTotals(List<CartItem>? items = null)
        {
            var cartItems = items ?? HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            SubTotal = cartItems.Sum(item => item.Total);
        }
    }
}