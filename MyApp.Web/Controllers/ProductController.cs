using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Web.Data;
using MyApp.Web.Models;

namespace MyApp.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =========================
        // USER + ADMIN
        // =========================

        // GET: /Product
        public async Task<IActionResult> Index(
    string? search,
    int? subCategoryId)
        {
            var query = _context.Products
                .Include(p => p.SubCategory)
                .ThenInclude(s => s!.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            if (subCategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.SubCategoryId == subCategoryId.Value);
            }

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.Search = search;

            ViewBag.SubCategories = await _context.SubCategories
                .Include(s => s.Category)
                .OrderBy(s => s.Category!.Name)
                .ThenBy(s => s.Name)
                .ToListAsync();

            ViewBag.SelectedSubCategory = subCategoryId;

            return View(products);
        }


        // =========================
        // ADMIN ONLY
        // =========================

        // GET: /Product/Manage
        public async Task<IActionResult> Manage(string search)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var productsQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            var products = await productsQuery
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.Search = search;

            return View(products);
        }


        // GET: /Product/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "User");

            ViewBag.SubCategories = await _context.SubCategories
                .Include(s => s.Category)
                .Where(s => s.Category!.IsActive)
                .OrderBy(s => s.Category!.Name)
                .ThenBy(s => s.Name)
                .ToListAsync();

            return View();
        }


        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "User");

            if (!ModelState.IsValid)
            {
                ViewBag.SubCategories = await _context.SubCategories
                    .Include(s => s.Category)
                    .Where(s => s.Category!.IsActive)
                    .OrderBy(s => s.Category!.Name)
                    .ThenBy(s => s.Name)
                    .ToListAsync();

                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                IsActive = true,
                SubCategoryId = model.SubCategoryId
            };

            // =========================
            // IMAGE UPLOAD
            // =========================

            if (model.ImageFile != null &&
                model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "products"
                );

                // Create folder if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique file name
                var fileName = Guid.NewGuid().ToString()
                               + Path.GetExtension(model.ImageFile.FileName);

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName
                );

                // Save image
                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // Save URL in database
                product.ImageUrl =
                    "/images/products/" + fileName;
            }

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }
        // GET: /Product/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        // POST: /Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.ImageUrl = model.ImageUrl;
            product.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }


        // GET: /Product/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        // POST: /Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }


        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");

            return role == "Admin";
        }

        // ============================
        // PRODUCT DETAILS
        // ============================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

       

    }
}