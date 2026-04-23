using System.ComponentModel.DataAnnotations;

namespace QuanLyHeThongBanHang.Models
{
    public class Kho : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TenKho { get; set; } = string.Empty;

        [StringLength(255)]
        public string? DiaChi { get; set; }

        // Navigation property
        public virtual ICollection<TonKho>? TonKhos { get; set; }
    }

}
