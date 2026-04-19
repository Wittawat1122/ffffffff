using Microsoft.AspNetCore.Mvc;
using fffff.Models.Db;
using System.Security.Cryptography;
using System.Text;

namespace fffff.Controllers
{
    public class AuthController : Controller
    {
        private readonly FinalProjectContext _context;

        public AuthController(FinalProjectContext context)
        {
            _context = context;
        }

        // ================= LOGIN =================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            // 🔥 รองรับทั้ง plain + hash
            bool isMatch = false;

            // plain text
            if (user.Password == password)
                isMatch = true;

            // hash (SHA256)
            if (user.Password == HashPassword(password))
                isMatch = true;

            if (!isMatch)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            // ✅ SESSION
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role ?? "customer");
            HttpContext.Session.SetString("Name", user.Name ?? "");

            // 🔥 ROLE
            if (user.Role == "admin")
                return RedirectToAction("Dashboard", "Admin");
            else
                return RedirectToAction("Index", "Customer");
        }

        // ================= REGISTER =================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string name, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Username already exists";
                return View();
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            var user = new User
            {
                Username = username,
                Name = name,
                Email = email,
                Password = password, // 🔥 ใช้ plain ไปก่อน
                Role = "customer",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // create loyalty point
            var loyaltyPoint = new LoyaltyPoint
            {
                UserId = user.UserId,
                Points = 0
            };

            _context.LoyaltyPoints.Add(loyaltyPoint);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ================= HASH FUNCTION =================
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}