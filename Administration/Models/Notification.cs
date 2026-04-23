using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Administration.Models
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int UtilisateurId { get; set; }

        public string Titre { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info";
        public bool IsRead { get; set; } = false;
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public int? RelatedCvId { get; set; }
        public int? RelatedOffreId { get; set; }
    }
}
