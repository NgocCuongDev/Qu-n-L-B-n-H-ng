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
    [RequirePermission("inventory_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class TonKhoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TonKhoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TonKho
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TonKhoDto>>> GetTonKhos()
        {
            return await _context.TonKhos
                .Include(t => t.SanPham)
                .Include(t => t.Kho)
                .Select(t => new TonKhoDto
                {
                    Id = t.Id,
                    SanPhamId = t.SanPhamId,
                    TenSanPham = t.SanPham != null ? t.SanPham.TenSanPham : null,
                    KhoId = t.KhoId,
                    TenKho = t.Kho != null ? t.Kho.TenKho : null,
                    SoLuong = t.SoLuong,
                    NgayTao = t.NgayTao,
                    NgayCapNhat = t.NgayCapNhat
                })
                .ToListAsync();
        }

        // GET: api/TonKho/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TonKhoDto>> GetTonKho(int id)
        {
            var tonKho = await _context.TonKhos
                .Include(t => t.SanPham)
                .Include(t => t.Kho)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tonKho == null)
            {
                return NotFound();
            }

            return new TonKhoDto
            {
                Id = tonKho.Id,
                SanPhamId = tonKho.SanPhamId,
                TenSanPham = tonKho.SanPham != null ? tonKho.SanPham.TenSanPham : null,
                KhoId = tonKho.KhoId,
                TenKho = tonKho.Kho != null ? tonKho.Kho.TenKho : null,
                SoLuong = tonKho.SoLuong,
                NgayTao = tonKho.NgayTao,
                NgayCapNhat = tonKho.NgayCapNhat
            };
        }

        // POST: api/TonKho
        [HttpPost]
        public async Task<ActionResult<TonKhoDto>> PostTonKho(CreateTonKhoDto createDto)
        {
            // Validate SanPham and Kho exist
            if (!await _context.SanPhams.AnyAsync(s => s.Id == createDto.SanPhamId))
            {
                return BadRequest("Sản phẩm không tồn tại.");
            }

            if (!await _context.Khos.AnyAsync(k => k.Id == createDto.KhoId))
            {
                return BadRequest("Kho không tồn tại.");
            }

            // Check if TonKho for this product and this warehouse already exists
            var existingTonKho = await _context.TonKhos
                .FirstOrDefaultAsync(t => t.SanPhamId == createDto.SanPhamId && t.KhoId == createDto.KhoId);

            if (existingTonKho != null)
            {
                return BadRequest("Tồn kho cho sản phẩm này tại kho này đã tồn tại. Vui lòng sử dụng chức năng cập nhật.");
            }

            var tonKho = new TonKho
            {
                SanPhamId = createDto.SanPhamId,
                KhoId = createDto.KhoId,
                SoLuong = createDto.SoLuong
            };

            _context.TonKhos.Add(tonKho);
            await _context.SaveChangesAsync();

            // Cập nhật tổng tồn kho cho sản phẩm
            await UpdateSanPhamTotalStock(tonKho.SanPhamId);

            // Fetch relations for return DTO
            var sanPham = await _context.SanPhams.FindAsync(tonKho.SanPhamId);
            var kho = await _context.Khos.FindAsync(tonKho.KhoId);

            return CreatedAtAction(nameof(GetTonKho), new { id = tonKho.Id }, new TonKhoDto
            {
                Id = tonKho.Id,
                SanPhamId = tonKho.SanPhamId,
                TenSanPham = sanPham?.TenSanPham,
                KhoId = tonKho.KhoId,
                TenKho = kho?.TenKho,
                SoLuong = tonKho.SoLuong,
                NgayTao = tonKho.NgayTao,
                NgayCapNhat = tonKho.NgayCapNhat
            });
        }

        // PUT: api/TonKho/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTonKho(int id, UpdateTonKhoDto updateDto)
        {
            var tonKho = await _context.TonKhos.FindAsync(id);
            if (tonKho == null)
            {
                return NotFound();
            }

            tonKho.SoLuong = updateDto.SoLuong;

            try
            {
                await _context.SaveChangesAsync();
                // Cập nhật tổng tồn kho cho sản phẩm
                await UpdateSanPhamTotalStock(tonKho.SanPhamId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TonKhoExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/TonKho/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTonKho(int id)
        {
            var tonKho = await _context.TonKhos.FindAsync(id);
            if (tonKho == null)
            {
                return NotFound();
            }

            var sanPhamId = tonKho.SanPhamId;
            _context.TonKhos.Remove(tonKho);
            await _context.SaveChangesAsync();

            // Cập nhật tổng tồn kho cho sản phẩm
            await UpdateSanPhamTotalStock(sanPhamId);

            return NoContent();
        }

        private bool TonKhoExists(int id)
        {
            return _context.TonKhos.Any(e => e.Id == id);
        }

        private async Task UpdateSanPhamTotalStock(int sanPhamId)
        {
            var sanPham = await _context.SanPhams.FindAsync(sanPhamId);
            if (sanPham != null)
            {
                sanPham.SoLuongTon = await _context.TonKhos
                    .Where(t => t.SanPhamId == sanPhamId)
                    .SumAsync(t => t.SoLuong);
                await _context.SaveChangesAsync();
            }
        }
    }
}
