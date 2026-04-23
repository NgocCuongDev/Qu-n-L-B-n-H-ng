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
    [RequirePermission("customers_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KhachHangController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/KhachHang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KhachHangDto>>> GetKhachHangs()
        {
            return await _context.KhachHangs
                .Select(k => new KhachHangDto
                {
                    Id = k.Id,
                    MaKhachHang = k.MaKhachHang,
                    HoTen = k.HoTen,
                    SoDienThoai = k.SoDienThoai,
                    Email = k.Email,
                    DiaChi = k.DiaChi,
                    LoaiKhach = k.LoaiKhach,
                    CongNo = k.CongNo,
                    AppUserId = k.AppUserId,
                    HinhAnh = k.HinhAnh,
                    NgayTao = k.NgayTao,
                    NgayCapNhat = k.NgayCapNhat
                })
                .ToListAsync();
        }

        // GET: api/KhachHang/5
        [HttpGet("{id}")]
        public async Task<ActionResult<KhachHangDto>> GetKhachHang(int id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);

            if (khachHang == null)
            {
                return NotFound();
            }

            return new KhachHangDto
            {
                Id = khachHang.Id,
                MaKhachHang = khachHang.MaKhachHang,
                HoTen = khachHang.HoTen,
                SoDienThoai = khachHang.SoDienThoai,
                Email = khachHang.Email,
                DiaChi = khachHang.DiaChi,
                LoaiKhach = khachHang.LoaiKhach,
                CongNo = khachHang.CongNo,
                AppUserId = khachHang.AppUserId,
                HinhAnh = khachHang.HinhAnh,
                NgayTao = khachHang.NgayTao,
                NgayCapNhat = khachHang.NgayCapNhat
            };
        }

        // POST: api/KhachHang
        // Note: For online registration, the AuthController should create KhachHang.
        // This endpoint is mostly for Walk-in customers created by Staff.
        [HttpPost]
        public async Task<ActionResult<KhachHangDto>> PostKhachHang(CreateKhachHangDto createDto)
        {
            if (await _context.KhachHangs.AnyAsync(k => k.MaKhachHang == createDto.MaKhachHang))
            {
                return BadRequest("Mã khách hàng đã tồn tại.");
            }

            if (await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == createDto.SoDienThoai))
            {
                return BadRequest("Số điện thoại đã được sử dụng.");
            }

            var khachHang = new KhachHang
            {
                MaKhachHang = createDto.MaKhachHang,
                HoTen = createDto.HoTen,
                SoDienThoai = createDto.SoDienThoai,
                Email = createDto.Email,
                DiaChi = createDto.DiaChi,
                LoaiKhach = createDto.LoaiKhach,
                HinhAnh = createDto.HinhAnh,
                CongNo = 0,
                AppUserId = createDto.AppUserId
            };

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetKhachHang), new { id = khachHang.Id }, new KhachHangDto
            {
                Id = khachHang.Id,
                MaKhachHang = khachHang.MaKhachHang,
                HoTen = khachHang.HoTen,
                SoDienThoai = khachHang.SoDienThoai,
                Email = khachHang.Email,
                DiaChi = khachHang.DiaChi,
                LoaiKhach = khachHang.LoaiKhach,
                CongNo = khachHang.CongNo,
                AppUserId = khachHang.AppUserId,
                HinhAnh = khachHang.HinhAnh,
                NgayTao = khachHang.NgayTao,
                NgayCapNhat = khachHang.NgayCapNhat
            });
        }

        // PUT: api/KhachHang/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKhachHang(int id, [FromBody] UpdateKhachHangDto updateDto)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            if (khachHang.SoDienThoai != updateDto.SoDienThoai && 
                await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == updateDto.SoDienThoai))
            {
                return BadRequest("Số điện thoại đã được sử dụng bởi khách hàng khác.");
            }

            khachHang.HoTen = updateDto.HoTen;
            khachHang.SoDienThoai = updateDto.SoDienThoai;
            khachHang.Email = updateDto.Email;
            khachHang.DiaChi = updateDto.DiaChi;
            khachHang.LoaiKhach = updateDto.LoaiKhach;

            khachHang.HinhAnh = updateDto.HinhAnh;
            khachHang.NgayCapNhat = DateTime.Now;

            try
            {
                _context.Entry(khachHang).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhachHangExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/KhachHang/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKhachHang(int id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            khachHang.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool KhachHangExists(int id)
        {
            return _context.KhachHangs.Any(e => e.Id == id);
        }
    }
}
