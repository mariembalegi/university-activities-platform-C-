using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniHub.BL.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        [Display(Name = "Prénom")]
        public string? FirstName { get; set; }

        [StringLength(50)]
        [Display(Name = "Nom")]
        public string? LastName { get; set; }

        public int? UsernameChangeLimit { get; set; } = 10;

        [Display(Name = "Photo de profil")]
        public byte[]? ProfilePicture { get; set; }

        [Display(Name = "Département")]
        public Department? Department { get; set; }

        [StringLength(20)]
        [Display(Name = "Numéro étudiant")]
        public string? StudentNumber { get; set; }

        // Navigation property - Un admin peut créer plusieurs activités
        public virtual ICollection<Activity> CreatedActivities { get; set; } = new List<Activity>();

        // Navigation property - Un étudiant peut avoir plusieurs inscriptions
        public virtual ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();

        [NotMapped]
        [Display(Name = "Nom complet")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
