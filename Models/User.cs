using System.ComponentModel.DataAnnotations;

namespace MyManual.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime JoinDate { get; set; }

        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// SHA256 해시된 비밀번호
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<UserChecklistStatus> ChecklistStatuses { get; set; } = new List<UserChecklistStatus>();
        public virtual ICollection<UserTaskStatus> TaskStatuses { get; set; } = new List<UserTaskStatus>();
    }
}