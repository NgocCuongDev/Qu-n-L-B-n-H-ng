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
    [RequirePermission("products_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/SanPham
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SanPhamDto>>> GetSanPhams()
        {
            return await _context.SanPhams
                .Include(s => s.DanhMuc)
                .Select(s => new SanPhamDto
                {
                    Id = s.Id,
                    MaSanPham = s.MaSanPham,
                    TenSanPham = s.TenSanPham,
                    DanhMucId = s.DanhMucId,
                    TenDanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : null,
                    DonViTinh = s.DonViTinh,
                    GiaNhap = s.GiaNhap,
                    GiaBan = s.GiaBan,
                    GiaGiam = s.GiaGiam,
                    SoLuongTon = s.SoLuongTon,
                    NgayTao = s.NgayTao,
                    NgayCapNhat = s.NgayCapNhat,
                    HinhAnh = s.HinhAnh
                })
                .ToListAsync();
        }

        // GET: api/SanPham/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<SanPhamDto>> GetSanPham(int id)
        {
            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            return new SanPhamDto
            {
                Id = sanPham.Id,
                MaSanPham = sanPham.MaSanPham,
                TenSanPham = sanPham.TenSanPham,
                DanhMucId = sanPham.DanhMucId,
                TenDanhMuc = sanPham.DanhMuc != null ? sanPham.DanhMuc.TenDanhMuc : null,
                DonViTinh = sanPham.DonViTinh,
                GiaNhap = sanPham.GiaNhap,
                GiaBan = sanPham.GiaBan,
                GiaGiam = sanPham.GiaGiam,
                SoLuongTon = sanPham.SoLuongTon,
                NgayTao = sanPham.NgayTao,
                NgayCapNhat = sanPham.NgayCapNhat,
                HinhAnh = sanPham.HinhAnh
            };
        }

        // POST: api/SanPham
        [HttpPost]
        public async Task<ActionResult<SanPhamDto>> PostSanPham(CreateSanPhamDto createDto)
        {
            // Kiểm tra mã sản phẩm đã tồn tại chưa (mặc dù đã có unique constraint ở database)
            if (await _context.SanPhams.AnyAsync(s => s.MaSanPham == createDto.MaSanPham))
            {
                return BadRequest("Mã sản phẩm đã tồn tại.");
            }

            var sanPham = new SanPham
            {
                MaSanPham = createDto.MaSanPham,
                TenSanPham = createDto.TenSanPham,
                DanhMucId = createDto.DanhMucId,
                DonViTinh = createDto.DonViTinh,
                GiaNhap = createDto.GiaNhap,
                GiaBan = createDto.GiaBan,
                GiaGiam = createDto.GiaGiam,
                SoLuongTon = 0,
                HinhAnh = createDto.HinhAnh
            };

            _context.SanPhams.Add(sanPham);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSanPham), new { id = sanPham.Id }, new SanPhamDto
            {
                Id = sanPham.Id,
                MaSanPham = sanPham.MaSanPham,
                TenSanPham = sanPham.TenSanPham,
                DanhMucId = sanPham.DanhMucId,
                TenDanhMuc = (await _context.DanhMucs.FindAsync(sanPham.DanhMucId))?.TenDanhMuc,
                DonViTinh = sanPham.DonViTinh,
                GiaNhap = sanPham.GiaNhap,
                GiaBan = sanPham.GiaBan,
                GiaGiam = sanPham.GiaGiam,
                SoLuongTon = sanPham.SoLuongTon,
                NgayTao = sanPham.NgayTao,
                NgayCapNhat = sanPham.NgayCapNhat,
                HinhAnh = sanPham.HinhAnh
            });
        }

        // PUT: api/SanPham/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSanPham(int id, UpdateSanPhamDto updateDto)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }

            sanPham.TenSanPham = updateDto.TenSanPham;
            sanPham.DanhMucId = updateDto.DanhMucId;
            sanPham.DonViTinh = updateDto.DonViTinh;
            sanPham.GiaNhap = updateDto.GiaNhap;
            sanPham.GiaBan = updateDto.GiaBan;
            sanPham.GiaGiam = updateDto.GiaGiam;
            sanPham.HinhAnh = updateDto.HinhAnh;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SanPhamExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/SanPham/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSanPham(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }

            sanPham.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- PUBLIC ENDPOINTS FOR STOREFRONT ---

        [AllowAnonymous]
        [HttpGet("public/new")]
        public async Task<ActionResult<IEnumerable<SanPhamDto>>> GetNewProducts([FromQuery] int limit = 8)
        {
            return await _context.SanPhams
                .Where(s => !s.IsDeleted)
                .Include(s => s.DanhMuc)
                .OrderByDescending(s => s.NgayTao)
                .Take(limit)
                .Select(s => new SanPhamDto
                {
                    Id = s.Id,
                    MaSanPham = s.MaSanPham,
                    TenSanPham = s.TenSanPham,
                    DanhMucId = s.DanhMucId,
                    TenDanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : null,
                    DonViTinh = s.DonViTinh,
                    GiaNhap = s.GiaNhap,
                    GiaBan = s.GiaBan,
                    GiaGiam = s.GiaGiam,
                    SoLuongTon = s.SoLuongTon,
                    NgayTao = s.NgayTao,
                    NgayCapNhat = s.NgayCapNhat,
                    HinhAnh = s.HinhAnh
                })
                .ToListAsync();
        }

        [AllowAnonymous]
        [HttpGet("public/sale")]
        public async Task<ActionResult<IEnumerable<SanPhamDto>>> GetSaleProducts([FromQuery] int limit = 8)
        {
            return await _context.SanPhams
                .Where(s => !s.IsDeleted && s.GiaGiam > 0)
                .Include(s => s.DanhMuc)
                .OrderByDescending(s => s.NgayCapNhat)
                .Take(limit)
                .Select(s => new SanPhamDto
                {
                    Id = s.Id,
                    MaSanPham = s.MaSanPham,
                    TenSanPham = s.TenSanPham,
                    DanhMucId = s.DanhMucId,
                    TenDanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : null,
                    DonViTinh = s.DonViTinh,
                    GiaNhap = s.GiaNhap,
                    GiaBan = s.GiaBan,
                    GiaGiam = s.GiaGiam,
                    SoLuongTon = s.SoLuongTon,
                    NgayTao = s.NgayTao,
                    NgayCapNhat = s.NgayCapNhat,
                    HinhAnh = s.HinhAnh
                })
                .ToListAsync();
        }

        [AllowAnonymous]
        [HttpGet("public/category/{id}")]
        public async Task<ActionResult<IEnumerable<SanPhamDto>>> GetProductsByCategory(int id, [FromQuery] int limit = 8)
        {
            return await _context.SanPhams
                .Where(s => !s.IsDeleted && s.DanhMucId == id)
                .Include(s => s.DanhMuc)
                .OrderByDescending(s => s.NgayTao)
                .Take(limit)
                .Select(s => new SanPhamDto
                {
                    Id = s.Id,
                    MaSanPham = s.MaSanPham,
                    TenSanPham = s.TenSanPham,
                    DanhMucId = s.DanhMucId,
                    TenDanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : null,
                    DonViTinh = s.DonViTinh,
                    GiaNhap = s.GiaNhap,
                    GiaBan = s.GiaBan,
                    GiaGiam = s.GiaGiam,
                    SoLuongTon = s.SoLuongTon,
                    NgayTao = s.NgayTao,
                    NgayCapNhat = s.NgayCapNhat,
                    HinhAnh = s.HinhAnh
                })
                .ToListAsync();
        }

        // GET: api/SanPham/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportProducts()
        {
            var products = await _context.SanPhams
                .Where(s => !s.IsDeleted)
                .Include(s => s.DanhMuc)
                .OrderBy(s => s.MaSanPham)
                .ToListAsync();

            var builder = new System.Text.StringBuilder();
            // Header
            builder.AppendLine("Mã Sản Phẩm,Tên Sản Phẩm,Danh Mục,Đơn Vị Tính,Giá Nhập,Giá Bán,Giá Giảm,Số Lượng Tồn,Ngày Tạo");

            foreach (var p in products)
            {
                // Xử lý dấu phẩy trong tên sản phẩm để tránh lệch cột CSV
                string tenSP = p.TenSanPham?.Replace(",", " ") ?? "";
                string tenDM = p.DanhMuc?.TenDanhMuc?.Replace(",", " ") ?? "";
                
                builder.AppendLine($"{p.MaSanPham},{tenSP},{tenDM},{p.DonViTinh},{p.GiaNhap},{p.GiaBan},{p.GiaGiam},{p.SoLuongTon},{p.NgayTao:dd/MM/yyyy HH:mm}");
            }

            // Sử dụng UTF-8 with BOM để Excel nhận diện được tiếng Việt
            var csvData = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
            
            return File(csvData, "text/csv", $"DanhSachSanPham_{DateTime.Now:yyyyMMdd}.csv");
        }

        private bool SanPhamExists(int id)
        {
            return _context.SanPhams.Any(e => e.Id == id);
        }
    }
}
