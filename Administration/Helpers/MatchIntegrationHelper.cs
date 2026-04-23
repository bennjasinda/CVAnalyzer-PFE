using Administration.Data;
using Administration.Models;
using Administration.Services;
using Microsoft.EntityFrameworkCore;

namespace Administration.Helpers
{
    public static class MatchIntegrationHelper
    {
        public static async Task EnsureMatchForCvAsync(ApplicationDbContext context, int offreId, int cvId)
        {
            var existing = await context.Matches.FirstOrDefaultAsync(m => m.OffreId == offreId && m.CvId == cvId);
            if (existing != null)
            {
                return;
            }

            // Get CV data and calculate enhanced scores
            var cv = await context.Cvs
                .Include(c => c.DonneesCv)
                .Include(c => c.CvExperiences)
                .Include(c => c.CvDiplomes)
                .FirstOrDefaultAsync(c => c.Id == cvId);

            var offre = await context.OffresEmploi.FindAsync(offreId);

            if (cv?.DonneesCv != null && offre != null)
            {
                var experiences = cv.CvExperiences?.ToList();
                var diplomes = cv.CvDiplomes?.ToList();

                var enhancedScores = EnhancedScoringEngine.CalculateEnhancedScores(
                    cv.DonneesCv, offre, experiences, diplomes);

                context.Matches.Add(new Match
                {
                    OffreId = offreId,
                    CvId = cvId,
                    CompetenceScore = enhancedScores.skills,
                    DiplomeScore = enhancedScores.diploma,
                    ExperienceScore = enhancedScores.experience,
                    BonusScore = enhancedScores.bonus,
                    SkillsBonusScore = enhancedScores.skillsBonus,
                    EducationBonusScore = enhancedScores.educationBonus,
                    GlobalScore = enhancedScores.global
                });
            }
            else
            {
                // Create a safe placeholder row when scoring is missing.
                // This prevents 404 pages and guarantees DB/UI consistency.
                context.Matches.Add(new Match
                {
                    OffreId = offreId,
                    CvId = cvId,
                    CompetenceScore = 0f,
                    DiplomeScore = 0f,
                    ExperienceScore = 0f,
                    BonusScore = 0f,
                    SkillsBonusScore = 0f,
                    EducationBonusScore = 0f,
                    GlobalScore = 0f
                });
            }
        }

        public static async Task EnsureMatchesForOffreAsync(ApplicationDbContext context, int offreId)
        {
            var cvIds = await context.Cvs
                .Where(c => c.OffreId == offreId)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var cvId in cvIds)
            {
                await EnsureMatchForCvAsync(context, offreId, cvId);
            }

            await context.SaveChangesAsync();
        }
    }
}
