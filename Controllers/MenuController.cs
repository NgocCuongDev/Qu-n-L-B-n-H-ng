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
    [RequirePermission("menus_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        // =====================
        // PUBLIC ENDPOINTS
        // =====================

        [AllowAnonymous]
        [HttpGet("menus")]
        public async Task<ActionResult<IEnumerable<MenuDto>>> GetPublicMenus([FromQuery] string position = "main")
        {
            var allMenus = await _context.Menus
                .Where(m => m.Status == 1 && m.Position == position)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();

            // Build tree structure
            var topLevelMenus = allMenus.Where(m => m.ParentId == null).ToList();
            var result = topLevelMenus.Select(m => MapMenuToDto(m, allMenus)).ToList();

            return result;
        }

        // =====================
        // ADMIN ENDPOINTS
        // =====================


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuDto>>> GetAdminMenus()
        {
            var menus = await _context.Menus
                .OrderBy(m => m.Position)
                .ThenBy(m => m.SortOrder)
                .ToListAsync();

            return menus.Select(m => MapMenuToDto(m)).ToList();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<MenuDto>> GetMenu(int id)
        {
            var menu = await _context.Menus
                .Include(m => m.Children)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null) return NotFound();

            return MapMenuToDto(menu);
        }


        [HttpPost]
        public async Task<ActionResult<MenuDto>> PostMenu(CreateMenuDto createDto)
        {
            var menu = new Menu
            {
                Name = createDto.Name,
                Link = createDto.Link,
                Type = createDto.Type,
                ParentId = createDto.ParentId,
                Position = createDto.Position,
                SortOrder = createDto.SortOrder,
                Status = createDto.Status
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMenu), new { id = menu.Id }, MapMenuToDto(menu));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenu(int id, CreateMenuDto updateDto)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound();

            menu.Name = updateDto.Name;
            menu.Link = updateDto.Link;
            menu.Type = updateDto.Type;
            menu.ParentId = updateDto.ParentId;
            menu.Position = updateDto.Position;
            menu.SortOrder = updateDto.SortOrder;
            menu.Status = updateDto.Status;
            menu.NgayCapNhat = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound();

            menu.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.Id == id);
        }

        private static MenuDto MapMenuToDto(Menu m, List<Menu>? allItems = null)
        {
            var dto = new MenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Link = m.Link,
                Type = m.Type,
                ParentId = m.ParentId,
                Position = m.Position,
                SortOrder = m.SortOrder,
                Status = m.Status
            };

            if (allItems != null)
            {
                dto.Children = allItems
                    .Where(child => child.ParentId == m.Id)
                    .Select(child => MapMenuToDto(child, allItems))
                    .ToList();
            }

            return dto;
        }
    }
}
