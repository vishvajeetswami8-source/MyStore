using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Web.Data;
using MyApp.Web.Models;

namespace MyApp.Web.Controllers
{
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // USER SIDE
        // ============================

        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateSupportTicketViewModel model)
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

            var ticket = new SupportTicket
            {
                UserId = userId.Value,
                Subject = model.Subject,
                Description = model.Description,
                Priority = model.Priority,
                Status = "Open",
                CreatedDate = DateTime.Now,

                // Admin has not seen this ticket yet
                AdminViewed = false,

                // User has already seen their own submission
                UserViewed = true
            };

            _context.SupportTickets.Add(ticket);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Support request #{ticket.Id} has been submitted successfully.";

            return RedirectToAction(nameof(MyTickets));
        }

        [HttpGet]
        public async Task<IActionResult> MyTickets()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var tickets = await _context.SupportTickets
                .Where(x => x.UserId == userId.Value)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ticket = await _context.SupportTickets
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }


        // ============================
        // ADMIN SIDE
        // ============================

        [HttpGet]
        public async Task<IActionResult> Manage(
            string? search,
            string? status)
        {
            if (!IsAdmin())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "User");
            }

            var query = _context.SupportTickets
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                if (int.TryParse(search, out int ticketId))
                {
                    query = query.Where(x =>
                        x.Id == ticketId ||
                        x.User!.Username.Contains(search) ||
                        (x.User.Email != null &&
                         x.User.Email.Contains(search)));
                }
                else
                {
                    query = query.Where(x =>
                        x.User!.Username.Contains(search) ||
                        (x.User.Email != null &&
                         x.User.Email.Contains(search)));
                }
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                status != "All")
            {
                query = query.Where(x =>
                    x.Status == status);
            }

            var tickets = await query
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(tickets);
        }


        [HttpGet]
        public async Task<IActionResult> AdminDetails(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "User");
            }

            var ticket = await _context.SupportTickets
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(
            SupportTicketReplyViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "User");
            }

            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (ticket == null)
            {
                return NotFound();
            }

            var allowedStatuses = new[]
            {
                "Open",
                "In Progress",
                "Resolved",
                "Closed"
            };

            if (!allowedStatuses.Contains(model.Status))
            {
                TempData["Error"] =
                    "Invalid support ticket status.";

                return RedirectToAction(
                    nameof(AdminDetails),
                    new { id = model.Id });
            }

            ticket.Status = model.Status;
            ticket.AdminResponse = model.AdminResponse;
            ticket.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Support ticket #{ticket.Id} has been updated.";

            return RedirectToAction(
                nameof(AdminDetails),
                new { id = ticket.Id });
        }


        private bool IsAdmin()
        {
            return HttpContext.Session
                .GetString("Role") == "Admin";
        }


        [HttpGet]
        public async Task<IActionResult> AdminNotificationCount()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            var count = await _context.SupportTickets
                .CountAsync(x => !x.AdminViewed);

            return Json(count);
        }

        [HttpGet]
        public async Task<IActionResult> UserNotificationCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized();
            }

            var count = await _context.SupportTickets
                .CountAsync(x =>
                    x.UserId == userId.Value &&
                    !x.UserViewed);

            return Json(count);
        }
    }
}