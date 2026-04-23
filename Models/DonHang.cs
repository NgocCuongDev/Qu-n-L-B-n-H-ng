using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models
{
    public class DonHang : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MaDonHang { get; set; } = string.Empty; // Ví dụ: HD202403270001

        public int KhachHangId { get; set; } // Foreign Key
        public int NhanVienId { get; set; }   // Foreign Key

        [DataType(DataType.Currency)]
        public decimal TongTienHang { get; set; }

        [DataType(DataType.Currency)]
        public decimal ChietKhau { get; set; } = 0;

        [DataType(DataType.Currency)]
        public decimal ThueVat { get; set; } = 0;

        [DataType(DataType.Currency)]
        public decimal TongThanhToan { get; set; }

        [Required]
        [StringLength(20)]
        public string TrangThai { get; set; } = "cho_thanh_toan"; // 'cho_thanh_toan', 'da_thanh_toan', 'da_huy'

        // --- Navigation Properties (Quan hệ) ---
        public virtual KhachHang? KhachHang { get; set; }
        public virtual NhanVien? NhanVien { get; set; }
        public virtual ICollection<ChiTietDonHang>? ChiTietDonHangs { get; set; }
        public virtual ICollection<ThanhToan>? ThanhToans { get; set; }
        public virtual ICollection<CongNo>? CongNos { get; set; }
    }
}
