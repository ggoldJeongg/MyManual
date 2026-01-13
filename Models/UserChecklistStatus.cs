using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyManual.Models
{
    /// <summary>
    /// 사용자별 체크리스트 완료 상태
    /// </summary>
    public class UserChecklistStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ChecklistItemId { get; set; }

        public bool IsChecked { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("ChecklistItemId")]
        public virtual ChecklistItem? ChecklistItem { get; set; }
    }
}
