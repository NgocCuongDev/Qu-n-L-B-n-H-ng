using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    [Table("Posts")]
    public class Post : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Image { get; set; }

        public int TopicId { get; set; }

        public int Status { get; set; } = 1;

        public bool IsFeatured { get; set; } = false;

        // Navigation property
        [ForeignKey("TopicId")]
        public virtual Topic? Topic { get; set; }
    }
}
