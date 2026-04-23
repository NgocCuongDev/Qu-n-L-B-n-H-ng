using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class KhachHangDto
    {
        public int Id { get; set; }
        public string MaKhachHang { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public string LoaiKhach { get; set; } = "ban_le";
        public decimal CongNo { get; set; }
        public string? AppUserId { get; set; }
        [JsonPropertyName("hinhAnh")]
        public string? HinhAnh { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateKhachHangDto
    {
        [Required]
        [StringLength(50)]
        public string MaKhachHang { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        public string LoaiKhach { get; set; } = "ban_le";

        [JsonPropertyName("hinhAnh")]
        public string? HinhAnh { get; set; }

        public string? AppUserId { get; set; }
    }

    public class UpdateKhachHangDto
    {
        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        public string LoaiKhach { get; set; } = "ban_le";

        [JsonPropertyName("hinhAnh")]
        public string? HinhAnh { get; set; }
    }
}
