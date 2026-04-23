using Administration.Models;

namespace Administration.ViewModels
{
    public class DashboardStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalRH { get; set; }
        public int TotalDirecteurs { get; set; }
        public int TotalOffres { get; set; }
        public int TotalCvs { get; set; }
        public int TotalMatches { get; set; }
        public int TotalAcceptes { get; set; }
        public int TotalRefuses { get; set; }
        public List<Cv> CandidatsAcceptes { get; set; } = new();
    }
}