using Microsoft.AspNetCore.Identity;

namespace QuanLyHeThongBanHang.Models
{
    public class AppUser : IdentityUser
    {
        // Base IdentityUser provides fields like:
        // Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        
        // You can add custom properties here if needed in the future
    }
}
