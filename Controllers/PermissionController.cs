using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHeThongBanHang.Data;
using QuanLyHeThongBanHang.Models;
using System.Text.Json.Serialization;

using QuanLyHeThongBanHang.Filters;

namespace QuanLyHeThongBanHang.Controllers
{
    [Authorize]
    [RequirePermission("permissions_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PermissionController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Permission/functions
        [HttpGet("functions")]
        public async Task<ActionResult<IEnumerable<AppFunction>>> GetFunctions()
        {
            return await _context.AppFunctions.OrderBy(f => f.SortOrder).ToListAsync();
        }

        // GET: api/Permission/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserPermissions(string userId)
        {
            var permissions = await _context.UserPermissions
                .Include(up => up.Function)
                .Where(up => up.UserId == userId)
                .Select(up => up.Function!.Code)
                .ToListAsync();

            return permissions;
        }

        public class AssignPermissionsDto
        {
            [JsonPropertyName("userId")]
            public string UserId { get; set; } = string.Empty;

            [JsonPropertyName("functionCodes")]
            public List<string> FunctionCodes { get; set; } = new List<string>();
        }

        // POST: api/Permission/assign
        [HttpPost("assign")]
        public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null) return NotFound("User not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Remove existing permissions
                var existing = await _context.UserPermissions.Where(up => up.UserId == dto.UserId).ToListAsync();
                _context.UserPermissions.RemoveRange(existing);
                await _context.SaveChangesAsync();

                // Add new permissions
                foreach (var code in dto.FunctionCodes)
                {
                    var function = await _context.AppFunctions.FirstOrDefaultAsync(f => f.Code == code);
                    if (function != null)
                    {
                        _context.UserPermissions.Add(new UserPermission
                        {
                            UserId = dto.UserId,
                            FunctionId = function.Id
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Permissions updated successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error updating permissions: " + ex.Message);
            }
        }
    }
}
