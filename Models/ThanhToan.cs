using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    public class ThanhToan : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DonHangId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; }

        [Required]
        [StringLength(20)]
        public string PhuongThuc { get; set; } = string.Empty; // 'tien_mat', 'chuyen_khoan', 'the'

        [StringLength(100)]
        public string? MaGiaoDichThamChieu { get; set; } // Mã giao dịch nếu là chuyển khoản

        [Required]
        public int NhanVienThuId { get; set; }

        // Navigation properties
        [ForeignKey("DonHangId")]
        public virtual DonHang? DonHang { get; set; }

        [ForeignKey("NhanVienThuId")]
        public virtual NhanVien? NhanVienThu { get; set; }
    }
}
