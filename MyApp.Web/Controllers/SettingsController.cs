using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Web.Data;
using MyApp.Web.Models;

namespace MyApp.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public SettingsController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // ============================
        // SETTINGS
        // ============================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }


        // ============================
        // CHANGE PASSWORD - GET
        // ============================

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        // ============================
        // CHANGE PASSWORD - POST
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Verify current password
            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    model.CurrentPassword);

            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "CurrentPassword",
                    "Current password is incorrect.");

                return View(model);
            }

            // Prevent same password
            var samePassword =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    model.NewPassword);

            if (samePassword !=
                PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "New password must be different from your current password.");

                return View(model);
            }

            // Hash new password
            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    model.NewPassword);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your password has been changed successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}