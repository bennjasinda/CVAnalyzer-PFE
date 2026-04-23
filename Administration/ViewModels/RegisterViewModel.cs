using System.ComponentModel.DataAnnotations;
using Administration.Validation;

namespace Administration.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string NomUtilisateur { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StrongPassword]
        public string MotPasse { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("MotPasse")]
        public string ConfirmMotPasse { get; set; } = string.Empty;

        public string Role { get; set; } = "Candidat";
    }
}