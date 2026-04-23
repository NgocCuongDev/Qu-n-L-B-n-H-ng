#pragma warning disable CS8618
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuanLyHeThongBanHang.Models;

namespace QuanLyHeThongBanHang.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets cho tất cả các bảng
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }
        public DbSet<CongNo> CongNos { get; set; }
        public DbSet<Kho> Khos { get; set; }
        public DbSet<TonKho> TonKhos { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<AppFunction> AppFunctions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        public override int SaveChanges()
        {
            AddAuditInfo();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddAuditInfo();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void AddAuditInfo()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    entity.NgayTao = DateTime.Now;
                }
                entity.NgayCapNhat = DateTime.Now;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === CẤU HÌNH UNIQUE CONSTRAINTS ===
            
            // ... (keeping existing index configs)
            modelBuilder.Entity<KhachHang>().HasIndex(k => k.MaKhachHang).IsUnique();
            modelBuilder.Entity<KhachHang>().HasIndex(k => k.SoDienThoai).IsUnique();
            modelBuilder.Entity<SanPham>().HasIndex(s => s.MaSanPham).IsUnique();
            modelBuilder.Entity<NhanVien>().HasIndex(n => n.MaNhanVien).IsUnique();
            modelBuilder.Entity<NhanVien>().HasIndex(n => n.Email).IsUnique();
            modelBuilder.Entity<DonHang>().HasIndex(d => d.MaDonHang).IsUnique();
            modelBuilder.Entity<ChiTietDonHang>().HasIndex(ct => new { ct.DonHangId, ct.SanPhamId }).IsUnique();
            modelBuilder.Entity<TonKho>().HasIndex(t => new { t.SanPhamId, t.KhoId }).IsUnique();
            modelBuilder.Entity<Banner>().HasIndex(b => b.Name).IsUnique();
            modelBuilder.Entity<Topic>().HasIndex(t => t.Slug).IsUnique();
            modelBuilder.Entity<Post>().HasIndex(p => p.Slug).IsUnique();
            modelBuilder.Entity<Menu>().HasIndex(m => m.Name);
            modelBuilder.Entity<AppFunction>().HasIndex(f => f.Code).IsUnique();

            // Cấu hình Unique cho Danh mục
            modelBuilder.Entity<DanhMuc>()
                .HasIndex(d => d.TenDanhMuc)
                .IsUnique();

            // === CẤU HÌNH GLOBAL QUERY FILTER CHO SOFT DELETE ===
            modelBuilder.Entity<SanPham>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<KhachHang>().HasQueryFilter(k => !k.IsDeleted);
            modelBuilder.Entity<NhanVien>().HasQueryFilter(n => !n.IsDeleted);
            modelBuilder.Entity<Kho>().HasQueryFilter(k => !k.IsDeleted);
            modelBuilder.Entity<DanhMuc>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<Banner>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Menu>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Topic>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted);

            // === CẤU HÌNH KIỂU DỮ LIỆU DECIMAL ===
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var decimalProperties = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

                foreach (var property in decimalProperties)
                {
                    property.SetColumnType("decimal(18,2)");
                }
            }

            // === CẤU HÌNH MỐI QUAN HỆ ===

            // 0. SanPham - DanhMuc (N-1)
            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.DanhMuc)
                .WithMany(d => d.SanPhams)
                .HasForeignKey(s => s.DanhMucId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1. DonHang - KhachHang (N-1)
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.KhachHang)
                .WithMany(k => k.DonHangs)
                .HasForeignKey(d => d.KhachHangId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. DonHang - NhanVien (N-1)
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.NhanVien)
                .WithMany(n => n.DonHangs)
                .HasForeignKey(d => d.NhanVienId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. ChiTietDonHang - DonHang (N-1)
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany(d => d.ChiTietDonHangs)
                .HasForeignKey(ct => ct.DonHangId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. ChiTietDonHang - SanPham (N-1)
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.SanPham)
                .WithMany(s => s.ChiTietDonHangs)
                .HasForeignKey(ct => ct.SanPhamId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4.1 UserPermission relationships
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Function)
                .WithMany(f => f.UserPermissions)
                .HasForeignKey(up => up.FunctionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany() // AppUser doesn't strictly need a collection of permissions unless we add it
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. ThanhToan - DonHang (N-1)
            modelBuilder.Entity<ThanhToan>()
                .HasOne(t => t.DonHang)
                .WithMany(d => d.ThanhToans)
                .HasForeignKey(t => t.DonHangId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. ThanhToan - NhanVien (N-1)
            modelBuilder.Entity<ThanhToan>()
                .HasOne(t => t.NhanVienThu)
                .WithMany(n => n.ThanhToans)
                .HasForeignKey(t => t.NhanVienThuId)
                .OnDelete(DeleteBehavior.Restrict);

            // 7. CongNo - KhachHang (N-1)
            modelBuilder.Entity<CongNo>()
                .HasOne(c => c.KhachHang)
                .WithMany(k => k.CongNos)
                .HasForeignKey(c => c.KhachHangId)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. CongNo - DonHang (N-1) - DonHangId có thể null
            modelBuilder.Entity<CongNo>()
                .HasOne(c => c.DonHang)
                .WithMany(d => d.CongNos)
                .HasForeignKey(c => c.DonHangId)
                .OnDelete(DeleteBehavior.SetNull);

            // 9. TonKho - SanPham (N-1)
            modelBuilder.Entity<TonKho>()
                .HasOne(t => t.SanPham)
                .WithMany(s => s.TonKhos)
                .HasForeignKey(t => t.SanPhamId)
                .OnDelete(DeleteBehavior.Cascade);

            // 10. TonKho - Kho (N-1)
            modelBuilder.Entity<TonKho>()
                .HasOne(t => t.Kho)
                .WithMany(k => k.TonKhos)
                .HasForeignKey(t => t.KhoId)
                .OnDelete(DeleteBehavior.Cascade);

            // 11. Post - Topic (N-1)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Topic)
                .WithMany(t => t.Posts)
                .HasForeignKey(p => p.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            // 12. Menu hierarchy (N-1)
            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 13. Topic hierarchy (N-1)
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(t => t.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}