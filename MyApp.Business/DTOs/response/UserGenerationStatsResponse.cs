using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Business.DTOs.response
{
    public class UserGenerationStatsResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;

        // Current Active Subscription
        public bool HasActiveSubscription { get; set; }
        public UserMembershipSubscriptionResponse? ActiveSubscription { get; set; }

        // Generation Stats
        public bool CanGenerate { get; set; }
        public int RemainingGenerations { get; set; }
        public string RemainingGenerationsDisplay => RemainingGenerations == -1 ? "Unlimited" : RemainingGenerations.ToString();
        public int TotalGenerationsUsed { get; set; }

        // Messages
        public string? Message { get; set; }
    }
}
