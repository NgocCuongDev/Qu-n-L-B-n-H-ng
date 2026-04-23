using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models.DTOs
{
    public class BannerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Image { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string Position { get; set; } = "slideshow";
        public int Status { get; set; }
        public int SortOrder { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateBannerDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string Image { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Link { get; set; }

        [StringLength(50)]
        public string Position { get; set; } = "slideshow";

        public int Status { get; set; } = 1;

        public int SortOrder { get; set; } = 0;
    }

    public class UpdateBannerDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string Image { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Link { get; set; }

        [StringLength(50)]
        public string Position { get; set; } = "slideshow";

        public int Status { get; set; }

        public int SortOrder { get; set; }
    }
}
