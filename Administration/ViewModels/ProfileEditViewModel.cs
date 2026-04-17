using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Administration.ViewModels
{
    public class ProfileEditViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Nom d'utilisateur")]
        public string NomUtilisateur { get; set; } = string.Empty;
        
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Téléphone")]
        public string? Phone { get; set; }
        
        [Display(Name = "Photo de profil")]
        public IFormFile? ProfileImage { get; set; }
        
        [Display(Name = "Département")]
        public string Department { get; set; } = string.Empty;
        
        [Display(Name = "Mot de passe actuel")]
        [Required(ErrorMessage = "Le mot de passe actuel est requis pour changer le mot de passe.")]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; } = string.Empty;
        
        [Display(Name = "Confirmer le nouveau mot de passe")]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
        
        // Property to store the current profile image path
        public string? CurrentPhotoUrl { get; set; }
    }
}