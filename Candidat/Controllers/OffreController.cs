using Microsoft.AspNetCore.Mvc;
using CvParsing.Data;
using CvParsing.Models;
using CvParsing.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace CvParsing.Controllers;

public class OffreController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public OffreController(AppDbContext context, IWebHostEnvironment env, IConfiguration configuration)
    {
        _context = context;
        _env = env;
        _configuration = configuration;
    }

    // ✅ ONLY CHANGE: filter ACTIF for candidates
    public IActionResult Index()
    {
        var offres = _context.OffresEmploi
            .Where(o => o.Statut == "ACTIF")
            .ToList();

        return View("~/Views/Offre/offer.cshtml", offres);
    }

    public IActionResult Details(int id)
    {
        var offre = _context.OffresEmploi.FirstOrDefault(o => o.Id == id);

        if (offre == null)
            return NotFound();

        ViewData["OffreId"] = id;

        var userId = HttpContext.Session.GetString("UserId");
        ViewBag.IsLoggedIn = !string.IsNullOrEmpty(userId);
        ViewBag.NomUtilisateur = HttpContext.Session.GetString("UserName");
        ViewBag.EmailUtilisateur = HttpContext.Session.GetString("UserEmail");

        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var uid))
        {
            var existing = _context.Cvs.AsNoTracking()
                .FirstOrDefault(c => c.UtilisateurId == uid && c.OffreId == id);

            ViewBag.HasCvSubmitted = existing != null;
        }
        else
        {
            ViewBag.HasCvSubmitted = false;
        }

        return View("~/Views/Offre/offer-details.cshtml", offre);
    }

    // ✅ ONLY CHANGE: filter ACTIF
    [HttpGet]
    public IActionResult Search(string? q, string? departement, string? typeContrat)
    {
        var all = _context.OffresEmploi
            .AsNoTracking()
            .Where(o => o.Statut == "ACTIF")
            .ToList();

        var vm = new OffreSearchResultsViewModel
        {
            Query = q,
            Departement = departement,
            TypeContrat = typeContrat,
            Departements = all
                .Select(o => (o.Departement ?? "").Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .Select(d => new SelectListItem { Value = d, Text = d, Selected = string.Equals(d, departement, StringComparison.OrdinalIgnoreCase) })
                .ToList(),

            TypesContrat = all
                .Select(o => (o.Type ?? "").Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .Select(t => new SelectListItem { Value = t, Text = t, Selected = string.Equals(t, typeContrat, StringComparison.OrdinalIgnoreCase) })
                .ToList()
        };

        IEnumerable<OffreEmploi> filtered = all;

        if (!string.IsNullOrWhiteSpace(departement))
        {
            filtered = filtered.Where(o =>
                string.Equals((o.Departement ?? "").Trim(), departement.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(typeContrat))
        {
            filtered = filtered.Where(o =>
                string.Equals((o.Type ?? "").Trim(), typeContrat.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        var query = (q ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(o =>
                IsFuzzyMatch(query, (o.Titre ?? "") + " " + (o.Description ?? "")));
        }

        vm.Results = filtered
            .OrderByDescending(o => o.DateCreation)
            .ToList();

        return View("~/Views/Offre/search-results.cshtml", vm);
    }

    [HttpPost]
    public async Task<IActionResult> UploadCv(int offreId, string nomComplet,
        string email, string? telephone, string? competences, string? experience,
        string? niveauEducation, string? autresInfos, IFormFile cvFile)
    {
        ViewData["OffreId"] = offreId;
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account",
                new { returnUrl = $"/Offre/Details/{offreId}" });

        var offre = _context.OffresEmploi.FirstOrDefault(o => o.Id == offreId);

        // Validation des champs obligatoires
        if (string.IsNullOrWhiteSpace(nomComplet))
        {
            ViewBag.UploadError = "Le nom complet est obligatoire.";
            ViewBag.IsLoggedIn = true;
            return View("~/Views/Offre/offer-details.cshtml", offre);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ViewBag.UploadError = "L'email est obligatoire.";
            ViewBag.IsLoggedIn = true;
            return View("~/Views/Offre/offer-details.cshtml", offre);
        }

        if (cvFile == null || cvFile.Length == 0)
        {
            ViewBag.UploadError = "Veuillez sélectionner un fichier CV.";
            ViewBag.IsLoggedIn = true;
            return View("~/Views/Offre/offer-details.cshtml", offre);
        }

        var ext = Path.GetExtension(cvFile.FileName).ToLowerInvariant();
        if (ext != ".pdf")
        {
            ViewBag.UploadError = "Format non accepté. Veuillez utiliser le format PDF uniquement pour l'extraction automatique des données.";
            ViewBag.IsLoggedIn = true;
            return View("~/Views/Offre/offer-details.cshtml", offre);
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cvs");
        Directory.CreateDirectory(uploadsFolder);
        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        var uid = int.Parse(userId);
        var previous = _context.Cvs.FirstOrDefault(c => c.UtilisateurId == uid && c.OffreId == offreId);

        if (previous != null)
        {
            var previousMatches = _context.Matches.Where(m => m.CvId == previous.Id).ToList();
            if (previousMatches.Count > 0)
            {
                _context.Matches.RemoveRange(previousMatches);
            }

            if (!string.IsNullOrWhiteSpace(previous.CheminFichier))
            {
                var rel = previous.CheminFichier.TrimStart('~').TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var physical = Path.Combine(_env.WebRootPath, rel);
                if (System.IO.File.Exists(physical))
                {
                    try { System.IO.File.Delete(physical); } catch { }
                }
            }

            _context.Cvs.Remove(previous);
            await _context.SaveChangesAsync();
        }

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await cvFile.CopyToAsync(stream);
        }

        // Extract data from CV file (PDF only)
        var extracted = ext == ".pdf"
            ? ExtractStructuredDataFromPdf(filePath, offre?.Description)
            : new StructuredCvData();

        // Validate that extraction succeeded
        if (string.IsNullOrWhiteSpace(extracted.Competences) && 
            string.IsNullOrWhiteSpace(extracted.Experience) && 
            string.IsNullOrWhiteSpace(extracted.Diplomes))
        {
            // Clean up uploaded file
            if (System.IO.File.Exists(filePath))
            {
                try { System.IO.File.Delete(filePath); } catch { }
            }

            ViewBag.UploadError = "Impossible d'extraire les informations de votre CV. Veuillez vérifier que votre CV contient vos compétences, expérience et formation.";
            ViewBag.IsLoggedIn = true;
            return View("~/Views/Offre/offer-details.cshtml", offre);
        }

        var newCv = new Cv
        {
            OffreId = offreId,
            UtilisateurId = uid,
            CheminFichier = $"/uploads/cvs/{uniqueFileName}",
            UploadDate = DateTime.Now,
            NomCandidat = nomComplet,
            Email = email,
            Telephone = telephone,
            Competences = extracted.Competences,
            Experience = extracted.Experience,
            NiveauEducation = extracted.Diplomes,
            AutresInfos = null
        };

        _context.Cvs.Add(newCv);
        await _context.SaveChangesAsync();

        var detailedScores = CalculateScores(newCv, offre);
        var match = await _context.Matches.FirstOrDefaultAsync(m => m.CvId == newCv.Id && m.OffreId == offreId);
        if (match == null)
        {
            match = new CvParsing.Models.Match
            {
                CvId = newCv.Id,
                OffreId = offreId
            };
            _context.Matches.Add(match);
        }

        match.DiplomeScore = detailedScores.diploma;
        match.ExperienceScore = detailedScores.experience;
        match.CompetenceScore = detailedScores.skills;
        match.GlobalScore = detailedScores.global;
        await _context.SaveChangesAsync();

        // Notify directors linked to this offer department.
        var offerDept = offre?.Departement?.Trim();
        if (!string.IsNullOrWhiteSpace(offerDept))
        {
            var directorIds = _context.Utilisateurs
                .Where(u => u.Role == "Directeur" && !string.IsNullOrWhiteSpace(u.Departements))
                .ToList()
                .Where(u => u.Departements!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Any(d => string.Equals(d, offerDept, StringComparison.OrdinalIgnoreCase)))
                .Select(u => u.Id)
                .Distinct()
                .ToList();

            foreach (var directorId in directorIds)
            {
                _context.Notifications.Add(new Notification
                {
                    RecipientUserId = directorId,
                    Title = "Nouvelle candidature liée à votre département",
                    Message = $"Un candidat a postulé à l'offre \"{offre?.Titre}\" ({offerDept}).",
                    Type = "DirectorApplication",
                    LinkUrl = $"/DirecteurDepartement/CvResult?offreId={offreId}&cvId={newCv.Id}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }

            if (directorIds.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        TempData["CvSubmitted"] = "1";
        return RedirectToAction(nameof(Details), new { id = offreId });
    }

    private static (float diploma, float experience, float skills, float global) CalculateScores(Cv cvData, OffreEmploi? offre)
    {
        if (offre == null)
        {
            return (0f, 0f, 0f, 0f);
        }

        static float Clamp(float value) => MathF.Max(0f, MathF.Min(100f, value));

        // Calculate individual scores
        var candidateEducationRank = EducationRank(cvData.NiveauEducation);
        var requiredEducationRank = EducationRank(offre.NiveauEducation);
        var diplomaScore = requiredEducationRank <= 0
            ? 100f
            : Clamp((candidateEducationRank / (float)requiredEducationRank) * 100f);

        var candidateYears = ExtractYears(cvData.Experience);
        var requiredYears = Math.Max(0, offre.Experience);
        var experienceScore = requiredYears <= 0
            ? 100f
            : Clamp((candidateYears / (float)requiredYears) * 100f);

        var candidateSkills = ParseSkills(cvData.Competences);
        var requiredSkills = ParseSkills(offre.Description);
        float skillsScore;
        if (requiredSkills.Count == 0)
        {
            skillsScore = candidateSkills.Count > 0 ? 100f : 50f;
        }
        else
        {
            var matched = candidateSkills.Intersect(requiredSkills).Count();
            skillsScore = Clamp((matched / (float)requiredSkills.Count) * 100f);
        }

        // Weighted global score: Skills 50%, Experience 30%, Diploma 20%
        var globalScore = Clamp((skillsScore * 0.50f) + (experienceScore * 0.30f) + (diplomaScore * 0.20f));

        // Apply bonus system
        float bonus = 0f;

        // Higher Diploma Bonus: +1 or +2 points
        if (requiredEducationRank > 0 && candidateEducationRank > requiredEducationRank)
        {
            var rankDifference = candidateEducationRank - requiredEducationRank;
            bonus += Math.Min(rankDifference * 1.5f, 3f); // Max +3 points for diploma
        }

        // Fresh Graduate Bonus: +1 point
        if (candidateYears <= 1 && candidateEducationRank >= 2)
        {
            bonus += 1f;
        }

        // Experience Bonus: +1 point
        if (candidateYears > requiredYears && requiredYears > 0)
        {
            bonus += 1f;
        }

        // Apply bonus and clamp to 100
        globalScore = Clamp(globalScore + bonus);

        return (diplomaScore, experienceScore, skillsScore, globalScore);
    }

    private static int EducationRank(string? education)
    {
        var value = (education ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(value)) return 0;
        if (value.Contains("doctor") || value.Contains("phd")) return 5;
        if (value.Contains("ing") || value.Contains("engineer")) return 4;
        if (value.Contains("master") || value.Contains("bac+5")) return 3;
        if (value.Contains("licence") || value.Contains("bachelor") || value.Contains("bac+3")) return 2;
        if (value.Contains("bac")) return 1;
        return 0;
    }

    private static int ExtractYears(string? experience)
    {
        if (string.IsNullOrWhiteSpace(experience)) return 0;
        var match = Regex.Match(experience, @"(\d+)");
        return match.Success && int.TryParse(match.Groups[1].Value, out var years) ? years : 0;
    }

    private static HashSet<string> ParseSkills(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var separators = new[] { ',', ';', '|', '/', '\n' };
        return text.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => s.Length > 1)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    // ===== SEARCH HELPERS (UNCHANGED) =====

    private static bool IsFuzzyMatch(string needle, string haystack)
    {
        var n = NormalizeForSearch(needle);
        if (string.IsNullOrWhiteSpace(n)) return true;

        var h = NormalizeForSearch(haystack);
        if (string.IsNullOrWhiteSpace(h)) return false;

        if (h.Contains(n, StringComparison.Ordinal)) return true;

        var words = h.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var maxDist = n.Length <= 4 ? 1 : (n.Length <= 7 ? 2 : 3);

        foreach (var w in words)
        {
            if (w.StartsWith(n, StringComparison.Ordinal)) return true;

            if (Math.Abs(w.Length - n.Length) > maxDist) continue;

            if (LevenshteinDistance(n, w, maxDist) <= maxDist) return true;
        }

        return false;
    }

    private static string NormalizeForSearch(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var s = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(s.Length);

        foreach (var ch in s)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_' || ch == '/')
                sb.Append(' ');
        }

        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static int LevenshteinDistance(string a, string b, int max)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        if (Math.Abs(a.Length - b.Length) > max) return max + 1;

        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++) prev[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            var best = curr[0];
            var ca = a[i - 1];

            for (var j = 1; j <= b.Length; j++)
            {
                var cost = (ca == b[j - 1]) ? 0 : 1;

                var val = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost
                );

                curr[j] = val;
                if (val < best) best = val;
            }

            if (best > max) return max + 1;

            (prev, curr) = (curr, prev);
        }

        return prev[b.Length];
    }

    private StructuredCvData ExtractStructuredDataFromPdf(string absoluteFilePath, string? jobDescription)
    {
        try
        {
            var scriptPath = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "score_matching.py"));
            if (!System.IO.File.Exists(scriptPath))
            {
                System.Diagnostics.Debug.WriteLine($"Python script not found at: {scriptPath}");
                return new StructuredCvData();
            }

            // Get GROQ API key from configuration or environment
            var groqApiKey = _configuration["Groq:ApiKey"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
            
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? _env.ContentRootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Set GROQ API key as environment variable for Python script
            if (!string.IsNullOrWhiteSpace(groqApiKey))
            {
                psi.EnvironmentVariables["GROQ_API_KEY"] = groqApiKey;
            }

            var safeJob = (jobDescription ?? string.Empty).Replace("\"", "\\\"");
            psi.ArgumentList.Add(scriptPath);
            psi.ArgumentList.Add(absoluteFilePath);
            psi.ArgumentList.Add(safeJob);

            using var process = Process.Start(psi);
            if (process == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to start Python process");
                return new StructuredCvData();
            }

            var output = process.StandardOutput.ReadToEnd();
            var errorOutput = process.StandardError.ReadToEnd();
            process.WaitForExit(30000); // Increased timeout to 30 seconds

            // Log errors for debugging
            if (!string.IsNullOrWhiteSpace(errorOutput))
            {
                System.Diagnostics.Debug.WriteLine($"Python script error: {errorOutput}");
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                System.Diagnostics.Debug.WriteLine("Python script returned empty output");
                return new StructuredCvData();
            }

            var parsed = JsonSerializer.Deserialize<ScoreMatchingOutput>(output, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var sections = parsed?.Sections;
            if (sections == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to parse Python output");
                return new StructuredCvData();
            }

            return new StructuredCvData
            {
                Competences = sections.Competences != null && sections.Competences.Count > 0
                    ? string.Join(", ", sections.Competences.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()))
                    : null,
                Experience = sections.Experiences != null && sections.Experiences.Count > 0
                    ? string.Join(" | ", sections.Experiences.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()))
                    : null,
                Diplomes = sections.Diplomes != null && sections.Diplomes.Count > 0
                    ? string.Join(" | ", sections.Diplomes.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()))
                    : null
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in PDF extraction: {ex.Message}");
            return new StructuredCvData();
        }
    }

    private sealed class StructuredCvData
    {
        public string? Competences { get; set; }
        public string? Experience { get; set; }
        public string? Diplomes { get; set; }
    }

    private sealed class ScoreMatchingOutput
    {
        public ScoreSections? Sections { get; set; }
    }

    private sealed class ScoreSections
    {
        public List<string>? Competences { get; set; }
        public List<string>? Experiences { get; set; }
        public List<string>? Diplomes { get; set; }
    }
}