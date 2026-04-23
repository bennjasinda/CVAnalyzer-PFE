using Administration.Data;
using Administration.Filters;
using Administration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Administration.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [SessionAuthorize("RH", "Directeur")]
        public async Task<IActionResult> Index()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Where(n => n.UtilisateurId == userId)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();

            // Mark all as read
            var unread = notifications.Where(n => !n.IsRead).ToList();
            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            if (unread.Any())
            {
                await _context.SaveChangesAsync();
            }

            ViewData["Title"] = "Notifications";
            return View(notifications);
        }
    }
}
