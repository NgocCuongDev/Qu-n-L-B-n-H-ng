using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    public class CongNo : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KhachHangId { get; set; }

        public int? DonHangId { get; set; } // Có thể NULL nếu là trả nợ không qua đơn hàng mới

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime NgayPhatSinh { get; set; } = DateTime.Now;

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienNo { get; set; } = 0; // Tiền phát sinh thêm (từ đơn hàng mới)

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienTra { get; set; } = 0; // Tiền khách trả

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoDuCuoi { get; set; } // Số dư công nợ sau phát sinh này

        [StringLength(500)]
        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("KhachHangId")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("DonHangId")]
        public virtual DonHang? DonHang { get; set; }
    }
}
