using Administration.Data;
using Administration.Filters;
using Administration.Helpers;
using Administration.Models;
using Administration.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Administration.Controllers
{
    [SessionAuthorize("RH")]
    public class RHController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RHController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= DASHBOARD =================
        public IActionResult Dashboard()
        {
            var totalOffres = _context.OffresEmploi.Count();
            var totalCvs = _context.Cvs.Count();
            var totalMatches = _context.Matches.Count();
            var totalAcceptes = _context.Cvs.Count(c => c.Statut == "Accepte");
            var totalRefuses = _context.Cvs.Count(c => c.Statut == "Refuse");
            var totalEnAttente = _context.Cvs.Count(c => c.Statut == "En attente");

            // Calculate offers by month (last 6 months)
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var offresByMonth = _context.OffresEmploi
                .Where(o => o.DateCreation >= sixMonthsAgo)
                .GroupBy(o => new { o.DateCreation.Year, o.DateCreation.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => g.Count())
                .ToList();

            // Pad with zeros if less than 6 months
            while (offresByMonth.Count < 6)
            {
                offresByMonth.Insert(0, 0);
            }

            var stats = new DashboardStatsViewModel
            {
                TotalOffres = totalOffres,
                TotalCvs = totalCvs,
                TotalMatches = totalMatches,
                TotalAcceptes = totalAcceptes,
                TotalRefuses = totalRefuses,
                CandidatsAcceptes = _context.Cvs
                    .Include(c => c.Utilisateur)
                    .Include(c => c.Offre)
                    .Where(c => c.Statut == "Accepte")
                    .OrderByDescending(c => c.UploadDate)
                    .ToList()
            };

            ViewBag.OffresByMonth = offresByMonth;
            ViewBag.TotalEnAttente = totalEnAttente;

            return View(stats);
        }

        // ================= EXPORT CSV CANDIDATS ACCEPTES =================
        public IActionResult ExportAcceptedCsv()
        {
            var acceptes = _context.Cvs
                .Include(c => c.Utilisateur)
                .Include(c => c.Offre)
                .Where(c => c.Statut == "Accepte")
                .OrderByDescending(c => c.UploadDate)
                .ToList();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Nom,Email,Poste,Département,Date Candidature,Score");

            foreach (var cv in acceptes)
            {
                var match = _context.Matches
                    .Where(m => m.CvId == cv.Id)
                    .OrderByDescending(m => m.GlobalScore)
                    .FirstOrDefault();
                var score = match?.GlobalScore.ToString("F2") ?? "N/A";
                var nom = cv.Utilisateur?.NomUtilisateur?.Replace(",", " ") ?? "";
                var email = cv.Utilisateur?.Email?.Replace(",", " ") ?? "";
                var poste = cv.Offre?.Titre?.Replace(",", " ") ?? "";
                var dept = cv.Offre?.Departement?.Replace(",", " ") ?? "";
                var date = cv.UploadDate.ToString("dd/MM/yyyy");
                csv.AppendLine($"{nom},{email},{poste},{dept},{date},{score}");
            }

            // Use UTF-8 encoding with BOM for proper Excel display of French accents
            var utf8WithBom = new System.Text.UTF8Encoding(true);
            var bytes = utf8WithBom.GetBytes(csv.ToString());
            var fileName = $"candidats_acceptes_{DateTime.Now:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }

        // ================= EXPORT PDF CANDIDATS ACCEPTES =================
        public IActionResult ExportAcceptedPdf()
        {
            var acceptes = _context.Cvs
                .Include(c => c.Utilisateur)
                .Include(c => c.Offre)
                .Where(c => c.Statut == "Accepte")
                .OrderByDescending(c => c.UploadDate)
                .ToList();

            // Generate HTML for PDF
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        h1 { color: #333; border-bottom: 3px solid #4CAF50; padding-bottom: 10px; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th { background: #4CAF50; color: white; padding: 12px; text-align: left; }
        td { padding: 10px; border-bottom: 1px solid #ddd; }
        tr:nth-child(even) { background: #f9f9f9; }
        .footer { margin-top: 30px; text-align: center; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <h1>Candidats Acceptés</h1>
    <table>
        <thead>
            <tr>
                <th>Nom</th>
                <th>Email</th>
                <th>Poste</th>
                <th>Score</th>
                <th>Date</th>
            </tr>
        </thead>
        <tbody>";

            foreach (var cv in acceptes)
            {
                var match = _context.Matches
                    .Where(m => m.CvId == cv.Id)
                    .OrderByDescending(m => m.GlobalScore)
                    .FirstOrDefault();
                var score = match?.GlobalScore.ToString("F1") ?? "N/A";
                var nom = cv.Utilisateur?.NomUtilisateur ?? "";
                var email = cv.Utilisateur?.Email ?? "";
                var poste = cv.Offre?.Titre ?? "";
                var date = cv.UploadDate.ToString("dd/MM/yyyy");

                html += $@"
            <tr>
                <td>{System.Net.WebUtility.HtmlEncode(nom)}</td>
                <td>{System.Net.WebUtility.HtmlEncode(email)}</td>
                <td>{System.Net.WebUtility.HtmlEncode(poste)}</td>
                <td>{score}</td>
                <td>{date}</td>
            </tr>";
            }

            html += @"
        </tbody>
    </table>
    <div class='footer'>
        <p>Généré le " + DateTime.Now.ToString("dd/MM/yyyy à HH:mm") + @"</p>
    </div>
</body>
</html>";

            // Return as HTML (user can print to PDF from browser)
            var bytes = System.Text.Encoding.UTF8.GetBytes(html);
            var fileName = $"candidats_acceptes_{DateTime.Now:yyyyMMdd}.html";
            return File(bytes, "text/html", fileName);
        }

        // ================= LISTE DES POSTES =================
        public IActionResult Postes(string? search, int page = 1)
        {
            const int pageSize = 10;
            page = page < 1 ? 1 : page;

            var query = _context.OffresEmploi.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.Titre.Contains(search) || o.Departement.Contains(search));

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var pagedItems = query
                .OrderByDescending(o => o.DateCreation)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.Pagination = new PaginationViewModel
            {
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
            };
            return View(pagedItems);
        }

        // ================= DÉTAIL D'UN POSTE =================
        public async Task<IActionResult> DetailPoste(int id, int page = 1)
        {
            const int pageSize = 8;
            page = page < 1 ? 1 : page;

            await MatchIntegrationHelper.EnsureMatchesForOffreAsync(_context, id);

            var offre = await _context.OffresEmploi
                .Include(o => o.Cvs)
                    .ThenInclude(c => c.Utilisateur)
                .Include(o => o.Cvs)
                    .ThenInclude(c => c.Matches)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (offre == null)
            {
                return NotFound();
            }

            var allCvs = offre.Cvs
                .Where(cv => cv.Utilisateur != null)
                .OrderByDescending(cv => cv.UploadDate)
                .ToList();

            var totalItems = allCvs.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var pagedCvs = allCvs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var candidats = pagedCvs
                .Select(cv => cv.Utilisateur)
                .DistinctBy(u => u.Id)
                .ToList();

            var cvByUserId = offre.Cvs
                .GroupBy(cv => cv.UtilisateurId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(cv => cv.UploadDate).First().Id);

            var scoreByUserId = offre.Cvs
                .GroupBy(cv => cv.UtilisateurId)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(cv => cv.Matches).OrderByDescending(m => m.GlobalScore).FirstOrDefault()?.GlobalScore ?? 0f
                );

            var statusByUserId = pagedCvs
                .ToDictionary(cv => cv.UtilisateurId, cv => cv.Statut);

            var viewModel = new PosteDetailViewModel
            {
                Offre = offre,
                Candidats = candidats
            };

            ViewBag.CvByUserId = cvByUserId;
            ViewBag.ScoreByUserId = scoreByUserId;
            ViewBag.StatusByUserId = statusByUserId;
            ViewBag.TotalCvs = totalItems;
            ViewBag.Pagination = new PaginationViewModel
            {
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // ================= CRÉER UN POSTE =================
        [HttpGet]
        public IActionResult CreatePoste()
        {
            // Get departments from database
            ViewBag.Departements = _context.Departements
                .Where(d => d.IsActive)
                .OrderBy(d => d.Nom)
                .Select(d => d.Nom)
                .ToList();
            return View(new OffreEmploi());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePoste(OffreEmploi model)
        {
            if (ModelState.IsValid)
            {
                var userId        = int.Parse(HttpContext.Session.GetString("UserId")!);
                model.IdResponsable = userId;
                model.DateCreation  = DateTime.Now;
                model.Statut        = "ACTIF";

                _context.OffresEmploi.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Poste créé avec succès.";
                return RedirectToAction("Postes");
            }
            
            // Re-populate departments on validation error
            ViewBag.Departements = _context.Departements
                .Where(d => d.IsActive)
                .OrderBy(d => d.Nom)
                .Select(d => d.Nom)
                .ToList();
            return View(model);
        }

        // ================= MODIFIER UN POSTE =================
        [HttpGet]
        public IActionResult EditPoste(int id)
        {
            var offre = _context.OffresEmploi.Find(id);
            if (offre == null) return NotFound();
            
            // Get departments from database
            ViewBag.Departements = _context.Departements
                .Where(d => d.IsActive)
                .OrderBy(d => d.Nom)
                .Select(d => d.Nom)
                .ToList();
            return View(offre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPoste(OffreEmploi model)
        {
            if (ModelState.IsValid)
            {
                _context.OffresEmploi.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "Poste modifié avec succès.";
                return RedirectToAction("Postes");
            }
            
            // Re-populate departments on validation error
            ViewBag.Departements = _context.Departements
                .Where(d => d.IsActive)
                .OrderBy(d => d.Nom)
                .Select(d => d.Nom)
                .ToList();
            return View(model);
        }

        // ================= SUPPRIMER UN POSTE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePoste(int id)
        {
            try
            {
                if (!OffreDeletionHelper.TryDeleteOffre(_context, id, out var err))
                {
                    TempData["Error"] = err ?? "Impossible de supprimer le poste.";
                    return RedirectToAction("Postes");
                }

                TempData["Success"] = "Poste supprimé avec succès.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la suppression du poste: {ex.Message}";
            }

            return RedirectToAction("Postes");
        }

        // ================= SUPPRIMER PLUSIEURS POSTES =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePostesSelection(string selectedIds)
        {
            if (string.IsNullOrEmpty(selectedIds))
            {
                TempData["Error"] = "Aucun poste sélectionné.";
                return RedirectToAction("Postes");
            }

            var ids = selectedIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.Parse(s))
                .ToList();

            var deleted = 0;
            foreach (var id in ids)
            {
                if (OffreDeletionHelper.TryDeleteOffre(_context, id, out _))
                    deleted++;
            }

            if (deleted > 0)
                TempData["Success"] = $"{deleted} poste(s) supprimé(s) avec succès.";
            else
                TempData["Error"] = "Aucun poste n'a pu être supprimé.";

            return RedirectToAction("Postes");
        }

        // ================= RÉSULTATS CV =================
        public IActionResult ResultatsCV(int? offreId, int page = 1)
        {
            const int pageSize = 10;
            page = page < 1 ? 1 : page;

            var offres = _context.OffresEmploi.ToList();
            ViewBag.Offres = offres;

            if (offreId.HasValue)
            {
                MatchIntegrationHelper.EnsureMatchesForOffreAsync(_context, offreId.Value).GetAwaiter().GetResult();

                var query = _context.Matches
                    .Include(m => m.Cv)
                        .ThenInclude(c => c.DonneesCv)
                    .Include(m => m.Cv)
                        .ThenInclude(c => c.Offre)
                    .Where(m => m.Cv.OffreId == offreId.Value)
                    .OrderByDescending(m => m.GlobalScore);

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var matches = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.SelectedOffreId = offreId.Value;
                ViewBag.Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
                return View(matches);
            }

            return View(new List<Match>());
        }

        // ================= RÉSULTAT DÉTAILLÉ D'UN CANDIDAT =================
        public IActionResult CvResult(int offreId, int cvId)
        {
            MatchIntegrationHelper.EnsureMatchForCvAsync(_context, offreId, cvId).GetAwaiter().GetResult();
            _context.SaveChanges();

            var match = _context.Matches
                .Include(m => m.Cv)
                    .ThenInclude(c => c.DonneesCv)
                .Include(m => m.Offre)
                .FirstOrDefault(m => m.OffreId == offreId && m.CvId == cvId);

            if (match == null) return NotFound();
            return View("~/Views/Admin/CvResult.cshtml", match);
        }

        // ================= ACCEPTER / REFUSER CANDIDATURE (RH) =================
        // Note: This uses existing CV status fields and stored Match scores.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AccepterCandidature(int cvId, int offreId)
        {
            var cv = _context.Cvs.FirstOrDefault(c => c.Id == cvId && c.OffreId == offreId);
            if (cv == null) return NotFound();

            cv.Statut = "Accepte";
            _context.SaveChanges();

            TempData["Success"] = "Candidature acceptée.";
            return RedirectToAction("DetailPoste", new { id = offreId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RefuserCandidature(int cvId, int offreId)
        {
            var cv = _context.Cvs.FirstOrDefault(c => c.Id == cvId && c.OffreId == offreId);
            if (cv == null) return NotFound();

            cv.Statut = "Refuse";
            _context.SaveChanges();

            TempData["Success"] = "Candidature refusée.";
            return RedirectToAction("DetailPoste", new { id = offreId });
        }

        // ================= PROFIL CANDIDAT =================
        [HttpGet]
        public async Task<IActionResult> ProfilCandidat(int id)
        {
            var candidat = await _context.Utilisateurs.FindAsync(id);
            if (candidat == null || candidat.Role != "Candidat")
            {
                return NotFound();
            }

            var latestCv = await _context.Cvs
                .Include(c => c.Matches)
                .Where(c => c.UtilisateurId == id)
                .OrderByDescending(c => c.UploadDate)
                .FirstOrDefaultAsync();

            var latestMatch = latestCv?.Matches
                .OrderByDescending(m => m.GlobalScore)
                .FirstOrDefault();

            if (latestMatch != null)
            {
                return RedirectToAction(nameof(CvResult), new { offreId = latestMatch.OffreId, cvId = latestMatch.CvId });
            }

            return View(candidat);
        }

        // ================= PROFIL =================
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            var user   = _context.Utilisateurs.Find(userId);
            if (user == null) return NotFound();

            var vm = new ProfileEditViewModel
            {
                Id             = user.Id,
                NomUtilisateur = user.NomUtilisateur,
                Email          = user.Email,
                CurrentPhotoUrl = user.PhotoUrl
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Utilisateurs.Find(model.Id);
                if (user == null) return NotFound();

                if (_context.Utilisateurs.Any(u => u.NomUtilisateur == model.NomUtilisateur && u.Id != model.Id))
                {
                    ModelState.AddModelError("NomUtilisateur", "Ce nom d'utilisateur est déjà utilisé.");
                    return View(model);
                }

                if (_context.Utilisateurs.Any(u => u.Email == model.Email && u.Id != model.Id))
                {
                    ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                    return View(model);
                }

                user.NomUtilisateur = model.NomUtilisateur;
                user.Email = model.Email;

                var passwordChanged = false;

                // Handle password change
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    // Verify current password
                    if (string.IsNullOrEmpty(model.CurrentPassword))
                    {
                        ModelState.AddModelError("CurrentPassword", "Le mot de passe actuel est requis pour changer le mot de passe.");
                        return View(model);
                    }

                    // Verify current password using BCrypt
                    if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.MotPasse))
                    {
                        ModelState.AddModelError("CurrentPassword", "Le mot de passe actuel est incorrect.");
                        return View(model);
                    }

                    // Hash and save new password
                    user.MotPasse = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    passwordChanged = true;
                }

                // Handle profile image upload
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(fileStream);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(user.PhotoUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PhotoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image path
                    user.PhotoUrl = $"/uploads/profiles/{uniqueFileName}";
                }

                _context.SaveChanges();

                HttpContext.Session.SetString("Username", user.NomUtilisateur);
                HttpContext.Session.SetString("UserProfileImage", user.PhotoUrl ?? "");

                TempData["Success"] = passwordChanged
                    ? "Mot de passe modifié avec succès."
                    : "Profil mis à jour avec succès.";
                return RedirectToAction("Profile");
            }
            return View(model);
        }
    }
}