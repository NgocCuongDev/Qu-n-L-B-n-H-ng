using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class ChiTietDonHangDto
    {
        public int Id { get; set; }
        public int SanPhamId { get; set; }
        public string? TenSanPham { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class DonHangDto
    {
        public int Id { get; set; }
        public string MaDonHang { get; set; } = string.Empty;
        public int KhachHangId { get; set; }
        public string? TenKhachHang { get; set; }
        public int NhanVienId { get; set; }
        public string? TenNhanVien { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal ChietKhau { get; set; }
        public decimal ThueVat { get; set; }
        public decimal TongThanhToan { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }

        public List<ChiTietDonHangDto> ChiTietDonHangs { get; set; } = new List<ChiTietDonHangDto>();
    }

    public class CreateChiTietDonHangDto
    {
        [Required]
        public int SanPhamId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }
    }

    public class CreateDonHangDto
    {
        [Required]
        public int KhachHangId { get; set; }

        [Required]
        public int NhanVienId { get; set; }

        public decimal ChietKhau { get; set; } = 0;

        public decimal ThueVat { get; set; } = 0;

        [Required]
        public List<CreateChiTietDonHangDto> ChiTietDonHangs { get; set; } = new List<CreateChiTietDonHangDto>();
    }

    public class UpdateDonHangStatusDto
    {
        [Required]
        [StringLength(20)]
        public string TrangThai { get; set; } = string.Empty; // 'cho_thanh_toan', 'da_thanh_toan', 'da_huy', etc.
    }
}
