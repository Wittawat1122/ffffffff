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
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            // Statistics for dashboard
            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.PendingShipments = _context.Shipments.Count(s => s.Status == "Pending");

            // Recent orders
            var recentOrders = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToList();

            return View(recentOrders);
        }

        // 2.2 ระบบบริหารคลังสินค้า - Inventory Management
        public IActionResult Inventory()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var products = _context.Products
                .Include(p => p.ProductSizes)
                .ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(string brand, string model, string fieldType, decimal price, string? imageUrl, int[] sizes, int[] stocks)
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
            _context.SaveChanges();

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
            _context.SaveChanges();

            return RedirectToAction("Inventory");
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var product = _context.Products
                .Include(p => p.ProductSizes)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        public IActionResult EditProduct(int productId, string brand, string model, string fieldType, decimal price, string? imageUrl)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            product.Brand = brand;
            product.Model = model;
            product.FieldType = fieldType;
            product.Price = price;
            product.ImageUrl = imageUrl;

            _context.SaveChanges();
            return RedirectToAction("Inventory");
        }

        [HttpPost]
        public IActionResult UpdateStock(int sizeId, int newStock)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var productSize = _context.ProductSizes.Find(sizeId);
            if (productSize != null)
            {
                productSize.Stock = newStock;
                _context.SaveChanges();
            }

            return RedirectToAction("Inventory");
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("Inventory");
        }

        // 2.1 ระบบจัดการขนส่ง - Shipping Management
        public IActionResult Shipments()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var shipments = _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o!.User)
                .ToList();

            return View(shipments);
        }

        [HttpPost]
        public IActionResult UpdateShipment(int shipmentId, string status, string trackingNumber)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var shipment = _context.Shipments.Find(shipmentId);
            if (shipment != null)
            {
                shipment.Status = status;
                shipment.TrackingNumber = trackingNumber;

                // Update order status if shipment is delivered
                if (status == "Delivered")
                {
                    var order = _context.Orders.Find(shipment.OrderId);
                    if (order != null)
                        order.Status = "Completed";
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Shipments");
        }

        // 2.3 ระบบจัดการโปรโมชั่น - Promotion Management
        public IActionResult Promotions()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var promotions = _context.Promotions
                .OrderByDescending(p => p.StartDate)
                .ToList();

            var coupons = _context.Coupons
                .OrderByDescending(c => c.CouponId)
                .ToList();

            ViewBag.Coupons = coupons;
            return View(promotions);
        }

        [HttpPost]
        public IActionResult AddPromotion(string title, DateOnly startDate, DateOnly endDate, int discountPercent)
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
            _context.SaveChanges();

            return RedirectToAction("Promotions");
        }

        [HttpPost]
        public IActionResult AddCoupon(string code, int discountPercent, DateOnly expiryDate)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var coupon = new Coupon
            {
                Code = code,
                DiscountPercent = discountPercent,
                ExpiryDate = expiryDate
            };

            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            return RedirectToAction("Promotions");
        }

        [HttpPost]
        public IActionResult DeletePromotion(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var promotion = _context.Promotions.Find(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                _context.SaveChanges();
            }

            return RedirectToAction("Promotions");
        }

        // 2.4 ระบบรายงานรายรับ-รายจ่าย - Financial Reports
        public IActionResult Reports()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            // Daily sales
            var today = DateOnly.FromDateTime(DateTime.Now);
            var dailySales = _context.Orders
                .Where(o => o.Status == "Completed" && DateOnly.FromDateTime(o.CreatedAt!.Value) == today)
                .Sum(o => o.TotalPrice);

            // Monthly sales
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var monthlySales = _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt >= monthStart)
                .Sum(o => o.TotalPrice);

            // Total sales
            var totalSales = _context.Orders
                .Where(o => o.Status == "Completed")
                .Sum(o => o.TotalPrice);

            // Order counts
            var totalOrders = _context.Orders.Count();
            var completedOrders = _context.Orders.Count(o => o.Status == "Completed");
            var pendingOrders = _context.Orders.Count(o => o.Status == "Pending");

            ViewBag.DailySales = dailySales ?? 0;
            ViewBag.MonthlySales = monthlySales ?? 0;
            ViewBag.TotalSales = totalSales ?? 0;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.PendingOrders = pendingOrders;

            // Recent completed orders for the report table
            var recentOrders = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(20)
                .ToList();

            return View(recentOrders);
        }

        // Order Management
        public IActionResult Orders()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        public IActionResult OrderDetails(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                .Include(o => o.Shipments)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
            }

            return RedirectToAction("Orders");
        }

        // User Management
        public IActionResult Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(int userId, string name, string email, string role)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var user = _context.Users.Find(userId);
            if (user == null) return NotFound();

            user.Name = name;
            user.Email = email;
            user.Role = role;

            _context.SaveChanges();
            return RedirectToAction("Users");
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("Users");
        }
    }
}
