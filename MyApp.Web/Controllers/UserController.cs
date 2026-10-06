using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Web.Data;
using MyApp.Web.Models;

namespace MyApp.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: /User
        public async Task<IActionResult> Index(string search)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            var usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.Role.Contains(search));
            }

            var users = await usersQuery
                .OrderBy(u => u.Username)
                .ToListAsync();

            ViewBag.Search = search;

            return View(users);
        }

        // GET: /User/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Username == model.Username);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Username",
                    "Username already exists.");

                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Role = model.Role
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                model.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: /User/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Username = model.Username;
            user.Email = model.Email;
            user.Role = model.Role;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");

            return role == "Admin";
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsUser(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentRole = HttpContext.Session.GetString("Role");

            if (currentUserId == null || currentRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role == "Admin")
            {
                return BadRequest("Admin impersonation is not allowed.");
            }

            HttpContext.Session.SetInt32(
                "OriginalAdminId",
                currentUserId.Value);

            HttpContext.Session.SetInt32(
                "UserId",
                user.Id);

            HttpContext.Session.SetString(
                "Username",
                user.Username);

            HttpContext.Session.SetString(
                "Role",
                user.Role);

            HttpContext.Session.SetString(
                "IsImpersonating",
                "true");

            return RedirectToAction("Index", "Home");
        }

    }
}