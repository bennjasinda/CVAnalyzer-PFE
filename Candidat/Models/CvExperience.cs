using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CvParsing.Models;

/// <summary>
/// Represents a professional experience entry extracted from a CV
/// </summary>
[Table("CvExperiences")]
public class CvExperience
{
    [Key]
    public int Id { get; set; }

    public int CvId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string? Position { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    [ForeignKey(nameof(CvId))]
    public virtual Cv Cv { get; set; } = null!;
}
