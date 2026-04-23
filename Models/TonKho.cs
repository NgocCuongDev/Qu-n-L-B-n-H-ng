using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    public class TonKho : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SanPhamId { get; set; }

        [Required]
        public int KhoId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int SoLuong { get; set; } = 0;

        // Navigation properties
        [ForeignKey("SanPhamId")]
        public virtual SanPham? SanPham { get; set; }

        [ForeignKey("KhoId")]
        public virtual Kho? Kho { get; set; }
    }
}
