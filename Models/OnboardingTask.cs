using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyManual.Models
{
    public class OnboardingTask
    {
        [Key]
        public int Id { get; set; }

        public int Day { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public int ManualId { get; set; }

        // Navigation properties
        [ForeignKey("ManualId")]
        public virtual Manual? Manual { get; set; }

        public virtual ICollection<UserTaskStatus> UserStatuses { get; set; } = new List<UserTaskStatus>();
    }
}
