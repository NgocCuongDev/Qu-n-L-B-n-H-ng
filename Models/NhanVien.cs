using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    public class NhanVien : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string MaNhanVien { get; set; } = string.Empty; // Ví dụ: NV001

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string AppUserId { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        public string? HinhAnh { get; set; } // URL ảnh đại diện nhân viên

        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }


        public virtual ICollection<DonHang>? DonHangs { get; set; }
        public virtual ICollection<ThanhToan>? ThanhToans { get; set; }
    }
}
