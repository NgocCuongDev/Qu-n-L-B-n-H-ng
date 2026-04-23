using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyHeThongBanHang.Migrations
{
    /// <inheritdoc />
    public partial class AddHinhAnhToSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "SanPhams");
        }
    }
}
