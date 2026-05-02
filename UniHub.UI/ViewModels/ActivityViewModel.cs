using System.ComponentModel.DataAnnotations;

namespace UniHub.UI.ViewModels
{
    public class ActivityViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Titre")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
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
    }
}
