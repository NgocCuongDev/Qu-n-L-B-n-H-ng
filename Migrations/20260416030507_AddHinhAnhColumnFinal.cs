using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyHeThongBanHang.Migrations
{
    /// <inheritdoc />
    public partial class AddHinhAnhColumnFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "KhachHangs");
        }
    }
}
