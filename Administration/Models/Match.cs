namespace Administration.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int CvId { get; set; }
        public int OffreId { get; set; }

        public float CompetenceScore { get; set; }
        public float DiplomeScore { get; set; }
        public float ExperienceScore { get; set; }
        public float GlobalScore { get; set; }

        // Bonus scores for enhanced matching
        public float BonusScore { get; set; }
        public float SkillsBonusScore { get; set; }
        public float EducationBonusScore { get; set; }

        public virtual Cv Cv { get; set; } = null!;
        public virtual OffreEmploi Offre { get; set; } = null!;
    }
}