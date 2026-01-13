using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyManual.Models
{
    public class Manual
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        public string Process { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<ChecklistItem> Checklist { get; set; } = new List<ChecklistItem>();
        public virtual ICollection<HistoryItem> History { get; set; } = new List<HistoryItem>();
        public virtual ICollection<OnboardingTask> OnboardingTasks { get; set; } = new List<OnboardingTask>();
    }

    public class ChecklistItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ManualId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        // Navigation properties
        [ForeignKey("ManualId")]
        public virtual Manual? Manual { get; set; }

        public virtual ICollection<UserChecklistStatus> UserStatuses { get; set; } = new List<UserChecklistStatus>();
    }

    public class HistoryItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ManualId { get; set; }

        [Required]
        public string Date { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("ManualId")]
        public virtual Manual? Manual { get; set; }
    }
}
