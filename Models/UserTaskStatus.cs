using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyManual.Models
{
    /// <summary>
    /// 사용자별 온보딩 태스크 완료 상태
    /// </summary>
    public class UserTaskStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int OnboardingTaskId { get; set; }

        public bool IsCompleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("OnboardingTaskId")]
        public virtual OnboardingTask? OnboardingTask { get; set; }
    }
}
