using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class NhanVienDto
    {
        public int Id { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
        // Thống kê
        public int SoDonHang { get; set; }
        public int SoThanhToan { get; set; }
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("hinhAnh")]
        public string? HinhAnh { get; set; }
    }

    public class CreateNhanVienDto
    {
        [Required]
        [StringLength(20)]
        public string MaNhanVien { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [Required]
        [JsonPropertyName("appUserId")]
        public string AppUserId { get; set; } = string.Empty;

        [JsonPropertyName("hinhAnh")]
        public string? HinhAnh { get; set; }
    }

    public class UpdateNhanVienDto
    {
        [Required]
        [StringLength(100)]
        [JsonPropertyName("hoTen")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(15)]
        [JsonPropertyName("soDienThoai")]
        public string? SoDienThoai { get; set; }

        [JsonPropertyName("hinhAnh")]
        public string? HinhAnh { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }
}
