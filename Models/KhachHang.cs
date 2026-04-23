using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    public class KhachHang : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MaKhachHang { get; set; } = string.Empty; // Ví dụ: KH00001

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        public string? HinhAnh { get; set; } // URL ảnh đại diện khách hàng

        public string LoaiKhach { get; set; } = "ban_le"; // 'ban_le' hoặc 'khach_si'

        [DataType(DataType.Currency)]
        public decimal CongNo { get; set; } = 0;

        public string? AppUserId { get; set; } // Nullable because walk-in customers might not have an online account

        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }

        public virtual ICollection<DonHang>? DonHangs { get; set; }
        public virtual ICollection<CongNo>? CongNos { get; set; }
    }
}
