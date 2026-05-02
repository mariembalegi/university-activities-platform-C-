using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniHub.BL.Entities
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Date de début")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Date de fin")]
        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        [StringLength(200)]
        [Display(Name = "Lieu")]
        public string? Location { get; set; }

        [Display(Name = "Nombre maximum de participants")]
        public int? MaxParticipants { get; set; }

        [Display(Name = "Date de création")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Date de modification")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "Publié")]
        public bool IsPublished { get; set; } = true;

        // Foreign Keys
        [Required]
        [Display(Name = "Département")]
        public Department Department { get; set; }

        [Required]
        [Display(Name = "Créé par")]
        public string CreatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser CreatedByUser { get; set; }
    }
}
