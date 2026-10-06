//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using MyApp.Web.Data;
//using MyApp.Web.Models;

//namespace MyApp.Web.Controllers
//{
//    public class OrderController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public OrderController(
//            ApplicationDbContext context)
//        {
//            _context = context;
//        }
//        public async Task<IActionResult> MyOrders()
//        {
//            var userId = HttpContext.Session.GetInt32("UserId");

//            if (userId == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var orders = await _context.Orders
//                .Where(o => o.UserId == userId)
//                .OrderByDescending(o => o.OrderDate)
//                .ToListAsync();

//            return View(orders);
//        }
//        public async Task<IActionResult> Manage()
//        {
//            if (!IsAdmin())
//            {
//                return RedirectToAction("AccessDenied", "User");
//            }

//            var orders = await _context.Orders
//                .Include(o => o.User)
//                .Include(o => o.OrderItems)
//                    .ThenInclude(oi => oi.Product)
//                .OrderByDescending(o => o.OrderDate)
//                .ToListAsync();

//            return View(orders);
//        }

//        private bool IsAdmin()
//        {
//            var role = HttpContext.Session.GetString("Role");

//            return role == "Admin";
//        }
//    }
//}




using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Web.Data;
using MyApp.Web.Models;

namespace MyApp.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Order/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";

                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                Items = cart.Items
                    .Where(x => x.Product != null)
                    .Select(x => new CheckoutItemViewModel
                    {
                        ProductName = x.Product!.Name,
                        ImageUrl = x.Product.ImageUrl,
                        UnitPrice = x.Product.Price,
                        Quantity = x.Quantity
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: /Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            CheckoutViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            // Get latest cart from database
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";

                return RedirectToAction("Index", "Cart");
            }

            // Validate products again
            foreach (var cartItem in cart.Items)
            {
                if (cartItem.Product == null ||
                    !cartItem.Product.IsActive)
                {
                    ModelState.AddModelError(
                        "",
                        $"Product is no longer available.");

                    return View("Checkout", model);
                }

                if (cartItem.Quantity <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid product quantity.");

                    return View("Checkout", model);
                }
            }

            // Calculate total from database prices
            decimal totalAmount = cart.Items.Sum(item =>
                item.Product!.Price * item.Quantity);

            // Create shipping address
            string shippingAddress =
                $"{model.FullName}, " +
                $"{model.Address}, " +
                $"{model.City}, " +
                $"{model.State} - " +
                $"{model.Pincode}. " +
                $"Phone: {model.Phone}";

            // Create Order
            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = totalAmount,
                ShippingAddress = shippingAddress
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            // Create OrderItems
            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product!.Price
                };

                _context.OrderItems.Add(orderItem);
            }

            // Remove cart items
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Confirmation),
                new { id = order.Id });
        }

        // GET: /Order/Confirmation/5
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


         //============================
         //MY ORDERS
         //============================
        //GET: /Order/MyOrders
       [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);

        }

        // ============================
        // ORDER DETAILS
        // ============================
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.UserId == userId.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


        // ============================
        // ADMIN ORDER MANAGEMENT
        // ============================

        public async Task<IActionResult> Manage(
            string? search,
            string? status)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var query = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            // Search by Order ID or customer information
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                if (int.TryParse(search, out int orderId))
                {
                    query = query.Where(o =>
                        o.Id == orderId ||
                        o.User!.Username.Contains(search) ||
                        (o.User.Email != null &&
                         o.User.Email.Contains(search)));
                }
                else
                {
                    query = query.Where(o =>
                        o.User!.Username.Contains(search) ||
                        (o.User.Email != null &&
                         o.User.Email.Contains(search)));
                }
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status) &&
                status != "All")
            {
                query = query.Where(o => o.Status == status);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(orders);
        }


        // ============================
        // ADMIN ORDER DETAILS
        // ============================

        public async Task<IActionResult> AdminDetails(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


        // ============================
        // UPDATE ORDER STATUS
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var allowedStatuses = new[]
            {
        "Pending",
        "Confirmed",
        "Shipped",
        "Delivered",
        "Cancelled"
    };

            if (!allowedStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid order status.";
                return RedirectToAction(nameof(AdminDetails), new { id });
            }

            // Don't allow changes after cancellation
            if (order.Status == "Cancelled")
            {
                TempData["Error"] =
                    "A cancelled order cannot be updated.";

                return RedirectToAction(nameof(AdminDetails), new { id });
            }

            // Don't allow changing Delivered order
            if (order.Status == "Delivered")
            {
                TempData["Error"] =
                    "A delivered order cannot be updated.";

                return RedirectToAction(nameof(AdminDetails), new { id });
            }

            order.Status = status;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Order #{order.Id} status updated to {status}.";

            return RedirectToAction(nameof(AdminDetails), new { id });
        }


        // ============================
        // CANCEL ORDER
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Cancelled")
            {
                TempData["Error"] =
                    "This order is already cancelled.";

                return RedirectToAction(nameof(AdminDetails),
                    new { id });
            }

            if (order.Status == "Delivered")
            {
                TempData["Error"] =
                    "A delivered order cannot be cancelled.";

                return RedirectToAction(nameof(AdminDetails),
                    new { id });
            }

            if (order.Status == "Shipped")
            {
                TempData["Error"] =
                    "A shipped order cannot be cancelled from admin.";

                return RedirectToAction(nameof(AdminDetails),
                    new { id });
            }

            order.Status = "Cancelled";

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Order #{order.Id} has been cancelled.";

            return RedirectToAction(nameof(AdminDetails),
                new { id });
        }


        // ============================
        // ADMIN CHECK
        // ============================

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }
    }
}