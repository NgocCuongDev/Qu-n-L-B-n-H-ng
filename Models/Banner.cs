using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models
{
    public class Banner : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty; // Tiêu đề Banner

        [StringLength(500)]
        public string? Description { get; set; } // Mô tả ngắn

        [Required]
        public string Image { get; set; } = string.Empty; // Đường dẫn ảnh

        [StringLength(500)]
        public string? Link { get; set; } // Liên kết khi nhấn vào

        [StringLength(50)]
        public string Position { get; set; } = "slideshow"; // Vị trí (slideshow, sidebar, v.v.)

        public int Status { get; set; } = 1; // 1: Hiện, 0: Ẩn

        public int SortOrder { get; set; } = 0; // Thứ tự sắp xếp
    }
}
