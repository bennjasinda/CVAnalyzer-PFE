using Administration.Models;
using System.Text.RegularExpressions;

namespace Administration.Services
{
    /// <summary>
    /// Service for extracting and storing structured CV data from Python extraction results
    /// </summary>
    public class CvDataExtractionService
    {
        /// <summary>
        /// Normalize and split extracted skills/competences into a list of names.
        /// Input comes from Python extraction output (string or list joined as string).
        /// </summary>
        public static List<string> ExtractCompetenceNames(string? competencesText)
        {
            if (string.IsNullOrWhiteSpace(competencesText))
                return new List<string>();

            var parts = competencesText.Split(
                new[] { ',', ';', '|', '/', '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            return parts
                .Select(p => Regex.Replace(p.Trim(), @"\s+", " "))
                .Where(p => p.Length > 1)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Extract and store experiences from the CV data
        /// </summary>
        public static List<CvExperience> ExtractExperiences(int cvId, string? experienceText)
        {
            var experiences = new List<CvExperience>();

            if (string.IsNullOrWhiteSpace(experienceText))
                return experiences;

            // Split experiences by common separators
            var experienceItems = SplitExperienceItems(experienceText);

            foreach (var item in experienceItems)
            {
                var trimmed = item.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                var experience = new CvExperience
                {
                    CvId = cvId,
                    Description = trimmed,
                    Company = ExtractCompany(trimmed),
                    Position = ExtractPosition(trimmed),
                    StartDate = ExtractStartDate(trimmed),
                    EndDate = ExtractEndDate(trimmed),
                    IsCurrent = IsCurrentPosition(trimmed)
                };

                experiences.Add(experience);
            }

            return experiences;
        }

        /// <summary>
        /// Extract and store diplomas from the CV data
        /// </summary>
        public static List<CvDiplome> ExtractDiplomes(int cvId, string? educationText, string? autresInfos)
        {
            var diplomes = new List<CvDiplome>();

            if (string.IsNullOrWhiteSpace(educationText))
                return diplomes;

            // Split diplomas by common separators
            var diplomaItems = SplitDiplomaItems(educationText);

            foreach (var item in diplomaItems)
            {
                var trimmed = item.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                var diplome = new CvDiplome
                {
                    CvId = cvId,
                    Designation = trimmed,
                    Institution = ExtractInstitution(trimmed),
                    Field = ExtractField(trimmed),
                    YearObtained = ExtractYear(trimmed),
                    Mention = ExtractMention(trimmed)
                };

                diplomes.Add(diplome);
            }

            return diplomes;
        }

        #region Helper Methods

        private static List<string> SplitExperienceItems(string text)
        {
            // Split by newlines or numbered lists
            var items = Regex.Split(text, @"\n|\r\n|\d+\.\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (items.Count <= 1)
            {
                // Try splitting by common experience keywords
                items = Regex.Split(text, @"(?=De\s|À\s|Chez\s|Dans\s)", RegexOptions.IgnoreCase)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
            }

            return items;
        }

        private static List<string> SplitDiplomaItems(string text)
        {
            // Split by newlines or common separators
            var items = Regex.Split(text, @"\n|\r\n|;")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            return items;
        }

        private static string? ExtractCompany(string text)
        {
            // Try to extract company name (after "chez", "à", "dans")
            var match = Regex.Match(text, @"(?:chez|à|dans|at)\s+([A-Z][a-zA-Z\s&.,]+?)(?:\s*[-|,]|\s*$)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return null;
        }

        private static string? ExtractPosition(string text)
        {
            // Try to extract position/title
            var match = Regex.Match(text, @"(?:poste|position|role|titre)[\s:]+([^,\n]+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            // Or first part before company
            var parts = text.Split(new[] { "chez", "à", "dans", "at" }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                return parts[0].Trim();

            return null;
        }

        private static DateTime? ExtractStartDate(string text)
        {
            // Try to extract date patterns like "Jan 2020", "01/2020", "2020"
            var match = Regex.Match(text, @"((?:Jan|Fev|Mar|Avr|Mai|Jun|Jul|Aou|Sep|Oct|Nov|Dec)[a-z]*\s+\d{4}|\d{1,2}/\d{4}|\d{4})");
            if (match.Success)
            {
                if (DateTime.TryParse(match.Groups[1].Value, out var date))
                    return date;
            }

            return null;
        }

        private static DateTime? ExtractEndDate(string text)
        {
            // Try to extract end date patterns
            var match = Regex.Match(text, @"(?:à|jusqu'en?|until|to)\s+((?:Jan|Fev|Mar|Avr|Mai|Jun|Jul|Aou|Sep|Oct|Nov|Dec)[a-z]*\s+\d{4}|\d{1,2}/\d{4}|\d{4}|Present|Actuel|Now)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var value = match.Groups[1].Value;
                if (value.Equals("present", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("actuel", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("now", StringComparison.OrdinalIgnoreCase))
                    return null; // Current position

                if (DateTime.TryParse(value, out var date))
                    return date;
            }

            return null;
        }

        private static bool IsCurrentPosition(string text)
        {
            return Regex.IsMatch(text, @"(?:Present|Actuel|Now|En\s+cours|Current)", RegexOptions.IgnoreCase);
        }

        private static string? ExtractInstitution(string text)
        {
            // Try to extract institution/university name
            var match = Regex.Match(text, @"(?:université|école|institut|university|school|college)\s+([^\n,;]+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return null;
        }

        private static string? ExtractField(string text)
        {
            // Try to extract field of study
            var match = Regex.Match(text, @"(?:en|dans|field\s+of|major)\s+([^\n,;]+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return null;
        }

        private static int? ExtractYear(string text)
        {
            // Try to extract year
            var match = Regex.Match(text, @"\b(20\d{2}|19\d{2})\b");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var year))
                return year;

            return null;
        }

        private static string? ExtractMention(string text)
        {
            // Try to extract mention/grade
            var match = Regex.Match(text, @"(?:mention|grade|note)[\s:]+([^,\n;]+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            return null;
        }

        #endregion
    }
}
