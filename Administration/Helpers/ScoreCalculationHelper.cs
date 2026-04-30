using Administration.Models;
using System.Text.RegularExpressions;

namespace Administration.Helpers
{
    public static class ScoreCalculationHelper
    {
        public static (float diploma, float experience, float skills, float global) CalculateScores(Cv cvData, OffreEmploi? offre)
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
    }
}
