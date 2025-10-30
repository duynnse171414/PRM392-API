using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;


namespace MyApp.Business.Services
{
    public interface IMembershipPackageService
    {
        // CRUD Package
        Task<IEnumerable<MembershipPackageResponse>> GetAllAsync();
        Task<MembershipPackageResponse?> GetByIdAsync(int id);
        Task<MembershipPackageResponse> CreateAsync(MembershipPackageRequest request);
        Task<MembershipPackageResponse> UpdateAsync(int id, MembershipPackageUpdateRequest request);
        Task<bool> DeleteAsync(int id);

        // Subscription Management
        Task<UserMembershipSubscriptionResponse> PurchasePackageAsync(int userId, int packageId);
        Task<UserMembershipSubscriptionResponse?> GetActiveSubscriptionAsync(int userId);
        Task<IEnumerable<UserMembershipSubscriptionResponse>> GetUserSubscriptionHistoryAsync(int userId);
        Task<bool> CancelSubscriptionAsync(int userId, int subscriptionId);

        // Generation Management
        Task<UserGenerationStatsResponse> GetUserGenerationStatsAsync(int userId);
        Task<bool> CanUserGenerateAsync(int userId);
        Task<bool> ConsumeGenerationAsync(int userId);

        // Admin: Get all subscriptions
        Task<IEnumerable<UserMembershipSubscriptionResponse>> GetAllSubscriptionsAsync();
    }
}
