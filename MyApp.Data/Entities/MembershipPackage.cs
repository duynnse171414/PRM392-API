using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Data.Entities
{
    public class MembershipPackage
    {
        public int PackageId { get; set; } // PK
        public string PackageName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; } // duration in days
        public int ModelGenerationLimit { get; set; } // Số lượt gen 3D model (-1 = unlimited)
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation - quan hệ với UserMembershipSubscription
        public ICollection<UserMembershipSubscription> UserSubscriptions { get; set; } = new List<UserMembershipSubscription>();
    }
}