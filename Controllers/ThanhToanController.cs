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
    [RequirePermission("payments_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ThanhToanController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ThanhToan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ThanhToanDto>>> GetThanhToans()
        {
            return await _context.ThanhToans
                .Include(t => t.DonHang)
                .Include(t => t.NhanVienThu)
                .Select(t => new ThanhToanDto
                {
                    Id = t.Id,
                    DonHangId = t.DonHangId,
                    MaDonHang = t.DonHang != null ? t.DonHang.MaDonHang : null,
                    NgayThanhToan = t.NgayThanhToan,
                    SoTien = t.SoTien,
                    PhuongThuc = t.PhuongThuc,
                    MaGiaoDichThamChieu = t.MaGiaoDichThamChieu,
                    NhanVienThuId = t.NhanVienThuId,
                    TenNhanVienThu = t.NhanVienThu != null ? t.NhanVienThu.HoTen : null,
                    NgayTao = t.NgayTao,
                    NgayCapNhat = t.NgayCapNhat
                })
                .ToListAsync();
        }

        // GET: api/ThanhToan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ThanhToanDto>> GetThanhToan(int id)
        {
            var thanhToan = await _context.ThanhToans
                .Include(t => t.DonHang)
                .Include(t => t.NhanVienThu)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (thanhToan == null)
            {
                return NotFound();
            }

            return new ThanhToanDto
            {
                Id = thanhToan.Id,
                DonHangId = thanhToan.DonHangId,
                MaDonHang = thanhToan.DonHang != null ? thanhToan.DonHang.MaDonHang : null,
                NgayThanhToan = thanhToan.NgayThanhToan,
                SoTien = thanhToan.SoTien,
                PhuongThuc = thanhToan.PhuongThuc,
                MaGiaoDichThamChieu = thanhToan.MaGiaoDichThamChieu,
                NhanVienThuId = thanhToan.NhanVienThuId,
                TenNhanVienThu = thanhToan.NhanVienThu != null ? thanhToan.NhanVienThu.HoTen : null,
                NgayTao = thanhToan.NgayTao,
                NgayCapNhat = thanhToan.NgayCapNhat
            };
        }

        // POST: api/ThanhToan
        [HttpPost]
        public async Task<ActionResult<ThanhToanDto>> PostThanhToan(CreateThanhToanDto createDto)
        {
            // Validate foreign keys
            var donHang = await _context.DonHangs.FindAsync(createDto.DonHangId);
            if (donHang == null)
            {
                return BadRequest("Đơn hàng không tồn tại.");
            }

            var nhanVien = await _context.NhanViens.FindAsync(createDto.NhanVienThuId);
            if (nhanVien == null)
            {
                return BadRequest("Nhân viên thu tiền không tồn tại.");
            }

            var thanhToan = new ThanhToan
            {
                DonHangId = createDto.DonHangId,
                SoTien = createDto.SoTien,
                PhuongThuc = createDto.PhuongThuc,
                MaGiaoDichThamChieu = createDto.MaGiaoDichThamChieu,
                NhanVienThuId = createDto.NhanVienThuId,
                NgayThanhToan = DateTime.Now
            };

            // Optional: If total payments equal order totally paid, update order status automatically
            var totalPaidSoFar = await _context.ThanhToans
                .Where(t => t.DonHangId == createDto.DonHangId && !t.IsDeleted)
                .SumAsync(t => t.SoTien);

            if (totalPaidSoFar + createDto.SoTien >= donHang.TongThanhToan)
            {
                donHang.TrangThai = "da_thanh_toan";
            }

            _context.ThanhToans.Add(thanhToan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetThanhToan), new { id = thanhToan.Id }, new ThanhToanDto
            {
                Id = thanhToan.Id,
                DonHangId = thanhToan.DonHangId,
                MaDonHang = donHang.MaDonHang,
                NgayThanhToan = thanhToan.NgayThanhToan,
                SoTien = thanhToan.SoTien,
                PhuongThuc = thanhToan.PhuongThuc,
                MaGiaoDichThamChieu = thanhToan.MaGiaoDichThamChieu,
                NhanVienThuId = thanhToan.NhanVienThuId,
                TenNhanVienThu = nhanVien.HoTen,
                NgayTao = thanhToan.NgayTao,
                NgayCapNhat = thanhToan.NgayCapNhat
            });
        }

        // PUT: api/ThanhToan/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutThanhToan(int id, UpdateThanhToanDto updateDto)
        {
            var thanhToan = await _context.ThanhToans.FindAsync(id);
            if (thanhToan == null)
            {
                return NotFound();
            }

            thanhToan.PhuongThuc = updateDto.PhuongThuc;
            thanhToan.MaGiaoDichThamChieu = updateDto.MaGiaoDichThamChieu;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ThanhToanExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/ThanhToan/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteThanhToan(int id)
        {
            var thanhToan = await _context.ThanhToans.FindAsync(id);
            if (thanhToan == null)
            {
                return NotFound();
            }

            thanhToan.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ThanhToanExists(int id)
        {
            return _context.ThanhToans.Any(e => e.Id == id);
        }
    }
}
