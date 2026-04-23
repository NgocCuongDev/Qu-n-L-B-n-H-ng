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
    [RequirePermission("categories_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DanhMucController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DanhMuc
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DanhMucDto>>> GetDanhMucs()
        {
            return await _context.DanhMucs
                .Select(d => new DanhMucDto
                {
                    Id = d.Id,
                    TenDanhMuc = d.TenDanhMuc,
                    GhiChu = d.GhiChu,
                    NgayTao = d.NgayTao,
                    NgayCapNhat = d.NgayCapNhat
                })
                .ToListAsync();
        }

        // GET: api/DanhMuc/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<DanhMucDto>> GetDanhMuc(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);

            if (danhMuc == null)
            {
                return NotFound();
            }

            return new DanhMucDto
            {
                Id = danhMuc.Id,
                TenDanhMuc = danhMuc.TenDanhMuc,
                GhiChu = danhMuc.GhiChu,
                NgayTao = danhMuc.NgayTao,
                NgayCapNhat = danhMuc.NgayCapNhat
            };
        }

        // POST: api/DanhMuc
        [HttpPost]
        public async Task<ActionResult<DanhMucDto>> PostDanhMuc(CreateDanhMucDto createDto)
        {
            var danhMuc = new DanhMuc
            {
                TenDanhMuc = createDto.TenDanhMuc,
                GhiChu = createDto.GhiChu
            };

            _context.DanhMucs.Add(danhMuc);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDanhMuc), new { id = danhMuc.Id }, new DanhMucDto
            {
                Id = danhMuc.Id,
                TenDanhMuc = danhMuc.TenDanhMuc,
                GhiChu = danhMuc.GhiChu,
                NgayTao = danhMuc.NgayTao,
                NgayCapNhat = danhMuc.NgayCapNhat
            });
        }

        // PUT: api/DanhMuc/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDanhMuc(int id, UpdateDanhMucDto updateDto)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
            {
                return NotFound();
            }

            danhMuc.TenDanhMuc = updateDto.TenDanhMuc;
            danhMuc.GhiChu = updateDto.GhiChu;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DanhMucExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/DanhMuc/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDanhMuc(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
            {
                return NotFound();
            }

            danhMuc.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DanhMucExists(int id)
        {
            return _context.DanhMucs.Any(e => e.Id == id);
        }
    }
}
