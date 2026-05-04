using System.ComponentModel.DataAnnotations;

namespace UniHub.BL.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        // Foreign keys
        [Required]
        public int ActivityId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public Activity? Activity { get; set; }

        public ApplicationUser? User { get; set; }
    }
}
