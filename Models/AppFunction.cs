using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models
{
    public class AppFunction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        // Navigation property
        public virtual ICollection<UserPermission>? UserPermissions { get; set; }
    }
}
