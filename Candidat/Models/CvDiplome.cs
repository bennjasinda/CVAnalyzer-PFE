using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CvParsing.Models;

/// <summary>
/// Represents a diploma/education entry extracted from a CV
/// </summary>
[Table("CvDiplomes")]
public class CvDiplome
{
    [Key]
    public int Id { get; set; }

    public int CvId { get; set; }

    [Required]
    public string Designation { get; set; } = string.Empty;

    public string? Institution { get; set; }

    public string? Field { get; set; }

    public int? YearObtained { get; set; }

    public string? Mention { get; set; }

    [ForeignKey(nameof(CvId))]
    public virtual Cv Cv { get; set; } = null!;
}
