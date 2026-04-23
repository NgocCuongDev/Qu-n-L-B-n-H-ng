using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    public class ChiTietDonHang : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DonHangId { get; set; }

        [Required]
        public int SanPhamId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien { get; set; } // SoLuong * DonGia

        // Navigation properties
        [ForeignKey("DonHangId")]
        public virtual DonHang? DonHang { get; set; }

        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }
    }
}
