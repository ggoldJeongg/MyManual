using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyManual.Models
{
    public class ManualImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ManualId { get; set; }

        [Required]
        [MaxLength(500)]
        public string BlobUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FileName { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("ManualId")]
        public virtual Manual? Manual { get; set; }
    }
}
