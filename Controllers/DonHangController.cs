using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHeThongBanHang.Data;
using QuanLyHeThongBanHang.Models;
using QuanLyHeThongBanHang.Models.DTOs;

using QuanLyHeThongBanHang.Filters;

namespace QuanLyHeThongBanHang.Controllers
{
    [Authorize]
    [RequirePermission("orders_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DonHangController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DonHang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonHangDto>>> GetDonHangs()
        {
            return await _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .Select(d => new DonHangDto
                {
                    Id = d.Id,
                    MaDonHang = d.MaDonHang,
                    KhachHangId = d.KhachHangId,
                    TenKhachHang = d.KhachHang != null ? d.KhachHang.HoTen : null,
                    NhanVienId = d.NhanVienId,
                    TenNhanVien = d.NhanVien != null ? d.NhanVien.HoTen : null,
                    TongTienHang = d.TongTienHang,
                    ChietKhau = d.ChietKhau,
                    ThueVat = d.ThueVat,
                    TongThanhToan = d.TongThanhToan,
                    TrangThai = d.TrangThai,
                    SoDienThoai = d.KhachHang != null ? d.KhachHang.SoDienThoai : null,
                    DiaChi = d.KhachHang != null ? d.KhachHang.DiaChi : null,
                    PhuongThucThanhToan = d.ThanhToans != null && d.ThanhToans.Any() ? d.ThanhToans.First().PhuongThuc : "Chưa xác định",
                    NgayTao = d.NgayTao,
                    NgayCapNhat = d.NgayCapNhat
                })
                .ToListAsync();
        }

        // GET: api/DonHang/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DonHangDto>> GetDonHang(int id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .Include(d => d.ChiTietDonHangs!)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donHang == null)
            {
                return NotFound();
            }

            var dto = new DonHangDto
            {
                Id = donHang.Id,
                MaDonHang = donHang.MaDonHang,
                KhachHangId = donHang.KhachHangId,
                TenKhachHang = donHang.KhachHang != null ? donHang.KhachHang.HoTen : null,
                NhanVienId = donHang.NhanVienId,
                TenNhanVien = donHang.NhanVien != null ? donHang.NhanVien.HoTen : null,
                TongTienHang = donHang.TongTienHang,
                ChietKhau = donHang.ChietKhau,
                ThueVat = donHang.ThueVat,
                TongThanhToan = donHang.TongThanhToan,
                TrangThai = donHang.TrangThai,
                SoDienThoai = donHang.KhachHang != null ? donHang.KhachHang.SoDienThoai : null,
                DiaChi = donHang.KhachHang != null ? donHang.KhachHang.DiaChi : null,
                PhuongThucThanhToan = donHang.ThanhToans != null && donHang.ThanhToans.Any() ? donHang.ThanhToans.First().PhuongThuc : "Chưa xác định",
                NgayTao = donHang.NgayTao,
                NgayCapNhat = donHang.NgayCapNhat,
                ChiTietDonHangs = donHang.ChiTietDonHangs?.Select(ct => new ChiTietDonHangDto
                {
                    Id = ct.Id,
                    SanPhamId = ct.SanPhamId,
                    TenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : null,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien
                }).ToList() ?? new List<ChiTietDonHangDto>()
            };

            return dto;
        }

        // POST: api/DonHang
        [HttpPost]
        public async Task<ActionResult<DonHangDto>> PostDonHang(CreateDonHangDto createDto)
        {
            if (createDto.ChiTietDonHangs == null || !createDto.ChiTietDonHangs.Any())
            {
                return BadRequest("Đơn hàng phải có ít nhất 1 sản phẩm.");
            }

            if (!await _context.KhachHangs.AnyAsync(k => k.Id == createDto.KhachHangId))
            {
                return BadRequest("Khách hàng không tồn tại.");
            }

            if (!await _context.NhanViens.AnyAsync(n => n.Id == createDto.NhanVienId))
            {
                return BadRequest("Nhân viên không tồn tại.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo Đơn Hàng mới
                var donHang = new DonHang
                {
                    MaDonHang = GenerateMaDonHang(),
                    KhachHangId = createDto.KhachHangId,
                    NhanVienId = createDto.NhanVienId,
                    ChietKhau = createDto.ChietKhau,
                    ThueVat = createDto.ThueVat,
                    TrangThai = "cho_thanh_toan",
                    ChiTietDonHangs = new List<ChiTietDonHang>()
                };

                decimal tongTienHang = 0;

                // 2. Tạo Chi Tiết Đơn Hàng và tính giá
                foreach (var itemDto in createDto.ChiTietDonHangs)
                {
                    var sanPham = await _context.SanPhams.FindAsync(itemDto.SanPhamId);
                    if (sanPham == null)
                    {
                        return BadRequest($"Sản phẩm với ID {itemDto.SanPhamId} không tồn tại.");
                    }

                    // Kiểm tra tồn kho (đơn giản, bỏ qua nếu bán âm cho phép, hoặc thêm logic kiểm tra ở đây)
                    if (sanPham.SoLuongTon < itemDto.SoLuong)
                    {
                        return BadRequest($"Sản phẩm {sanPham.TenSanPham} không đủ số lượng tồn kho.");
                    }

                    var thanhTien = sanPham.GiaBan * itemDto.SoLuong;
                    tongTienHang += thanhTien;

                    // Trừ tồn kho từ các kho (TonKho)
                    var quantitiesToSubtract = itemDto.SoLuong;
                    var tableTonKhos = await _context.TonKhos
                        .Where(t => t.SanPhamId == itemDto.SanPhamId && t.SoLuong > 0)
                        .OrderByDescending(t => t.SoLuong) // Ưu tiên trừ kho nhiều hàng trước
                        .ToListAsync();

                    if (tableTonKhos.Sum(t => t.SoLuong) < quantitiesToSubtract)
                    {
                        return BadRequest($"Sản phẩm {sanPham.TenSanPham} không đủ tổng số lượng trong các kho.");
                    }

                    foreach (var tk in tableTonKhos)
                    {
                        if (quantitiesToSubtract <= 0) break;

                        if (tk.SoLuong >= quantitiesToSubtract)
                        {
                            tk.SoLuong -= quantitiesToSubtract;
                            quantitiesToSubtract = 0;
                        }
                        else
                        {
                            quantitiesToSubtract -= tk.SoLuong;
                            tk.SoLuong = 0;
                        }
                    }

                    // Cập nhật lại tổng tồn kho ở bảng SanPham để đồng bộ
                    sanPham.SoLuongTon -= itemDto.SoLuong;

                    donHang.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        SanPhamId = itemDto.SanPhamId,
                        SoLuong = itemDto.SoLuong,
                        DonGia = sanPham.GiaBan,
                        ThanhTien = thanhTien
                    });
                }

                donHang.TongTienHang = tongTienHang;
                donHang.TongThanhToan = tongTienHang - createDto.ChietKhau + createDto.ThueVat;

                // 3. Lưu vào database
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetDonHang), new { id = donHang.Id }, await GetDonHang(donHang.Id));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Đã xảy ra lỗi khi tạo đơn hàng: " + ex.Message);
            }
        }

        // PUT: api/DonHang/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateDonHangStatusDto updateDto)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }

            // TODO: Bổ sung logic hoàn trả số lượng kho nếu trạng thái chuyển sang 'da_huy'
            
            donHang.TrangThai = updateDto.TrangThai;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/DonHang/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonHang(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }

            // Xóa mềm đơn hàng (không xóa thật, nhưng ẩn khỏi query thường)
            // Cần implement lại logic trả lại tồn kho nếu xóa đơn hàng
            donHang.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GenerateMaDonHang()
        {
            // Simple logic for example. YYYYMMDD + Random
            return $"HD{DateTime.Now.ToString("yyyyMMdd")}{new Random().Next(1000, 9999)}";
        }
    }
}
