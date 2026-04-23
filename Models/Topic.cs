using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    [Table("Topics")]
    public class Topic : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? ParentId { get; set; }

        public int SortOrder { get; set; } = 0;

        public int Status { get; set; } = 1;

        // Navigation properties
        [ForeignKey("ParentId")]
        public virtual Topic? Parent { get; set; }
        public virtual ICollection<Topic> Children { get; set; } = new List<Topic>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
