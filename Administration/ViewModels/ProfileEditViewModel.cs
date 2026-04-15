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
        
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; } = string.Empty;
    }
}