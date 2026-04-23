using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class CongNoDto
    {
        public int Id { get; set; }
        public int KhachHangId { get; set; }
        public string? TenKhachHang { get; set; }
        public int? DonHangId { get; set; }
        public string? MaDonHang { get; set; }
        public DateTime NgayPhatSinh { get; set; }
        public decimal SoTienNo { get; set; }
        public decimal SoTienTra { get; set; }
        public decimal SoDuCuoi { get; set; }
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateCongNoDto
    {
        [Required]
        public int KhachHangId { get; set; }

        public int? DonHangId { get; set; }

        public decimal SoTienNo { get; set; } = 0;

        public decimal SoTienTra { get; set; } = 0;

        [StringLength(500)]
        public string? GhiChu { get; set; }
    }

    public class UpdateCongNoDto
    {
        [StringLength(500)]
        public string? GhiChu { get; set; }
    }
}
