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
    [RequirePermission("banners_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BannerController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Banner

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BannerDto>>> GetBanners()
        {
            return await _context.Banners
                .Select(b => new BannerDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Image = b.Image,
                    Link = b.Link,
                    Position = b.Position,
                    Status = b.Status,
                    SortOrder = b.SortOrder,
                    NgayTao = b.NgayTao,
                    NgayCapNhat = b.NgayCapNhat
                })
                .ToListAsync();
        }

        // GET: api/Banner/public (Cho Storefront)
        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<BannerDto>>> GetPublicBanners()
        {
            return await _context.Banners
                .Where(b => b.Status == 1 && !b.IsDeleted)
                .OrderBy(b => b.SortOrder)
                .Select(b => new BannerDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Image = b.Image,
                    Link = b.Link,
                    Position = b.Position,
                    Status = b.Status,
                    SortOrder = b.SortOrder,
                    NgayTao = b.NgayTao,
                    NgayCapNhat = b.NgayCapNhat
                })
                .ToListAsync();
        }

        // GET: api/Banner/5

        [HttpGet("{id}")]
        public async Task<ActionResult<BannerDto>> GetBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
            {
                return NotFound();
            }

            return new BannerDto
            {
                Id = banner.Id,
                Name = banner.Name,
                Description = banner.Description,
                Image = banner.Image,
                Link = banner.Link,
                Position = banner.Position,
                Status = banner.Status,
                SortOrder = banner.SortOrder,
                NgayTao = banner.NgayTao,
                NgayCapNhat = banner.NgayCapNhat
            };
        }

        // POST: api/Banner

        [HttpPost]
        public async Task<ActionResult<BannerDto>> PostBanner(CreateBannerDto createDto)
        {
            var banner = new Banner
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Image = createDto.Image,
                Link = createDto.Link,
                Position = createDto.Position,
                Status = createDto.Status,
                SortOrder = createDto.SortOrder
            };

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBanner), new { id = banner.Id }, new BannerDto
            {
                Id = banner.Id,
                Name = banner.Name,
                Description = banner.Description,
                Image = banner.Image,
                Link = banner.Link,
                Position = banner.Position,
                Status = banner.Status,
                SortOrder = banner.SortOrder,
                NgayTao = banner.NgayTao,
                NgayCapNhat = banner.NgayCapNhat
            });
        }

        // PUT: api/Banner/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBanner(int id, UpdateBannerDto updateDto)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            banner.Name = updateDto.Name;
            banner.Description = updateDto.Description;
            banner.Image = updateDto.Image;
            banner.Link = updateDto.Link;
            banner.Position = updateDto.Position;
            banner.Status = updateDto.Status;
            banner.SortOrder = updateDto.SortOrder;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BannerExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Banner/5 (Soft Delete)

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();

            banner.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BannerExists(int id)
        {
            return _context.Banners.Any(e => e.Id == id);
        }
    }
}
