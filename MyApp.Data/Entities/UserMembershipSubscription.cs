using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Data.Entities
{
    public enum SubscriptionStatus
    {
        Active,
        Expired,
        Cancelled
    }

    public class UserMembershipSubscription
    {
        public int SubscriptionId { get; set; } // PK
        public int UserId { get; set; }
        public int PackageId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SubscriptionStatus Status { get; set; }

        public int RemainingGenerations { get; set; } // Số lượt còn lại (-1 = unlimited)
        public int TotalGenerationsUsed { get; set; } = 0; // Tổng số lượt đã dùng

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public User User { get; set; } = null!;
        public MembershipPackage Package { get; set; } = null!;
    }
}