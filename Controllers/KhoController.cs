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
    [RequirePermission("warehouses_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class KhoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KhoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Kho
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KhoDto>>> GetKhos()
        {
            return await _context.Khos
                .Select(k => new KhoDto
                {
                    Id = k.Id,
                    TenKho = k.TenKho,
                    DiaChi = k.DiaChi,
                    NgayTao = k.NgayTao,
                    NgayCapNhat = k.NgayCapNhat
                })
                .ToListAsync();
        }

        // GET: api/Kho/5
        [HttpGet("{id}")]
        public async Task<ActionResult<KhoDto>> GetKho(int id)
        {
            var kho = await _context.Khos.FindAsync(id);

            if (kho == null)
            {
                return NotFound();
            }

            return new KhoDto
            {
                Id = kho.Id,
                TenKho = kho.TenKho,
                DiaChi = kho.DiaChi,
                NgayTao = kho.NgayTao,
                NgayCapNhat = kho.NgayCapNhat
            };
        }

        // POST: api/Kho
        [HttpPost]
        public async Task<ActionResult<KhoDto>> PostKho(CreateKhoDto createDto)
        {
            var kho = new Kho
            {
                TenKho = createDto.TenKho,
                DiaChi = createDto.DiaChi
            };

            _context.Khos.Add(kho);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetKho), new { id = kho.Id }, new KhoDto
            {
                Id = kho.Id,
                TenKho = kho.TenKho,
                DiaChi = kho.DiaChi,
                NgayTao = kho.NgayTao,
                NgayCapNhat = kho.NgayCapNhat
            });
        }

        // PUT: api/Kho/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKho(int id, UpdateKhoDto updateDto)
        {
            var kho = await _context.Khos.FindAsync(id);
            if (kho == null)
            {
                return NotFound();
            }

            kho.TenKho = updateDto.TenKho;
            kho.DiaChi = updateDto.DiaChi;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhoExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Kho/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKho(int id)
        {
            var kho = await _context.Khos.FindAsync(id);
            if (kho == null)
            {
                return NotFound();
            }

            kho.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool KhoExists(int id)
        {
            return _context.Khos.Any(e => e.Id == id);
        }
    }
}
