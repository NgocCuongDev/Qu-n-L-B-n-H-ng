using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHeThongBanHang.Models
{
    public class UserPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int FunctionId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        [ForeignKey("FunctionId")]
        public virtual AppFunction? Function { get; set; }
    }
}
