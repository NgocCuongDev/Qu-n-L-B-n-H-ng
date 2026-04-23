using System.Text.Json.Serialization;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class DashboardStatsDto
    {
        [JsonPropertyName("totalRevenue")]
        public decimal TotalRevenue { get; set; }

        [JsonPropertyName("totalOrders")]
        public int TotalOrders { get; set; }

        [JsonPropertyName("totalCustomers")]
        public int TotalCustomers { get; set; }

        [JsonPropertyName("totalProducts")]
        public int TotalProducts { get; set; }

        [JsonPropertyName("revenueTrend")]
        public double RevenueTrend { get; set; } // Mock percentage for now

        [JsonPropertyName("recentOrders")]
        public List<RecentOrderDto> RecentOrders { get; set; } = new();

        [JsonPropertyName("salesChartData")]
        public List<ChartDataDto> SalesChartData { get; set; } = new();
    }

    public class RecentOrderDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("maDonHang")]
        public string MaDonHang { get; set; } = string.Empty;

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class ChartDataDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty; // Month or Day name

        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }
}
