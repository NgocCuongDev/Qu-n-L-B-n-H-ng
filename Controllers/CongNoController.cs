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
    [RequirePermission("debts_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class CongNoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CongNoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CongNo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CongNoDto>>> GetCongNos()
        {
            return await _context.CongNos
                .Include(c => c.KhachHang)
                .Include(c => c.DonHang)
                .Select(c => new CongNoDto
                {
                    Id = c.Id,
                    KhachHangId = c.KhachHangId,
                    TenKhachHang = c.KhachHang != null ? c.KhachHang.HoTen : null,
                    DonHangId = c.DonHangId,
                    MaDonHang = c.DonHang != null ? c.DonHang.MaDonHang : null,
                    NgayPhatSinh = c.NgayPhatSinh,
                    SoTienNo = c.SoTienNo,
                    SoTienTra = c.SoTienTra,
                    SoDuCuoi = c.SoDuCuoi,
                    GhiChu = c.GhiChu,
                    NgayTao = c.NgayTao,
                    NgayCapNhat = c.NgayCapNhat
                })
                .ToListAsync();
        }

        // GET: api/CongNo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CongNoDto>> GetCongNo(int id)
        {
            var congNo = await _context.CongNos
                .Include(c => c.KhachHang)
                .Include(c => c.DonHang)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (congNo == null)
            {
                return NotFound();
            }

            return new CongNoDto
            {
                Id = congNo.Id,
                KhachHangId = congNo.KhachHangId,
                TenKhachHang = congNo.KhachHang != null ? congNo.KhachHang.HoTen : null,
                DonHangId = congNo.DonHangId,
                MaDonHang = congNo.DonHang != null ? congNo.DonHang.MaDonHang : null,
                NgayPhatSinh = congNo.NgayPhatSinh,
                SoTienNo = congNo.SoTienNo,
                SoTienTra = congNo.SoTienTra,
                SoDuCuoi = congNo.SoDuCuoi,
                GhiChu = congNo.GhiChu,
                NgayTao = congNo.NgayTao,
                NgayCapNhat = congNo.NgayCapNhat
            };
        }

        // POST: api/CongNo
        [HttpPost]
        public async Task<ActionResult<CongNoDto>> PostCongNo(CreateCongNoDto createDto)
        {
            // Transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var khachHang = await _context.KhachHangs.FindAsync(createDto.KhachHangId);
                if (khachHang == null)
                {
                    return BadRequest("Khách hàng không tồn tại.");
                }

                DonHang? donHang = null;
                if (createDto.DonHangId.HasValue)
                {
                    donHang = await _context.DonHangs.FindAsync(createDto.DonHangId.Value);
                    if (donHang == null)
                    {
                        return BadRequest("Đơn hàng không tồn tại.");
                    }
                }

                // Cập nhật công nợ KH hiện tại
                khachHang.CongNo += createDto.SoTienNo;
                khachHang.CongNo -= createDto.SoTienTra;

                var congNo = new CongNo
                {
                    KhachHangId = createDto.KhachHangId,
                    DonHangId = createDto.DonHangId,
                    SoTienNo = createDto.SoTienNo,
                    SoTienTra = createDto.SoTienTra,
                    SoDuCuoi = khachHang.CongNo, // Lưu số dư ngay sau giao dịch này
                    GhiChu = createDto.GhiChu,
                    NgayPhatSinh = DateTime.Now
                };

                _context.CongNos.Add(congNo);
                
                // Entity Framework will track 'khachHang' modifications automatically.
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetCongNo), new { id = congNo.Id }, new CongNoDto
                {
                    Id = congNo.Id,
                    KhachHangId = congNo.KhachHangId,
                    TenKhachHang = khachHang.HoTen,
                    DonHangId = congNo.DonHangId,
                    MaDonHang = donHang?.MaDonHang,
                    NgayPhatSinh = congNo.NgayPhatSinh,
                    SoTienNo = congNo.SoTienNo,
                    SoTienTra = congNo.SoTienTra,
                    SoDuCuoi = congNo.SoDuCuoi,
                    GhiChu = congNo.GhiChu,
                    NgayTao = congNo.NgayTao,
                    NgayCapNhat = congNo.NgayCapNhat
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Đã xảy ra lỗi khi tạo báo cáo công nợ: " + ex.Message);
            }
        }

        // PUT: api/CongNo/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCongNo(int id, UpdateCongNoDto updateDto)
        {
            var congNo = await _context.CongNos.FindAsync(id);
            if (congNo == null)
            {
                return NotFound();
            }

            // Ghi chú là thứ duy nhất an toàn để sửa sau khi đã chốt giao dịch sổ cái
            congNo.GhiChu = updateDto.GhiChu;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CongNoExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // Soft Delete (lưu ý: xóa thì không tự động rollback số dư KH, trong thực tế sẽ có transaction điều hướng)
        // DELETE: api/CongNo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCongNo(int id)
        {
            var congNo = await _context.CongNos.FindAsync(id);
            if (congNo == null)
            {
                return NotFound();
            }

            congNo.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CongNoExists(int id)
        {
            return _context.CongNos.Any(e => e.Id == id);
        }
    }
}
