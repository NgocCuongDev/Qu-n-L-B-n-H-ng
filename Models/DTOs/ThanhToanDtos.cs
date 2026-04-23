using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class ThanhToanDto
    {
        public int Id { get; set; }
        public int DonHangId { get; set; }
        public string? MaDonHang { get; set; }
        public DateTime NgayThanhToan { get; set; }
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;
        public string? MaGiaoDichThamChieu { get; set; }
        public int NhanVienThuId { get; set; }
        public string? TenNhanVienThu { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateThanhToanDto
    {
        [Required]
        public int DonHangId { get; set; }

        [Required]
        public decimal SoTien { get; set; }

        [Required]
        [StringLength(20)]
        public string PhuongThuc { get; set; } = string.Empty; // 'tien_mat', 'chuyen_khoan', 'the'

        [StringLength(100)]
        public string? MaGiaoDichThamChieu { get; set; }

        [Required]
        public int NhanVienThuId { get; set; }
    }

    public class UpdateThanhToanDto
    {
        [Required]
        [StringLength(20)]
        public string PhuongThuc { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MaGiaoDichThamChieu { get; set; }
    }
}
