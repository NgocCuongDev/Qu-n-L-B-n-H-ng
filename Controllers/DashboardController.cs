using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHeThongBanHang.Data;
using QuanLyHeThongBanHang.Models.DTOs;

using QuanLyHeThongBanHang.Filters;

namespace QuanLyHeThongBanHang.Controllers
{
    [Authorize]
    [RequirePermission("dashboard_view")]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetStats()
        {
            // 1. Calculate General Stats
            var totalRevenue = await _context.DonHangs
                .Where(d => d.TrangThai == "da_thanh_toan" && !d.IsDeleted)
                .SumAsync(d => d.TongThanhToan);

            var totalOrders = await _context.DonHangs
                .Where(d => !d.IsDeleted)
                .CountAsync();

            var totalCustomers = await _context.KhachHangs
                .Where(d => !d.IsDeleted)
                .CountAsync();

            var totalProducts = await _context.SanPhams
                .Where(d => !d.IsDeleted)
                .CountAsync();

            // 2. Fetch Recent Orders
            var recentOrders = await _context.DonHangs
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.NgayTao)
                .Take(5)
                .Select(d => new RecentOrderDto
                {
                    Id = d.Id,
                    MaDonHang = d.MaDonHang,
                    CustomerName = d.KhachHang != null ? d.KhachHang.HoTen : "Khách lẻ",
                    TotalAmount = d.TongThanhToan,
                    Status = d.TrangThai,
                    CreatedAt = d.NgayTao
                })
                .ToListAsync();

            // 3. Simple Sales Chart Data (Mocking last 6 months for now)
            // In a real scenario, this would group by month
            var chartData = new List<ChartDataDto>
            {
                new ChartDataDto { Name = "Tháng 11", Value = 12000000 },
                new ChartDataDto { Name = "Tháng 12", Value = 18500000 },
                new ChartDataDto { Name = "Tháng 1", Value = 15000000 },
                new ChartDataDto { Name = "Tháng 2", Value = 22000000 },
                new ChartDataDto { Name = "Tháng 3", Value = 35000000 },
                new ChartDataDto { Name = "Tháng 4", Value = totalRevenue } // Using current total as current month for demo
            };

            var stats = new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                TotalProducts = totalProducts,
                RevenueTrend = 15.5, // Mock
                RecentOrders = recentOrders,
                SalesChartData = chartData
            };

            return Ok(stats);
        }
    }
}
