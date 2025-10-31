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
        public DbSet<UserMembershipSubscription> UserMembershipSubscriptions { get; set; } = null!;
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Cấu hình mặc định cho tất cả decimal
            configurationBuilder.Properties<decimal>()
                .HavePrecision(18, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình MySQL auto-increment
            modelBuilder.UseCollation("utf8mb4_general_ci");

            // ============ Định nghĩa Khóa chính (Primary Keys) ============
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Model3D>().HasKey(m => m.ModelId);
            modelBuilder.Entity<MembershipPackage>().HasKey(p => p.PackageId);
            modelBuilder.Entity<GenerationHistory>().HasKey(h => h.HistoryId);
            modelBuilder.Entity<UserMembershipSubscription>().HasKey(s => s.SubscriptionId);
            modelBuilder.Entity<PaymentTransaction>().HasKey(pt => pt.TransactionId);

            // ============ Cấu hình Auto Increment cho Primary Keys ============
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

            modelBuilder.Entity<UserMembershipSubscription>()
                .Property(s => s.SubscriptionId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<PaymentTransaction>()
                .Property(pt => pt.TransactionId)
                .ValueGeneratedOnAdd();

            // ============ Cấu hình User Entity ============
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Password)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.Email)
                    .HasMaxLength(255);

                entity.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // ✅ Cấu hình CreatedAt với default value
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email);
            });

            // ============ Cấu hình MembershipPackage Entity ============
            modelBuilder.Entity<MembershipPackage>(entity =>
            {
                entity.Property(p => p.PackageName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.Description)
                    .HasMaxLength(500);

                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

                entity.Property(p => p.ModelGenerationLimit)
                    .IsRequired();

                entity.HasIndex(p => p.PackageName);
            });

            // ============ Cấu hình UserMembershipSubscription Entity ============
            modelBuilder.Entity<UserMembershipSubscription>(entity =>
            {
                entity.Property(s => s.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(s => s.RemainingGenerations)
                    .IsRequired();

                entity.Property(s => s.TotalGenerationsUsed)
                    .IsRequired()
                    .HasDefaultValue(0);

                // Index để tìm kiếm nhanh
                entity.HasIndex(s => new { s.UserId, s.Status, s.EndDate });
                entity.HasIndex(s => s.Status);
            });

            // ============ Quan hệ 1-nhiều: User -> Models ============
            modelBuilder.Entity<User>()
                .HasMany(u => u.Models)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============ Quan hệ 1-nhiều: User -> GenerationHistories ============
            modelBuilder.Entity<User>()
                .HasMany(u => u.GenerationHistories)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============ Quan hệ 1-nhiều: Model3D -> GenerationHistories ============
            modelBuilder.Entity<Model3D>()
                .HasMany(m => m.GenerationHistories)
                .WithOne(h => h.Model3D)
                .HasForeignKey(h => h.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ Quan hệ 1-nhiều: User -> UserMembershipSubscriptions ============
            modelBuilder.Entity<User>()
                .HasMany(u => u.MembershipSubscriptions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============ Quan hệ 1-nhiều: MembershipPackage -> UserMembershipSubscriptions ============
            modelBuilder.Entity<MembershipPackage>()
                .HasMany(p => p.UserSubscriptions)
                .WithOne(s => s.Package)
                .HasForeignKey(s => s.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ Cấu hình PaymentTransaction Entity ============
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.Property(pt => pt.OrderId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(pt => pt.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending");

                entity.Property(pt => pt.Amount)
                    .HasPrecision(18, 2);

                entity.Property(pt => pt.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.Property(pt => pt.IsDeleted)
                    .HasDefaultValue(false);

                // Unique index on OrderId for idempotency
                entity.HasIndex(pt => pt.OrderId).IsUnique();
                entity.HasIndex(pt => new { pt.UserId, pt.Status });
                entity.HasIndex(pt => pt.CreatedAt);
            });

            // ============ Quan hệ: PaymentTransaction -> User ============
            modelBuilder.Entity<User>()
                .HasMany<PaymentTransaction>()
                .WithOne(pt => pt.User)
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============ Quan hệ: PaymentTransaction -> MembershipPackage ============
            modelBuilder.Entity<MembershipPackage>()
                .HasMany<PaymentTransaction>()
                .WithOne(pt => pt.Package)
                .HasForeignKey(pt => pt.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ Seed Data - Sample Packages ============
            modelBuilder.Entity<MembershipPackage>().HasData(
                new MembershipPackage
                {
                    PackageId = 1,
                    PackageName = "Free Trial",
                    Description = "Gói dùng thử miễn phí với 5 lượt gen model 3D",
                    Price = 0,
                    DurationDays = 7,
                    ModelGenerationLimit = 5,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new MembershipPackage
                {
                    PackageId = 2,
                    PackageName = "Basic",
                    Description = "Gói cơ bản cho người dùng thông thường với 50 lượt gen/tháng",
                    Price = 99000,
                    DurationDays = 30,
                    ModelGenerationLimit = 50,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new MembershipPackage
                {
                    PackageId = 3,
                    PackageName = "Premium",
                    Description = "Gói cao cấp với 200 lượt gen/tháng và ưu tiên hỗ trợ",
                    Price = 299000,
                    DurationDays = 30,
                    ModelGenerationLimit = 200,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new MembershipPackage
                {
                    PackageId = 4,
                    PackageName = "Pro",
                    Description = "Gói chuyên nghiệp với số lượt gen không giới hạn",
                    Price = 499000,
                    DurationDays = 30,
                    ModelGenerationLimit = -1, // -1 = unlimited
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}