using Microsoft.EntityFrameworkCore;
using MyApp.Data.Entities;

namespace MyApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Model3D> Models { get; set; } = null!;
        public DbSet<MembershipPackage> MembershipPackages { get; set; } = null!;
        public DbSet<GenerationHistory> GenerationHistories { get; set; } = null!;

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Cấu hình mặc định cho tất cả decimal
            configurationBuilder.Properties<decimal>()
                .HavePrecision(18, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình MySQL auto-increment - THÊM DÒNG NÀY
            modelBuilder.UseCollation("utf8mb4_general_ci");

            // Định nghĩa Khóa chính (Primary Keys)
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Model3D>().HasKey(m => m.ModelId);
            modelBuilder.Entity<MembershipPackage>().HasKey(p => p.PackageId);
            modelBuilder.Entity<GenerationHistory>().HasKey(h => h.HistoryId);

            // Cấu hình Auto Increment cho Primary Keys
            modelBuilder.Entity<User>()
                .Property(u => u.UserId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Model3D>()
                .Property(m => m.ModelId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<MembershipPackage>()
                .Property(p => p.PackageId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<GenerationHistory>()
                .Property(h => h.HistoryId)
                .ValueGeneratedOnAdd();

            // Quan hệ 1-nhiều: User -> Models
            modelBuilder.Entity<User>()
                .HasMany(u => u.Models)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ 1-nhiều: User -> GenerationHistories
            modelBuilder.Entity<User>()
                .HasMany(u => u.GenerationHistories)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ 1-nhiều: Model3D -> GenerationHistories
            modelBuilder.Entity<Model3D>()
                .HasMany(m => m.GenerationHistories)
                .WithOne(h => h.Model3D)
                .HasForeignKey(h => h.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ nhiều-nhiều: User <-> MembershipPackage
            modelBuilder.Entity<User>()
                .HasMany(u => u.SubscribedPackages)
                .WithMany(p => p.Subscribers)
                .UsingEntity<Dictionary<string, object>>(
                    "UserPackage",
                    j => j.HasOne<MembershipPackage>().WithMany().HasForeignKey("PackageId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "PackageId");
                        j.ToTable("UserPackage");
                    });
        }
    }
}