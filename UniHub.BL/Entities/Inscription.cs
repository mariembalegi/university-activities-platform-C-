using System.ComponentModel.DataAnnotations;

namespace UniHub.BL.Entities
{
    public class Inscription
    {
        public int Id { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime InscriptionDate { get; set; } = DateTime.Now;

        public bool IsConfirmed { get; set; } = true;

        // Navigation properties
        public Activity? Activity { get; set; }

        public ApplicationUser? User { get; set; }
    }
}
