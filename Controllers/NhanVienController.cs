using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHeThongBanHang.Data;
using QuanLyHeThongBanHang.Models;
using QuanLyHeThongBanHang.Models.DTOs;

using QuanLyHeThongBanHang.Filters;

namespace QuanLyHeThongBanHang.Controllers
{
    [Authorize]
    [RequirePermission("employees_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class NhanVienController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public NhanVienController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/NhanVien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhanVienDto>>> GetNhanViens()
        {
            var nhanViens = await _context.NhanViens
                .Include(n => n.DonHangs)
                .Include(n => n.ThanhToans)
                .ToListAsync();

            var result = new List<NhanVienDto>();
            foreach (var n in nhanViens)
            {
                var appUser = await _userManager.FindByIdAsync(n.AppUserId);
                var roles = appUser != null ? await _userManager.GetRolesAsync(appUser) : new List<string>();
                result.Add(new NhanVienDto
                {
                    Id = n.Id,
                    MaNhanVien = n.MaNhanVien,
                    HoTen = n.HoTen,
                    Email = n.Email,
                    SoDienThoai = n.SoDienThoai,
                    AppUserId = n.AppUserId,
                    NgayTao = n.NgayTao,
                    NgayCapNhat = n.NgayCapNhat,
                    SoDonHang = n.DonHangs?.Count ?? 0,
                    SoThanhToan = n.ThanhToans?.Count ?? 0,
                    Role = roles.FirstOrDefault() ?? "N/A",
                    HinhAnh = n.HinhAnh
                });
            }
            return result;
        }

        // GET: api/NhanVien/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NhanVienDto>> GetNhanVien(int id)
        {
            var nhanVien = await _context.NhanViens
                .Include(n => n.DonHangs)
                .Include(n => n.ThanhToans)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            var appUser = await _userManager.FindByIdAsync(nhanVien.AppUserId);
            var roles = appUser != null ? await _userManager.GetRolesAsync(appUser) : new List<string>();

            return new NhanVienDto
            {
                Id = nhanVien.Id,
                MaNhanVien = nhanVien.MaNhanVien,
                HoTen = nhanVien.HoTen,
                Email = nhanVien.Email,
                SoDienThoai = nhanVien.SoDienThoai,
                AppUserId = nhanVien.AppUserId,
                NgayTao = nhanVien.NgayTao,
                NgayCapNhat = nhanVien.NgayCapNhat,
                SoDonHang = nhanVien.DonHangs?.Count ?? 0,
                SoThanhToan = nhanVien.ThanhToans?.Count ?? 0,
                Role = roles.FirstOrDefault() ?? "N/A",
                HinhAnh = nhanVien.HinhAnh
            };
        }

        // POST: api/NhanVien
        // Note: In real app, you should create NhanVien via AuthController during Registration.
        [HttpPost]
        public async Task<ActionResult<NhanVienDto>> PostNhanVien(CreateNhanVienDto createDto)
        {
            if (await _context.NhanViens.AnyAsync(n => n.MaNhanVien == createDto.MaNhanVien))
            {
                return BadRequest("Mã nhân viên đã tồn tại.");
            }

            if (await _context.NhanViens.AnyAsync(n => n.Email == createDto.Email))
            {
                return BadRequest("Email đã được sử dụng.");
            }

            var nhanVien = new NhanVien
            {
                MaNhanVien = createDto.MaNhanVien,
                HoTen = createDto.HoTen,
                Email = createDto.Email,
                SoDienThoai = createDto.SoDienThoai,
                AppUserId = createDto.AppUserId,
                HinhAnh = createDto.HinhAnh
            };

            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNhanVien), new { id = nhanVien.Id }, new NhanVienDto
            {
                Id = nhanVien.Id,
                MaNhanVien = nhanVien.MaNhanVien,
                HoTen = nhanVien.HoTen,
                Email = nhanVien.Email,
                SoDienThoai = nhanVien.SoDienThoai,
                AppUserId = nhanVien.AppUserId,
                NgayTao = nhanVien.NgayTao,
                NgayCapNhat = nhanVien.NgayCapNhat,
                HinhAnh = nhanVien.HinhAnh
            });
        }

        // PUT: api/NhanVien/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNhanVien(int id, [FromBody] UpdateNhanVienDto updateDto)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            nhanVien.HoTen = updateDto.HoTen;
            nhanVien.SoDienThoai = updateDto.SoDienThoai;
            nhanVien.HinhAnh = updateDto.HinhAnh;
            nhanVien.NgayCapNhat = DateTime.Now;

            // Cập nhật Role nếu có yêu cầu thay đổi
            if (!string.IsNullOrEmpty(updateDto.Role))
            {
                var appUser = await _userManager.FindByIdAsync(nhanVien.AppUserId);
                if (appUser != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(appUser);
                    // Xóa tất cả các role cũ
                    if (currentRoles.Count > 0)
                    {
                        await _userManager.RemoveFromRolesAsync(appUser, currentRoles);
                    }
                    // Gán role mới
                    await _userManager.AddToRoleAsync(appUser, updateDto.Role);
                }
            }

            try
            {
                _context.Entry(nhanVien).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NhanVienExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/NhanVien/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNhanVien(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            nhanVien.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NhanVienExists(int id)
        {
            return _context.NhanViens.Any(e => e.Id == id);
        }
    }
}
