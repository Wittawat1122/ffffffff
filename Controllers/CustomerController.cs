using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using fffff.Models.Db;

namespace fffff.Controllers
{
    public class CustomerController : Controller
    {
        private readonly FinalProjectContext _context;

        public CustomerController(FinalProjectContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // 1.4 ระบบเลือกชมสินค้า - Product browsing by field type and brand
        public async Task<IActionResult> Index(string? fieldType, string? brand, string? search)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var query = _context.Products
                .Include(p => p.ProductSizes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(fieldType))
                query = query.Where(p => p.FieldType == fieldType);

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(p => p.Brand == brand);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Model!.Contains(search) || p.Brand!.Contains(search));

            ViewBag.FieldTypes = await _context.Products.Select(p => p.FieldType).Distinct().ToListAsync();
            ViewBag.Brands = await _context.Products.Select(p => p.Brand).Distinct().ToListAsync();
            ViewBag.CurrentFieldType = fieldType;
            ViewBag.CurrentBrand = brand;
            ViewBag.Search = search;

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var product = await _context.Products
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // 1.2 ระบบจัดการที่อยู่ - Address management
        public async Task<IActionResult> Addresses()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return View(addresses);
        }

        [HttpGet]
        public IActionResult AddAddress()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress(string address, string label, bool isDefault)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (isDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ToListAsync();
                foreach (var addr in existingDefaults)
                    addr.IsDefault = false;
            }

            var newAddress = new Address
            {
                UserId = userId,
                Address1 = address,
                Label = label,
                IsDefault = isDefault
            };

            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return RedirectToAction("Addresses");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);

            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Addresses");
        }

        // 1.1 ระบบจัดการคูปอง - Coupon management
        public async Task<IActionResult> Coupons()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var myCoupons = await _context.UserCoupons
                .Include(uc => uc.Coupon)
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            var availableCoupons = await _context.Coupons
                .Where(c => c.ExpiryDate > DateOnly.FromDateTime(DateTime.Now) &&
                    !_context.UserCoupons.Any(uc => uc.UserId == userId && uc.CouponId == c.CouponId))
                .ToListAsync();

            ViewBag.AvailableCoupons = availableCoupons;
            return View(myCoupons);
        }

        [HttpPost]
        public async Task<IActionResult> CollectCoupon(int couponId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var existing = await _context.UserCoupons
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CouponId == couponId);

            if (existing == null)
            {
                var userCoupon = new UserCoupon
                {
                    UserId = userId.Value,
                    CouponId = couponId,
                    IsUsed = false
                };
                _context.UserCoupons.Add(userCoupon);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Coupons");
        }

        // 1.3 ระบบสะสมคะแนน - Loyalty Points
        public async Task<IActionResult> LoyaltyPoints()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var points = await _context.LoyaltyPoints
                .Where(lp => lp.UserId == userId)
                .SumAsync(lp => lp.Points);

            ViewBag.TotalPoints = points;
            return View();
        }

        // Shopping Cart & Orders
        public async Task<IActionResult> Cart()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var cartItems = HttpContext.Session.GetString($"Cart_{userId}");
            ViewBag.CartItems = cartItems;

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();
            ViewBag.Addresses = addresses;

            var coupons = await _context.UserCoupons
                .Include(uc => uc.Coupon)
                .Where(uc => uc.UserId == userId && uc.IsUsed == false)
                .ToListAsync();
            ViewBag.Coupons = coupons;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(int addressId, int? userCouponId, decimal totalPrice, string items)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var order = new Order
            {
                UserId = userId,
                AddressId = addressId,
                TotalPrice = totalPrice,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add order items
            var itemList = items.Split(';');
            foreach (var item in itemList)
            {
                var parts = item.Split(',');
                if (parts.Length == 4)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = int.Parse(parts[0]),
                        Size = int.Parse(parts[1]),
                        Quantity = int.Parse(parts[2]),
                        Price = decimal.Parse(parts[3])
                    };
                    _context.OrderItems.Add(orderItem);

                    // Reduce stock
                    var productSize = await _context.ProductSizes
                        .FirstOrDefaultAsync(ps => ps.ProductId == orderItem.ProductId && ps.Size == orderItem.Size);
                    if (productSize != null)
                    {
                        productSize.Stock -= orderItem.Quantity;
                    }
                }
            }

            // Mark coupon as used
            if (userCouponId.HasValue)
            {
                var userCoupon = await _context.UserCoupons.FindAsync(userCouponId);
                if (userCoupon != null)
                    userCoupon.IsUsed = true;
            }

            // Add loyalty points (1 point per 100 baht)
            var points = (int)(totalPrice / 100);
            var loyaltyPoint = new LoyaltyPoint
            {
                UserId = userId.Value,
                Points = points
            };
            _context.LoyaltyPoints.Add(loyaltyPoint);

            // Create shipment
            var shipment = new Shipment
            {
                OrderId = order.OrderId,
                Status = "Pending",
                TrackingNumber = ""
            };
            _context.Shipments.Add(shipment);

            await _context.SaveChangesAsync();

            // Clear cart
            HttpContext.Session.Remove($"Cart_{userId}");

            return RedirectToAction("OrderDetails", new { id = order.OrderId });
        }

        public async Task<IActionResult> Orders()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var orders = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.Shipments)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                .Include(o => o.Shipments)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
