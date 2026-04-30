using Administration.Data;
using Administration.Models;
using Microsoft.EntityFrameworkCore;

namespace Administration.Helpers
{
    public static class MatchIntegrationHelper
    {
        public static async Task EnsureMatchForCvAsync(ApplicationDbContext context, int offreId, int cvId)
        {
            if (cvId <= 0)
            {
                return;
            }

            var cv = await context.Cvs
                .Include(c => c.Offre)
                .FirstOrDefaultAsync(c => c.Id == cvId && c.OffreId == offreId);
            
            if (cv == null)
            {
                return;
            }

            var existing = await context.Matches.FirstOrDefaultAsync(m => m.OffreId == offreId && m.CvId == cvId);
            
            // If match already exists and has scores, don't recalculate
            if (existing != null && existing.GlobalScore > 0)
            {
                return;
            }

            // Calculate scores using the CV data and offer requirements
            var scores = ScoreCalculationHelper.CalculateScores(cv, cv.Offre);

            if (existing != null)
            {
                // Update existing match with calculated scores
                existing.CompetenceScore = scores.skills;
                existing.DiplomeScore = scores.diploma;
                existing.ExperienceScore = scores.experience;
                existing.GlobalScore = scores.global;
            }
            else
            {
                // Create new match with calculated scores
                context.Matches.Add(new Match
                {
                    OffreId = offreId,
                    CvId = cvId,
                    CompetenceScore = scores.skills,
                    DiplomeScore = scores.diploma,
                    ExperienceScore = scores.experience,
                    GlobalScore = scores.global
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
