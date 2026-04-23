using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class SanPhamDto
    {
        public int Id { get; set; }
        public string MaSanPham { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public int? DanhMucId { get; set; }
        public string? TenDanhMuc { get; set; }
        public string DonViTinh { get; set; } = string.Empty;
        public decimal GiaNhap { get; set; }
        public decimal GiaBan { get; set; }
        public decimal? GiaGiam { get; set; }
        public int SoLuongTon { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateSanPhamDto
    {
        [Required]
        [StringLength(50)]
        public string MaSanPham { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string TenSanPham { get; set; } = string.Empty;

        public int? DanhMucId { get; set; }

        [Required]
        [StringLength(20)]
        public string DonViTinh { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal GiaNhap { get; set; }

        [Range(0, double.MaxValue)]
        public decimal GiaBan { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? GiaGiam { get; set; }

        public string? HinhAnh { get; set; }
    }

    public class UpdateSanPhamDto
    {
        [Required]
        [StringLength(200)]
        public string TenSanPham { get; set; } = string.Empty;

        public int? DanhMucId { get; set; }

        [Required]
        [StringLength(20)]
        public string DonViTinh { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal GiaNhap { get; set; }

        [Range(0, double.MaxValue)]
        public decimal GiaBan { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? GiaGiam { get; set; }

        public string? HinhAnh { get; set; }
    }
}
