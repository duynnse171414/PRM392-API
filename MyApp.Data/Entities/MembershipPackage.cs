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

        public bool IsDeleted { get; set; } = false;


        // Many-to-many with User (subscribes)
        public ICollection<User> Subscribers { get; set; } = new List<User>();
    }
}
