using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Web.Data;
using MyApp.Web.Models;

namespace MyApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<AccountController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
            {
                _logger.LogWarning(
                    "Login failed. User not found: {Username}",
                    model.Username);

                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning(
                    "Login failed. Invalid password for user: {Username}",
                    model.Username);

                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            _logger.LogInformation(
                "User logged in successfully. Username: {Username}, Role: {Role}",
                user.Username,
                user.Role);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("Username");

            _logger.LogInformation(
                "User logged out. Username: {Username}",
                username ?? "Unknown");

            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> CreateTestUser()
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Username == "admin");

            if (existingUser != null)
            {
                return Content("User already exists.");
            }

            var user = new User
            {
                Username = "admin",
                Email = "admin@myapp.com"
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                "Admin@123");

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Content("Test user created successfully.");
        }

        //[HttpGet]
        //public async Task<IActionResult> LoginAsUser(int id)
        //{
        //    var currentUserId = HttpContext.Session.GetInt32("UserId");
        //    var currentRole = HttpContext.Session.GetString("Role");

        //    if (currentUserId == null || currentRole != "Admin")
        //    {
        //        return RedirectToAction("AccessDenied", "User");
        //    }

        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Id == id);

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    if (user.Role == "Admin")
        //    {
        //        return BadRequest("Admin impersonation is not allowed.");
        //    }

        //    HttpContext.Session.SetInt32(
        //        "OriginalAdminId",
        //        currentUserId.Value);

        //    HttpContext.Session.SetInt32(
        //        "UserId",
        //        user.Id);

        //    HttpContext.Session.SetString(
        //        "Username",
        //        user.Username);

        //    HttpContext.Session.SetString(
        //        "Role",
        //        user.Role);

        //    HttpContext.Session.SetString(
        //        "IsImpersonating",
        //        "true");

        //    return RedirectToAction("Index", "Home");
        //}

        //[HttpGet]
        //public IActionResult BackToAdmin()
        //{
        //    var originalAdminId =
        //        HttpContext.Session.GetInt32("OriginalAdminId");

        //    if (originalAdminId == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    HttpContext.Session.SetInt32(
        //        "UserId",
        //        originalAdminId.Value);

        //    HttpContext.Session.SetString(
        //        "Role",
        //        "Admin");

        //    HttpContext.Session.SetString(
        //        "IsImpersonating",
        //        "false");

        //    HttpContext.Session.Remove("OriginalAdminId");

        //    var username = HttpContext.Session.GetString("Username");

        //    return RedirectToAction("Index", "Home");
        //}

        [HttpGet]
        public async Task<IActionResult> BackToAdmin()
        {
            var originalAdminId =
                HttpContext.Session.GetInt32("OriginalAdminId");

            if (originalAdminId == null)
            {
                return RedirectToAction("Login");
            }

            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == originalAdminId.Value);

            if (admin == null || admin.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "User");
            }

            // Restore Admin session
            HttpContext.Session.SetInt32(
                "UserId",
                admin.Id);

            HttpContext.Session.SetString(
                "Username",
                admin.Username);

            HttpContext.Session.SetString(
                "Role",
                admin.Role);

            // Clear impersonation information
            HttpContext.Session.Remove("OriginalAdminId");
            HttpContext.Session.Remove("IsImpersonating");

            return RedirectToAction("Index", "User");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
    RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == model.Username);

            if (usernameExists)
            {
                ModelState.AddModelError(
                    "Username",
                    "Username is already registered.");

                return View(model);
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Email",
                    "Email is already registered.");

                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,

                // New users should always be normal users
                Role = "User"
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    model.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Registration successful. Please login.";

            return RedirectToAction(nameof(Login));
        }
    }
}