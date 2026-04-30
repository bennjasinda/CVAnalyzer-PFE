using Administration.Data;
using Administration.Filters;
using Administration.Helpers;
using Administration.Models;
using Administration.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Administration.Controllers
{
    [SessionAuthorize("Directeur")]
    public class DirecteurDepartementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DirecteurDepartementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= DASHBOARD =================
        public IActionResult Dashboard()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var user = _context.Utilisateurs.Find(userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var userDepartments = user.Departements?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .ToList() ?? new List<string>();

            var departmentOffres = _context.OffresEmploi
                .Where(o => userDepartments.Contains(o.Departement))
                .ToList();

            var offreIds = departmentOffres.Select(o => o.Id).ToList();

            var departmentCvs = _context.Cvs
                .Where(c => offreIds.Contains(c.OffreId))
                .ToList();

            var cvIds = departmentCvs.Select(c => c.Id).ToList();

            var departmentMatches = _context.Matches
                .Where(m => cvIds.Contains(m.CvId))
                .ToList();

            var stats = new DashboardStatsViewModel
            {
                TotalOffres  = departmentOffres.Count,
                TotalCvs     = departmentCvs.Count,
                TotalMatches = departmentMatches.Count,
                TotalUsers   = userDepartments.Count
            };

            ViewBag.UserDepartments = userDepartments;
            ViewBag.AcceptedCount = 0;
            ViewBag.RejectedCount = 0;

            return View(stats);
        }

        // ================= POSTES =================
        public IActionResult Postes(string? search)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var user = _context.Utilisateurs.Find(userId);

            if (user == null || string.IsNullOrEmpty(user.Departements))
                return View(new List<OffreEmploi>());

            var userDepartments = user.Departements
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .ToList();

            var query = _context.OffresEmploi
                .Where(o => userDepartments.Contains(o.Departement));

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o =>
                    o.Titre.Contains(search) ||
                    o.Departement.Contains(search));
            }

            ViewBag.Search = search;
            ViewBag.UserDepartments = userDepartments;

            return View(query.ToList());
        }

        // ================= DETAILS POSTE =================
        // Alias pour DetailPoste (sans 's') pour éviter les erreurs 404
        public IActionResult DetailPoste(int id)
        {
            return RedirectToAction(nameof(DetailsPoste), new { id });
        }

        public IActionResult DetailsPoste(int id)
        {
            MatchIntegrationHelper.EnsureMatchesForOffreAsync(_context, id)
                .GetAwaiter().GetResult();

            var offre = _context.OffresEmploi
                .Include(o => o.Cvs)
                    .ThenInclude(c => c.Matches)
                .FirstOrDefault(o => o.Id == id);

            if (offre == null) return NotFound();

            return View(offre);
        }

        // ================= CV RESULT =================
        public IActionResult CvResult(int offreId, int cvId)
        {
            if (cvId <= 0)
            {
                TempData["Error"] = "Candidature introuvable.";
                return RedirectToAction(nameof(DetailsPoste), new { id = offreId });
            }

            var cvExists = _context.Cvs.Any(c => c.Id == cvId && c.OffreId == offreId);
            if (!cvExists)
            {
                TempData["Error"] = "Candidature introuvable.";
                return RedirectToAction(nameof(DetailsPoste), new { id = offreId });
            }

            MatchIntegrationHelper.EnsureMatchForCvAsync(_context, offreId, cvId)
                .GetAwaiter().GetResult();

            _context.SaveChanges();

            var match = _context.Matches
                .Include(m => m.Cv)
                    .ThenInclude(c => c.Utilisateur)
                .Include(m => m.Offre)
                .FirstOrDefault(m => m.OffreId == offreId && m.CvId == cvId);

            if (match == null) return NotFound();

            ViewBag.ShowDirectorValidationActions = true;

            return View("~/Views/Admin/CvResult.cshtml", match);
        }

        // ================= CREATE POSTE =================
        [HttpGet]
        public IActionResult CreatePoste()
            => View(new OffreEmploi());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePoste(OffreEmploi model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.DateCreation = DateTime.Now;
            model.Statut = "ACTIF";
            model.IdResponsable = 0;

            _context.OffresEmploi.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Poste créé avec succès.";
            return RedirectToAction("Postes");
        }

        // ================= EDIT POSTE =================
        [HttpGet]
        public IActionResult EditPoste(int id)
        {
            var offre = _context.OffresEmploi.Find(id);
            if (offre == null) return NotFound();
            return View(offre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPoste(OffreEmploi model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.OffresEmploi.Update(model);
            _context.SaveChanges();

            TempData["Success"] = "Poste modifié avec succès.";
            return RedirectToAction("Postes");
        }

        // ================= DELETE POSTE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePoste(int id)
        {
            try
            {
                if (!OffreDeletionHelper.TryDeleteOffre(_context, id, out var err))
                {
                    TempData["Error"] = err ?? "Erreur suppression.";
                    return RedirectToAction("Postes");
                }

                TempData["Success"] = "Poste supprimé avec succès.";
            }
            catch
            {
                TempData["Error"] = "Erreur lors de la suppression.";
            }

            return RedirectToAction("Postes");
        }

        // ================= RESULTATS CV =================
        public IActionResult ResultatsCV(int? offreId)
        {
            ViewBag.Offres = _context.OffresEmploi.ToList();

            if (!offreId.HasValue)
                return View(new List<Match>());

            MatchIntegrationHelper.EnsureMatchesForOffreAsync(_context, offreId.Value)
                .GetAwaiter().GetResult();

            var matches = _context.Matches
                .Include(m => m.Cv)
                    .ThenInclude(c => c.Offre)
                .Where(m => m.Cv.OffreId == offreId.Value)
                .OrderByDescending(m => m.GlobalScore)
                .ToList();

            ViewBag.SelectedOffreId = offreId.Value;

            return View(matches);
        }

        // ================= PROFILE =================
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var user = _context.Utilisateurs.Find(userId);

            if (user == null) return NotFound();

            return View(new ProfileEditViewModel
            {
                Id = user.Id,
                NomUtilisateur = user.NomUtilisateur,
                Email = user.Email,
                CurrentPhotoUrl = user.PhotoUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Utilisateurs.Find(model.Id);
            if (user == null) return NotFound();

            user.NomUtilisateur = model.NomUtilisateur;
            user.Email = model.Email;

            _context.SaveChanges();

            TempData["Success"] = "Profil mis à jour.";
            return RedirectToAction("Profile");
        }

        // ================= VALIDATION CV =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCandidateValidationStatus(int cvId, int offreId, string status, bool returnToDetails = false)
        {
            if (status != "Accepted" && status != "Rejected")
            {
                TempData["Error"] = "Statut invalide.";
                return RedirectToAction(nameof(DetailsPoste), new { id = offreId });
            }

            var cv = _context.Cvs.FirstOrDefault(c => c.Id == cvId && c.OffreId == offreId);
            if (cv == null)
            {
                TempData["Error"] = "CV introuvable.";
                return RedirectToAction(nameof(DetailsPoste), new { id = offreId });
            }

            cv.ValidationStatus = status;
            _context.SaveChanges();

            TempData["Success"] = "Statut mis à jour.";

            return RedirectToAction(nameof(DetailsPoste), new { id = offreId });
        }
    }
}