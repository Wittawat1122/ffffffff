using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using fffff.Models.Db;

namespace fffff.Controllers
{
    public class AdminController : Controller
    {
        private readonly FinalProjectContext _context;

        public AdminController(FinalProjectContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "admin";
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // Admin Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            // Statistics for dashboard
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.PendingShipments = await _context.Shipments.CountAsync(s => s.Status == "Pending");

            // Recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(recentOrders);
        }

        // 2.2 ระบบบริหารคลังสินค้า - Inventory Management
        public async Task<IActionResult> Inventory()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var products = await _context.Products
                .Include(p => p.ProductSizes)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(string brand, string model, string fieldType, decimal price, string? imageUrl, int[] sizes, int[] stocks)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var product = new Product
            {
                Brand = brand,
                Model = model,
                FieldType = fieldType,
                Price = price,
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Add sizes
            for (int i = 0; i < sizes.Length; i++)
            {
                var productSize = new ProductSize
                {
                    ProductId = product.ProductId,
                    Size = sizes[i],
                    Stock = stocks[i]
                };
                _context.ProductSizes.Add(productSize);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Inventory");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var product = await _context.Products
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int productId, string brand, string model, string fieldType, decimal price, string? imageUrl)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            product.Brand = brand;
            product.Model = model;
            product.FieldType = fieldType;
            product.Price = price;
            product.ImageUrl = imageUrl;

            await _context.SaveChangesAsync();
            return RedirectToAction("Inventory");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int sizeId, int newStock)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var productSize = await _context.ProductSizes.FindAsync(sizeId);
            if (productSize != null)
            {
                productSize.Stock = newStock;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Inventory");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Inventory");
        }

        // 2.1 ระบบจัดการขนส่ง - Shipping Management
        public async Task<IActionResult> Shipments()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var shipments = await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o!.User)
                .ToListAsync();

            return View(shipments);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateShipment(int shipmentId, string status, string trackingNumber)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var shipment = await _context.Shipments.FindAsync(shipmentId);
            if (shipment != null)
            {
                shipment.Status = status;
                shipment.TrackingNumber = trackingNumber;

                // Update order status if shipment is delivered
                if (status == "Delivered")
                {
                    var order = await _context.Orders.FindAsync(shipment.OrderId);
                    if (order != null)
                        order.Status = "Completed";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Shipments");
        }

        // 2.3 ระบบจัดการโปรโมชั่น - Promotion Management
        public async Task<IActionResult> Promotions()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var promotions = await _context.Promotions
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            var coupons = await _context.Coupons
                .OrderByDescending(c => c.CouponId)
                .ToListAsync();

            ViewBag.Coupons = coupons;
            return View(promotions);
        }

        [HttpPost]
        public async Task<IActionResult> AddPromotion(string title, DateOnly startDate, DateOnly endDate, int discountPercent)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var promotion = new Promotion
            {
                Title = title,
                StartDate = startDate,
                EndDate = endDate,
                DiscountPercent = discountPercent
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return RedirectToAction("Promotions");
        }

        [HttpPost]
        public async Task<IActionResult> AddCoupon(string code, int discountPercent, DateOnly expiryDate)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var coupon = new Coupon
            {
                Code = code,
                DiscountPercent = discountPercent,
                ExpiryDate = expiryDate
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return RedirectToAction("Promotions");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Promotions");
        }

        // 2.4 ระบบรายงานรายรับ-รายจ่าย - Financial Reports
        public async Task<IActionResult> Reports()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            // Daily sales
            var today = DateOnly.FromDateTime(DateTime.Now);
            var dailySales = await _context.Orders
                .Where(o => o.Status == "Completed" && DateOnly.FromDateTime(o.CreatedAt!.Value) == today)
                .SumAsync(o => o.TotalPrice);

            // Monthly sales
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var monthlySales = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt >= monthStart)
                .SumAsync(o => o.TotalPrice);

            // Total sales
            var totalSales = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalPrice);

            // Order counts
            var totalOrders = await _context.Orders.CountAsync();
            var completedOrders = await _context.Orders.CountAsync(o => o.Status == "Completed");
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");

            ViewBag.DailySales = dailySales ?? 0;
            ViewBag.MonthlySales = monthlySales ?? 0;
            ViewBag.TotalSales = totalSales ?? 0;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.PendingOrders = pendingOrders;

            // Recent completed orders for the report table
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(20)
                .ToListAsync();

            return View(recentOrders);
        }

        // Order Management
        public async Task<IActionResult> Orders()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                .Include(o => o.Shipments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Orders");
        }
    }
}
