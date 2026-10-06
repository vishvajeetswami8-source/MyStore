using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Web.Data;
using MyApp.Web.Models;

namespace MyApp.Web.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlist = await _context.Wishlists
                .Include(w => w.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserId = userId.Value
                };
            }

            return View(wishlist);
        }

        // GET: /Wishlist/Add?productId=1
        public async Task<IActionResult> Add(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check product
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == productId &&
                    p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            // Find user's wishlist
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId);

            // Create wishlist if user doesn't have one
            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserId = userId.Value
                };

                _context.Wishlists.Add(wishlist);

                await _context.SaveChangesAsync();
            }

            // Check whether product already exists
            var existingItem = await _context.WishlistItems
                .FirstOrDefaultAsync(wi =>
                    wi.WishlistId == wishlist.Id &&
                    wi.ProductId == productId);

            if (existingItem == null)
            {
                var wishlistItem = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = productId
                };

                _context.WishlistItems.Add(wishlistItem);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlistItem = await _context.WishlistItems
                .Include(wi => wi.Wishlist)
                .FirstOrDefaultAsync(wi =>
                    wi.ProductId == productId &&
                    wi.Wishlist!.UserId == userId);

            if (wishlistItem != null)
            {
                _context.WishlistItems.Remove(wishlistItem);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}