using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyHeThongBanHang.Data;
using QuanLyHeThongBanHang.Models;
using QuanLyHeThongBanHang.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace QuanLyHeThongBanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<AppUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        public class RegisterStaffDto
        {
            [JsonPropertyName("email")]
            public string Email { get; set; } = string.Empty;

            [JsonPropertyName("password")]
            public string Password { get; set; } = string.Empty;

            [JsonPropertyName("hoTen")]
            public string HoTen { get; set; } = string.Empty;

            [JsonPropertyName("soDienThoai")]
            public string SoDienThoai { get; set; } = string.Empty;

            [JsonPropertyName("role")]
            public string Role { get; set; } = "Admin";

            [JsonPropertyName("hinhAnh")]
            public string? HinhAnh { get; set; }
        }

        public class LoginDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register-staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDto model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest(new { Status = "Error", Message = "User already exists!" });

            AppUser user = new AppUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { Status = "Error", Message = $"User creation failed! Chi tiết lỗi: {errors}" });
            }

            // Gán quyền linh hoạt từ model.Role
            var roleName = string.IsNullOrEmpty(model.Role) ? "Admin" : model.Role;
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));

            await _userManager.AddToRoleAsync(user, roleName);

            // Create NhanVien profile for this Staff
            // Sử dụng các chữ số cuối của Ticks kết hợp mã ngẫu nhiên để đảm bảo duy nhất
            var staffUnique = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            var nhanVien = new NhanVien
            {
                MaNhanVien = "NV_" + DateTime.Now.ToString("yyMMdd") + "_" + staffUnique,
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                AppUserId = user.Id,
                HinhAnh = model.HinhAnh
            };
            
            try 
            {
                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Nếu lưu profile thất bại, xóa user identity vừa tạo để đảm bảo tính nhất quán (tùy chọn)
                await _userManager.DeleteAsync(user);
                return StatusCode(StatusCodes.Status500InternalServerError, new { 
                    Status = "Error", 
                    Message = $"Lưu hồ sơ nhân viên thất bại! Chi tiết: {ex.InnerException?.Message ?? ex.Message}" 
                });
            }

            return Ok(new { Status = "Success", Message = $"Staff created successfully with role {roleName}!" });
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterStaffDto model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest(new { Status = "Error", Message = "User already exists with this email!" });

            // 1. Kiểm tra xem khách hàng đã tồn tại trong hệ thống (mua tại quầy) chưa
            var existingKhachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.Email == model.Email || (!string.IsNullOrEmpty(model.SoDienThoai) && k.SoDienThoai == model.SoDienThoai));

            if (existingKhachHang != null && !string.IsNullOrEmpty(existingKhachHang.AppUserId))
            {
                // Kiểm tra xem User Identity đó còn tồn tại thực tế không
                var linkedUser = await _userManager.FindByIdAsync(existingKhachHang.AppUserId);
                if (linkedUser != null)
                {
                    return BadRequest(new { Status = "Error", Message = "Email hoặc Số điện thoại này đã được đăng ký và liên kết với một tài khoản khác." });
                }
                
                // Nếu linkedUser không tồn tại (dữ liệu rác), xóa AppUserId cũ để cho phép đăng ký mới
                existingKhachHang.AppUserId = null;
                _context.KhachHangs.Update(existingKhachHang);
                await _context.SaveChangesAsync();
            }

            // 2. Tạo User Identity
            AppUser user = new AppUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                PhoneNumber = model.SoDienThoai
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = $"User creation failed! Chi tiết lỗi: {errors}" });
            }

            // Gán role Customer
            if (!await _roleManager.RoleExistsAsync("Customer"))
                await _roleManager.CreateAsync(new IdentityRole("Customer"));

            await _userManager.AddToRoleAsync(user, "Customer");

            // 3. Liên kết hoặc tạo mới hồ sơ KhachHang
            if (existingKhachHang != null)
            {
                // Liên kết với hồ sơ cũ
                existingKhachHang.AppUserId = user.Id;
                // Cập nhật lại họ tên/email nếu cần (tùy chọn)
                if (string.IsNullOrEmpty(existingKhachHang.Email)) existingKhachHang.Email = model.Email;
                _context.KhachHangs.Update(existingKhachHang);
            }
            else
            {
                // Tạo hồ sơ mới hoàn toàn
                // Tạo mã khách hàng dựa trên ngày tháng và chuỗi ngẫu nhiên
                var customerUnique = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                var khachHang = new KhachHang
                {
                    MaKhachHang = "KH_" + DateTime.Now.ToString("yyMMdd") + "_" + customerUnique,
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    AppUserId = user.Id,
                    HinhAnh = model.HinhAnh
                };
                _context.KhachHangs.Add(khachHang);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Status = "Success", Message = existingKhachHang != null ? "Customer linked and registered successfully!" : "Customer created and registered successfully!" });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                // 1. Tìm thông tin nhân viên (ưu tiên theo ID, sau đó theo Email)
                var userEmailNormal = user.Email?.ToLower().Trim();
                var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(n => n.AppUserId == user.Id);
                if (nhanVien == null && !string.IsNullOrEmpty(userEmailNormal))
                {
                    nhanVien = await _context.NhanViens.FirstOrDefaultAsync(n => n.Email.ToLower() == userEmailNormal);
                    if (nhanVien != null)
                    {
                        nhanVien.AppUserId = user.Id;
                        _context.NhanViens.Update(nhanVien);
                        await _context.SaveChangesAsync();
                    }
                }

                // 2. Nếu không phải nhân viên, tìm thông tin khách hàng
                string? hinhAnh = nhanVien?.HinhAnh;
                string hoTen = nhanVien?.HoTen ?? "";
                string soDienThoai = nhanVien?.SoDienThoai ?? "";
                string diaChi = "";

                if (nhanVien == null && !string.IsNullOrEmpty(userEmailNormal))
                {
                    var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.AppUserId == user.Id || (k.Email != null && k.Email.ToLower() == userEmailNormal));
                    if (khachHang != null)
                    {
                        hoTen = khachHang.HoTen;
                        hinhAnh = khachHang.HinhAnh;
                        soDienThoai = khachHang.SoDienThoai ?? "";
                        diaChi = khachHang.DiaChi ?? "";
                        
                        // Tự động liên kết nếu chưa có ID
                        if (string.IsNullOrEmpty(khachHang.AppUserId))
                        {
                            khachHang.AppUserId = user.Id;
                            _context.KhachHangs.Update(khachHang);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // Fallback cuối cùng nếu không tìm thấy hồ sơ nào
                if (string.IsNullOrEmpty(hoTen))
                {
                    hoTen = user.UserName ?? user.Email ?? "User";
                }

                if (string.IsNullOrEmpty(soDienThoai))
                {
                    soDienThoai = user.PhoneNumber ?? "";
                }

                // 3. Lấy danh sách quyền hạn chi tiết từ database
                var permissions = await _context.UserPermissions
                    .Include(up => up.Function)
                    .Where(up => up.UserId == user.Id)
                    .Select(up => up.Function!.Code)
                    .ToListAsync();

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    roles = userRoles,
                    permissions = permissions, // MỚI: Danh sách quyền chức năng
                    userId = user.Id,
                    email = user.Email,
                    hoTen = hoTen,
                    hinhAnh = hinhAnh,
                    soDienThoai = soDienThoai,
                    diaChi = diaChi
                });
            }
            return Unauthorized();
        }


        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "SuperSecretKeyExample1234567890123456"));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"] ?? "http://localhost:5000",
                audience: _configuration["JWT:ValidAudience"] ?? "http://localhost:5000",
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
