using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    [Table("Menus")]
    public class Menu : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Link { get; set; }

        [StringLength(50)]
        public string? Type { get; set; } // category, post, page, custom

        public int? ParentId { get; set; }

        [StringLength(50)]
        public string? Position { get; set; } // main, footer, top-bar

        public int SortOrder { get; set; } = 0;

        public int Status { get; set; } = 1; // 1: Active, 0: Inactive

        // Navigation property for hierarchy
        [ForeignKey("ParentId")]
        public virtual Menu? Parent { get; set; }
        public virtual ICollection<Menu> Children { get; set; } = new List<Menu>();
    }
}
