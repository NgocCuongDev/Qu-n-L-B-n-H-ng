using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class DanhMucDto
    {
        public int Id { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateDanhMucDto
    {
        [Required]
        [StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class UpdateDanhMucDto
    {
        [Required]
        [StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }
}
