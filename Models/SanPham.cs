using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    public class SanPham : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MaSanPham { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string TenSanPham { get; set; } = string.Empty;

        public int? DanhMucId { get; set; } // Foreign Key

        [Required]
        [StringLength(20)]
        public string DonViTinh { get; set; } = string.Empty; // Ví dụ: "Cái", "Hộp"

        [DataType(DataType.Currency)]
        public decimal GiaNhap { get; set; }

        [DataType(DataType.Currency)]
        public decimal GiaBan { get; set; }
        
        [DataType(DataType.Currency)]
        public decimal? GiaGiam { get; set; } // Giá khuyến mãi (nếu có)

        public int SoLuongTon { get; set; } = 0;

        public string? HinhAnh { get; set; }
 
        // Navigation properties
        [ForeignKey("DanhMucId")]
        public virtual DanhMuc? DanhMuc { get; set; }

        public virtual ICollection<ChiTietDonHang>? ChiTietDonHangs { get; set; }
        public virtual ICollection<TonKho>? TonKhos { get; set; }
    }
}
