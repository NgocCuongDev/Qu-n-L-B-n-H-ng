using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyHeThongBanHang.Migrations
{
    /// <inheritdoc />
    public partial class AddGiaGiamToSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GiaGiam",
                table: "SanPhams",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiaGiam",
                table: "SanPhams");
        }
    }
}
