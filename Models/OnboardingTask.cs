using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyManual.Models
{
    /// <summary>
    /// 사용자별 온보딩 태스크 (매뉴얼 할당)
    /// </summary>
    public class OnboardingTask
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 할당된 사용자 ID (null이면 템플릿)
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 할당된 Day (1, 2, 3, ...)
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 태스크 제목 (매뉴얼 제목에서 복사)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 연결된 매뉴얼 ID
        /// </summary>
        public int ManualId { get; set; }

        /// <summary>
        /// 표시 순서
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 생성 일시
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("ManualId")]
        public virtual Manual? Manual { get; set; }

        public virtual ICollection<UserTaskStatus> UserStatuses { get; set; } = new List<UserTaskStatus>();
    }
}
