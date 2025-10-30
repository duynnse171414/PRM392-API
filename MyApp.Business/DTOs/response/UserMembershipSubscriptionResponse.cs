using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Business.DTOs.response
{
    public class UserMembershipSubscriptionResponse
    {
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = null!;

        // Package Info
        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;
        public string? PackageDescription { get; set; }
        public decimal PackagePrice { get; set; }
        public int PackageDurationDays { get; set; }

        // Subscription Info
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!; // Active, Expired, Cancelled
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public int DaysRemaining => IsExpired ? 0 : (EndDate - DateTime.UtcNow).Days;

        // Generation Info
        public int RemainingGenerations { get; set; }
        public string RemainingGenerationsDisplay => RemainingGenerations == -1 ? "Unlimited" : RemainingGenerations.ToString();
        public int TotalGenerationsUsed { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
