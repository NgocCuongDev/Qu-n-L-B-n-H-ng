using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models
{
    public class DanhMuc : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;

        [StringLength(500)]
        public string? GhiChu { get; set; }

        // Navigation property
        public virtual ICollection<SanPham>? SanPhams { get; set; }
    }
}
