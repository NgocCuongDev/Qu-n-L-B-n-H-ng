using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class TonKhoDto
    {
        public int Id { get; set; }
        public int SanPhamId { get; set; }
        public string? TenSanPham { get; set; }
        public int KhoId { get; set; }
        public string? TenKho { get; set; }
        public int SoLuong { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateTonKhoDto
    {
        [Required]
        public int SanPhamId { get; set; }

        [Required]
        public int KhoId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int SoLuong { get; set; }
    }

    public class UpdateTonKhoDto
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int SoLuong { get; set; }
    }
}
