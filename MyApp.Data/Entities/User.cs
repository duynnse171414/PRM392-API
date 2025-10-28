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
        public int UserId { get; set; } // PK
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!; // store hash
        public UserRole Role { get; set; }
        public string? Email { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation
        public ICollection<Model3D> Models { get; set; } = new List<Model3D>();
        public ICollection<GenerationHistory> GenerationHistories { get; set; } = new List<GenerationHistory>();

        // If subscribe to many packages: either history or many-to-many
        public ICollection<MembershipPackage> SubscribedPackages { get; set; } = new List<MembershipPackage>();
    }
}