using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Web.Data;
using MyApp.Web.Models;

namespace MyApp.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // ============================
        // CATEGORY MANAGEMENT
        // ============================

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "User");

            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "User");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "User");

            if (!ModelState.IsValid)
                return View(model);

            var exists = await _context.Categories
                .AnyAsync(c => c.Name == model.Name);

            if (exists)
            {
                ModelState.AddModelError(
                    "Name",
                    "Category already exists.");

                return View(model);
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                IsActive = true
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        // ============================
        // SUBCATEGORY
        // ============================

        [HttpGet]
        public async Task<IActionResult> CreateSubCategory(int categoryId)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "User");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                return NotFound();

            ViewBag.Category = category;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubCategory(
            int categoryId,
            string name)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "User");

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(
                    "name",
                    "Subcategory name is required.");

                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == categoryId);

                ViewBag.Category = category;

                return View();
            }

            var exists = await _context.SubCategories
                .AnyAsync(s =>
                    s.CategoryId == categoryId &&
                    s.Name == name);

            if (exists)
            {
                ModelState.AddModelError(
                    "name",
                    "Subcategory already exists.");

                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == categoryId);

                ViewBag.Category = category;

                return View();
            }

            var subCategory = new SubCategory
            {
                Name = name.Trim(),
                CategoryId = categoryId
            };

            _context.SubCategories.Add(subCategory);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        // ============================
        // DELETE SUBCATEGORY
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "User");

            var subCategory = await _context.SubCategories
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subCategory == null)
                return NotFound();

            var hasProducts = await _context.Products
                .AnyAsync(p => p.SubCategoryId == id);

            if (hasProducts)
            {
                TempData["Error"] =
                    "Cannot delete this subcategory because products are assigned to it.";

                return RedirectToAction(nameof(Manage));
            }

            _context.SubCategories.Remove(subCategory);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }
    }
}