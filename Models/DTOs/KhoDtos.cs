using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class KhoDto
    {
        public int Id { get; set; }
        public string TenKho { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateKhoDto
    {
        [Required]
        [StringLength(100)]
        public string TenKho { get; set; } = string.Empty;

        [StringLength(255)]
        public string? DiaChi { get; set; }
    }

    public class UpdateKhoDto
    {
        [Required]
        [StringLength(100)]
        public string TenKho { get; set; } = string.Empty;

        [StringLength(255)]
        public string? DiaChi { get; set; }
    }
}
