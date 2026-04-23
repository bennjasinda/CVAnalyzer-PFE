using Administration.Models;
using System.Text.RegularExpressions;

namespace Administration.Services
{
    /// <summary>
    /// Enhanced scoring engine with bonus system for CV matching
    /// Calculates scores based on diploma, experience, and skills with bonus points
    /// </summary>
    public class EnhancedScoringEngine
    {
        /// <summary>
        /// Calculate comprehensive match scores with bonus system
        /// </summary>
        public static (float diploma, float experience, float skills, float bonus, float skillsBonus, float educationBonus, float global) 
            CalculateEnhancedScores(DonneesCv cvData, OffreEmploi? offre, List<CvExperience>? experiences, List<CvDiplome>? diplomes)
        {
            if (offre == null)
            {
                return (0f, 0f, 0f, 0f, 0f, 0f, 0f);
            }

            static float Clamp(float value) => MathF.Max(0f, MathF.Min(100f, value));

            // Base scores
            var diplomaScore = CalculateDiplomaScore(cvData.NiveauEducation, offre.NiveauEducation);
            var experienceScore = CalculateExperienceScore(cvData.Experience, offre.Experience);
            var skillsScore = CalculateSkillsScore(cvData.Competences, offre.Description);

            // Bonus scores
            var educationBonus = CalculateEducationBonus(cvData.NiveauEducation, offre.NiveauEducation, diplomes);
            var skillsBonus = CalculateSkillsBonus(cvData.Competences, offre.Description);
            var totalBonus = Clamp(educationBonus + skillsBonus);

            // Calculate global score with weighted components
            // Base: 30% diploma, 30% experience, 40% skills
            // Bonus: added to global score (max 20 points)
            var baseGlobalScore = Clamp((diplomaScore * 0.30f) + (experienceScore * 0.30f) + (skillsScore * 0.40f));
            var finalGlobalScore = Clamp(baseGlobalScore + (totalBonus * 0.20f)); // Bonus contributes up to 20% of final score

            return (diplomaScore, experienceScore, skillsScore, totalBonus, skillsBonus, educationBonus, finalGlobalScore);
        }

        #region Base Score Calculations

        private static float CalculateDiplomaScore(string? candidateEducation, string? requiredEducation)
        {
            static float Clamp(float value) => MathF.Max(0f, MathF.Min(100f, value));

            var candidateRank = EducationRank(candidateEducation);
            var requiredRank = EducationRank(requiredEducation);

            if (requiredRank <= 0)
                return 100f; // No requirement specified

            return Clamp((candidateRank / (float)requiredRank) * 100f);
        }

        private static float CalculateExperienceScore(string? candidateExperience, int requiredYears)
        {
            static float Clamp(float value) => MathF.Max(0f, MathF.Min(100f, value));

            var candidateYears = ExtractYears(candidateExperience);
            var required = Math.Max(0, requiredYears);

            if (required <= 0)
                return 100f; // No requirement specified

            return Clamp((candidateYears / (float)required) * 100f);
        }

        private static float CalculateSkillsScore(string? candidateSkills, string? jobDescription)
        {
            static float Clamp(float value) => MathF.Max(0f, MathF.Min(100f, value));

            var candidateSkillSet = ParseSkills(candidateSkills);
            var requiredSkillSet = ParseSkills(jobDescription);

            if (requiredSkillSet.Count == 0)
            {
                return candidateSkillSet.Count > 0 ? 100f : 50f;
            }

            var matched = candidateSkillSet.Intersect(requiredSkillSet).Count();
            return Clamp((matched / (float)requiredSkillSet.Count) * 100f);
        }

        #endregion

        #region Bonus Calculations

        /// <summary>
        /// Calculate bonus for education level exceeding requirements
        /// </summary>
        private static float CalculateEducationBonus(string? candidateEducation, string? requiredEducation, List<CvDiplome>? diplomes)
        {
            var candidateRank = EducationRank(candidateEducation);
            var requiredRank = EducationRank(requiredEducation);

            float bonus = 0f;

            // Bonus for higher education level
            if (candidateRank > requiredRank && requiredRank > 0)
            {
                var levelsAbove = candidateRank - requiredRank;
                bonus += Math.Min(levelsAbove * 10f, 30f); // Max 30 points bonus for education
            }

            // Bonus for multiple relevant diplomas
            if (diplomes != null && diplomes.Count > 1)
            {
                bonus += Math.Min((diplomes.Count - 1) * 5f, 15f); // Max 15 points for multiple diplomas
            }

            // Bonus for prestigious institutions (keywords)
            if (diplomes != null)
            {
                foreach (var diplome in diplomes)
                {
                    if (!string.IsNullOrWhiteSpace(diplome.Institution))
                    {
                        var institution = diplome.Institution.ToLowerInvariant();
                        if (institution.Contains("université") || institution.Contains("grande école") || 
                            institution.Contains("engineering") || institution.Contains("polytechnique"))
                        {
                            bonus += 5f;
                        }
                    }

                    // Bonus for mentions/honors
                    if (!string.IsNullOrWhiteSpace(diplome.Mention))
                    {
                        var mention = diplome.Mention.ToLowerInvariant();
                        if (mention.Contains("très bien") || mention.Contains("excellent"))
                            bonus += 5f;
                        else if (mention.Contains("bien") || mention.Contains("good"))
                            bonus += 3f;
                        else if (mention.Contains("assez bien"))
                            bonus += 2f;
                    }
                }
            }

            return Math.Min(bonus, 50f); // Cap education bonus at 50 points
        }

        /// <summary>
        /// Calculate bonus for additional skills beyond requirements
        /// </summary>
        private static float CalculateSkillsBonus(string? candidateSkills, string? jobDescription)
        {
            var candidateSkillSet = ParseSkills(candidateSkills);
            var requiredSkillSet = ParseSkills(jobDescription);

            float bonus = 0f;

            if (requiredSkillSet.Count > 0)
            {
                // Count extra skills (skills candidate has that aren't required)
                var extraSkills = candidateSkillSet.Except(requiredSkillSet).Count();
                
                // Bonus for extra relevant skills
                if (extraSkills > 0)
                {
                    bonus += Math.Min(extraSkills * 3f, 30f); // Max 30 points for extra skills
                }
            }
            else
            {
                // If no specific skills required, bonus for having many skills
                if (candidateSkillSet.Count > 5)
                {
                    bonus += Math.Min(candidateSkillSet.Count * 2f, 20f);
                }
            }

            // Bonus for in-demand skills (tech keywords)
            var inDemandSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "python", "java", "javascript", "typescript", "c#", "c++", "react", "angular", "vue",
                "node.js", "docker", "kubernetes", "aws", "azure", "machine learning", "ai",
                "data science", "sql", "mongodb", "git", "agile", "scrum"
            };

            var matchingInDemand = candidateSkillSet.Intersect(inDemandSkills).Count();
            bonus += Math.Min(matchingInDemand * 2f, 20f); // Max 20 points for in-demand skills

            return Math.Min(bonus, 50f); // Cap skills bonus at 50 points
        }

        #endregion

        #region Helper Methods

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

        #endregion
    }
}
