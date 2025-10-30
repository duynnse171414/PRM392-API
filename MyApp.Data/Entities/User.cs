using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Data.Entities
{
    public enum UserRole { Guest, Customer, Admin }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public UserRole Role { get; set; }
        public string? Email { get; set; }
        public bool IsDeleted { get; set; } = false;

        // ✅ SỬA: Không dùng DateTime.UtcNow ở đây, để EF Core xử lý
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Model3D> Models { get; set; } = new List<Model3D>();
        public ICollection<GenerationHistory> GenerationHistories { get; set; } = new List<GenerationHistory>();
        public ICollection<UserMembershipSubscription> MembershipSubscriptions { get; set; } = new List<UserMembershipSubscription>();
    }
}