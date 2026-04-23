using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHeThongBanHang.Data;
using QuanLyHeThongBanHang.Models;
using System.Security.Claims;

namespace QuanLyHeThongBanHang.Controllers
{
    [Authorize]
    [Route("api/order")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            if (request.order_items == null || !request.order_items.Any())
            {
                return BadRequest(new { message = "Giỏ hàng trống!" });
            }

            // 1. Tìm thông tin khách hàng dựa trên User đang đăng nhập
            var userEmail = User.Identity?.Name;
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == userEmail || k.AppUser!.Email == userEmail);

            if (khachHang == null)
            {
                return BadRequest(new { message = "Không tìm thấy thông tin khách hàng. Vui lòng cập nhật hồ sơ trước khi đặt hàng." });
            }

            // 2. Tìm hoặc tạo nhân viên "Hệ Thống Online" để gán cho đơn hàng
            var systemNhanVien = await _context.NhanViens.FirstOrDefaultAsync(n => n.MaNhanVien == "SYSTEM_WEB" || n.HoTen == "Hệ Thống Online");
            
            if (systemNhanVien == null)
            {
                // Lấy AppUserId của tài khoản đầu tiên có trong hệ thống để gán tạm (vì trường này bắt buộc)
                var firstUser = await _context.Users.FirstOrDefaultAsync();
                
                systemNhanVien = new NhanVien
                {
                    MaNhanVien = "SYSTEM_WEB",
                    HoTen = "Hệ Thống Online",
                    Email = "system@webstore.com",
                    AppUserId = firstUser?.Id ?? "system-id",
                    SoDienThoai = "0000000000",
                    NgayTao = DateTime.Now
                };
                _context.NhanViens.Add(systemNhanVien);
                await _context.SaveChangesAsync();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. Tạo Đơn Hàng mới
                var donHang = new DonHang
                {
                    MaDonHang = $"HD{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}",
                    KhachHangId = khachHang.Id,
                    NhanVienId = systemNhanVien.Id,
                    NgayTao = DateTime.Now,
                    TrangThai = "cho_thanh_toan",
                    ChiTietDonHangs = new List<ChiTietDonHang>()
                };

                decimal tongTienHang = 0;

                // 4. Xử lý từng sản phẩm
                foreach (var item in request.order_items)
                {
                    var sanPham = await _context.SanPhams.FindAsync(item.product_id);
                    if (sanPham == null)
                    {
                        return BadRequest(new { message = $"Sản phẩm ID {item.product_id} không tồn tại!" });
                    }

                    if (sanPham.SoLuongTon < item.quantity)
                    {
                        return BadRequest(new { message = $"Sản phẩm {sanPham.TenSanPham} không đủ số lượng trong kho!" });
                    }

                    var thanhTien = sanPham.GiaBan * item.quantity;
                    tongTienHang += thanhTien;

                    // 4.1. Trừ tồn kho chi tiết trong các kho (TonKhos)
                    var quantitiesToSubtract = item.quantity;
                    var tableTonKhos = await _context.TonKhos
                        .Where(t => t.SanPhamId == item.product_id && t.SoLuong > 0)
                        .OrderByDescending(t => t.SoLuong) // Ưu tiên trừ kho nhiều hàng trước
                        .ToListAsync();

                    if (tableTonKhos.Sum(t => t.SoLuong) < quantitiesToSubtract)
                    {
                        return BadRequest(new { message = $"Sản phẩm {sanPham.TenSanPham} không đủ tổng số lượng trong các kho chi tiết." });
                    }

                    foreach (var tk in tableTonKhos)
                    {
                        if (quantitiesToSubtract <= 0) break;

                        if (tk.SoLuong >= quantitiesToSubtract)
                        {
                            tk.SoLuong -= quantitiesToSubtract;
                            quantitiesToSubtract = 0;
                        }
                        else
                        {
                            quantitiesToSubtract -= tk.SoLuong;
                            tk.SoLuong = 0;
                        }
                    }

                    // 4.2. Cập nhật lại tổng tồn kho ở bảng SanPham để đồng bộ
                    sanPham.SoLuongTon -= item.quantity;

                    donHang.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        SanPhamId = item.product_id,
                        SoLuong = item.quantity,
                        DonGia = sanPham.GiaBan,
                        ThanhTien = thanhTien
                    });
                }

                donHang.TongTienHang = tongTienHang;
                donHang.TongThanhToan = tongTienHang; // Có thể bổ sung phí ship/chiết khấu ở đây

                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                // 5. TỰ ĐỘNG TẠO BẢN GHI THANH TOÁN
                var thanhToan = new ThanhToan
                {
                    DonHangId = donHang.Id,
                    SoTien = donHang.TongThanhToan,
                    PhuongThuc = request.payment_method ?? "cod",
                    MaGiaoDichThamChieu = "ONLINE_" + donHang.MaDonHang,
                    NhanVienThuId = systemNhanVien.Id,
                    NgayThanhToan = DateTime.Now,
                    NgayTao = DateTime.Now
                };

                _context.ThanhToans.Add(thanhToan);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Đặt hàng thành công!",
                    orderId = donHang.Id,
                    orderCode = donHang.MaDonHang
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // GET: api/order/{id}/print
        [HttpGet("{id}/print")]
        public async Task<IActionResult> GetOrderForPrint(int id)
        {
            var order = await _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .Include(d => d.ChiTietDonHangs!)
                    .ThenInclude(ct => ct.SanPham)
                .Include(d => d.ThanhToans)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng!" });

            return Ok(new {
                id = order.Id,
                code = order.MaDonHang,
                date = order.NgayTao,
                status = order.TrangThai,
                totalAmount = order.TongTienHang,
                discount = 0,
                tax = 0,
                finalAmount = order.TongThanhToan,
                paymentMethod = order.ThanhToans?.FirstOrDefault()?.PhuongThuc ?? "cod",
                customer = new {
                    name = order.KhachHang?.HoTen,
                    phone = order.KhachHang?.SoDienThoai,
                    address = order.KhachHang?.DiaChi,
                    email = order.KhachHang?.Email
                },
                staff = new {
                    name = order.NhanVien?.HoTen ?? "Hệ Thống Online"
                },
                items = order.ChiTietDonHangs?.Select(ct => new {
                    productName = ct.SanPham?.TenSanPham,
                    quantity = ct.SoLuong,
                    price = ct.DonGia,
                    total = ct.ThanhTien
                })
            });
        }

        // GET: api/order
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userEmail = User.Identity?.Name;
            var orders = await _context.DonHangs
                .Include(d => d.KhachHang)
                .Where(d => d.KhachHang!.Email == userEmail || d.KhachHang!.AppUser!.Email == userEmail)
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new {
                    id = d.Id,
                    code = d.MaDonHang,
                    total = d.TongThanhToan,
                    status = d.TrangThai,
                    date = d.NgayTao
                })
                .ToListAsync();

            return Ok(new { data = orders });
        }
    }

    // DTOs
    public class OrderRequest
    {
        public string? customer_name { get; set; }
        public string? customer_email { get; set; }
        public string? customer_phone { get; set; }
        public string? customer_address { get; set; }
        public string? note { get; set; }
        public string? payment_method { get; set; }
        public List<OrderItemRequest> order_items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        public int product_id { get; set; }
        public string? product_name { get; set; }
        public int quantity { get; set; }
        public decimal product_price { get; set; }
    }
}
