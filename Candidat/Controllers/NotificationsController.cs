using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CvParsing.Data;
using CvParsing.Models;

namespace CvParsing.Controllers;

public class NotificationsController : Controller
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Notifications") });

        var notifications = await _context.Notifications
            .Where(n => n.UtilisateurId == userId)
            .OrderByDescending(n => n.DateCreation)
            .ToListAsync();

        // Marquer toutes comme lues
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

