using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;
using MyApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;

namespace MyApp.Business.Services
{
    public class MembershipPackageService : IMembershipPackageService
    {
        private readonly AppDbContext _context;

        public MembershipPackageService(AppDbContext context)
        {
            _context = context;
        }

        #region CRUD Package

        public async Task<IEnumerable<MembershipPackageResponse>> GetAllAsync()
        {
            var packages = await _context.MembershipPackages
                .Include(p => p.UserSubscriptions.Where(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted))
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            return packages.Select(p => MapToResponse(p));
        }

        public async Task<MembershipPackageResponse?> GetByIdAsync(int id)
        {
            var package = await _context.MembershipPackages
                .Include(p => p.UserSubscriptions.Where(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted))
                .FirstOrDefaultAsync(p => p.PackageId == id && !p.IsDeleted);

            return package != null ? MapToResponse(package) : null;
        }

        public async Task<MembershipPackageResponse> CreateAsync(MembershipPackageRequest request)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.PackageName))
            {
                throw new ArgumentException("Package name is required");
            }
            if (request.Price < 0)
            {
                throw new ArgumentException("Price must be greater than or equal to 0");
            }
            if (request.DurationDays <= 0)
            {
                throw new ArgumentException("Duration days must be greater than 0");
            }
            if (request.ModelGenerationLimit < -1)
            {
                throw new ArgumentException("Model generation limit must be -1 (unlimited) or greater than 0");
            }

            // Kiểm tra trùng tên package
            var existingPackage = await _context.MembershipPackages
                .FirstOrDefaultAsync(p => p.PackageName == request.PackageName && !p.IsDeleted);

            if (existingPackage != null)
            {
                throw new ArgumentException("Package name already exists");
            }

            var entity = new MembershipPackage
            {
                PackageName = request.PackageName,
                Description = request.Description,
                Price = request.Price,
                DurationDays = request.DurationDays,
                ModelGenerationLimit = request.ModelGenerationLimit,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.MembershipPackages.Add(entity);
            await _context.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<MembershipPackageResponse> UpdateAsync(int id, MembershipPackageUpdateRequest request)
        {
            var existingPackage = await _context.MembershipPackages
                .Include(p => p.UserSubscriptions.Where(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted))
                .FirstOrDefaultAsync(p => p.PackageId == id && !p.IsDeleted);

            if (existingPackage == null)
            {
                throw new ArgumentException("Package not found");
            }

            if (!string.IsNullOrWhiteSpace(request.PackageName))
            {
                var duplicateName = await _context.MembershipPackages
                    .AnyAsync(p => p.PackageName == request.PackageName
                                && p.PackageId != id
                                && !p.IsDeleted);

                if (duplicateName)
                {
                    throw new ArgumentException("Package name already exists");
                }
                existingPackage.PackageName = request.PackageName;
            }

            if (request.Description != null)
            {
                existingPackage.Description = request.Description;
            }

            if (request.Price.HasValue)
            {
                if (request.Price.Value < 0)
                {
                    throw new ArgumentException("Price must be greater than or equal to 0");
                }
                existingPackage.Price = request.Price.Value;
            }

            if (request.DurationDays.HasValue)
            {
                if (request.DurationDays.Value <= 0)
                {
                    throw new ArgumentException("Duration days must be greater than 0");
                }
                existingPackage.DurationDays = request.DurationDays.Value;
            }

            if (request.ModelGenerationLimit.HasValue)
            {
                if (request.ModelGenerationLimit.Value < -1)
                {
                    throw new ArgumentException("Model generation limit must be -1 (unlimited) or greater than 0");
                }
                existingPackage.ModelGenerationLimit = request.ModelGenerationLimit.Value;
            }

            existingPackage.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToResponse(existingPackage);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var package = await _context.MembershipPackages
                .Include(p => p.UserSubscriptions)
                .FirstOrDefaultAsync(p => p.PackageId == id && !p.IsDeleted);

            if (package == null)
            {
                return false;
            }

            // Kiểm tra có subscription active không
            var activeSubscriptions = package.UserSubscriptions
                .Count(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted);

            if (activeSubscriptions > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete package. There are {activeSubscriptions} active subscriptions.");
            }

            package.IsDeleted = true;
            package.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Subscription Management

        public async Task<UserMembershipSubscriptionResponse> PurchasePackageAsync(int userId, int packageId)
        {
            // Kiểm tra user tồn tại
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Kiểm tra package tồn tại
            var package = await _context.MembershipPackages
                .FirstOrDefaultAsync(p => p.PackageId == packageId && !p.IsDeleted);

            if (package == null)
            {
                throw new ArgumentException("Package not found");
            }

            // Hủy các subscription active cũ (nếu có)
            var oldSubscriptions = await _context.UserMembershipSubscriptions
                .Where(s => s.UserId == userId
                         && s.Status == SubscriptionStatus.Active
                         && !s.IsDeleted)
                .ToListAsync();

            foreach (var oldSub in oldSubscriptions)
            {
                oldSub.Status = SubscriptionStatus.Cancelled;
                oldSub.UpdatedAt = DateTime.UtcNow;
            }

            // Tạo subscription mới
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(package.DurationDays);

            var newSubscription = new UserMembershipSubscription
            {
                UserId = userId,
                PackageId = packageId,
                StartDate = startDate,
                EndDate = endDate,
                Status = SubscriptionStatus.Active,
                RemainingGenerations = package.ModelGenerationLimit,
                TotalGenerationsUsed = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.UserMembershipSubscriptions.Add(newSubscription);
            await _context.SaveChangesAsync();

            // Load lại để có đầy đủ navigation properties
            await _context.Entry(newSubscription).Reference(s => s.User).LoadAsync();
            await _context.Entry(newSubscription).Reference(s => s.Package).LoadAsync();

            return MapToSubscriptionResponse(newSubscription);
        }

        public async Task<UserMembershipSubscriptionResponse?> GetActiveSubscriptionAsync(int userId)
        {
            var subscription = await _context.UserMembershipSubscriptions
                .Include(s => s.User)
                .Include(s => s.Package)
                .Where(s => s.UserId == userId
                         && s.Status == SubscriptionStatus.Active
                         && s.EndDate > DateTime.UtcNow
                         && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            return subscription != null ? MapToSubscriptionResponse(subscription) : null;
        }

        public async Task<IEnumerable<UserMembershipSubscriptionResponse>> GetUserSubscriptionHistoryAsync(int userId)
        {
            var subscriptions = await _context.UserMembershipSubscriptions
                .Include(s => s.User)
                .Include(s => s.Package)
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return subscriptions.Select(s => MapToSubscriptionResponse(s));
        }

        public async Task<bool> CancelSubscriptionAsync(int userId, int subscriptionId)
        {
            var subscription = await _context.UserMembershipSubscriptions
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId
                                       && s.UserId == userId
                                       && !s.IsDeleted);

            if (subscription == null)
            {
                return false;
            }

            if (subscription.Status != SubscriptionStatus.Active)
            {
                throw new InvalidOperationException("Subscription is not active");
            }

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<UserMembershipSubscriptionResponse>> GetAllSubscriptionsAsync()
        {
            var subscriptions = await _context.UserMembershipSubscriptions
                .Include(s => s.User)
                .Include(s => s.Package)
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return subscriptions.Select(s => MapToSubscriptionResponse(s));
        }

        #endregion

        #region Generation Management

        public async Task<UserGenerationStatsResponse> GetUserGenerationStatsAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var activeSubscription = await GetActiveSubscriptionAsync(userId);
            var canGenerate = await CanUserGenerateAsync(userId);

            var response = new UserGenerationStatsResponse
            {
                UserId = userId,
                Username = user.Username,
                HasActiveSubscription = activeSubscription != null,
                ActiveSubscription = activeSubscription,
                CanGenerate = canGenerate,
                RemainingGenerations = activeSubscription?.RemainingGenerations ?? 0,
                TotalGenerationsUsed = activeSubscription?.TotalGenerationsUsed ?? 0
            };

            if (!canGenerate)
            {
                if (activeSubscription == null)
                {
                    response.Message = "You don't have an active subscription. Please purchase a package to generate models.";
                }
                else if (activeSubscription.IsExpired)
                {
                    response.Message = "Your subscription has expired. Please renew to continue generating models.";
                }
                else if (activeSubscription.RemainingGenerations == 0)
                {
                    response.Message = "You have used all your model generations. Please upgrade your package.";
                }
            }

            return response;
        }

        public async Task<bool> CanUserGenerateAsync(int userId)
        {
            var subscription = await _context.UserMembershipSubscriptions
                .Where(s => s.UserId == userId
                         && s.Status == SubscriptionStatus.Active
                         && s.EndDate > DateTime.UtcNow
                         && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (subscription == null)
            {
                return false;
            }

            // -1 = unlimited
            if (subscription.RemainingGenerations == -1)
            {
                return true;
            }

            return subscription.RemainingGenerations > 0;
        }

        public async Task<bool> ConsumeGenerationAsync(int userId)
        {
            var subscription = await _context.UserMembershipSubscriptions
                .Where(s => s.UserId == userId
                         && s.Status == SubscriptionStatus.Active
                         && s.EndDate > DateTime.UtcNow
                         && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (subscription == null)
            {
                return false;
            }

            // Unlimited, không cần trừ
            if (subscription.RemainingGenerations == -1)
            {
                subscription.TotalGenerationsUsed++;
                subscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            // Kiểm tra còn lượt không
            if (subscription.RemainingGenerations <= 0)
            {
                return false;
            }

            // Trừ 1 lượt
            subscription.RemainingGenerations--;
            subscription.TotalGenerationsUsed++;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Helper Methods

        private MembershipPackageResponse MapToResponse(MembershipPackage entity)
        {
            return new MembershipPackageResponse
            {
                PackageId = entity.PackageId,
                PackageName = entity.PackageName,
                Description = entity.Description,
                Price = entity.Price,
                DurationDays = entity.DurationDays,
                ModelGenerationLimit = entity.ModelGenerationLimit,
                ActiveSubscriberCount = entity.UserSubscriptions?
                    .Count(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted) ?? 0,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        private UserMembershipSubscriptionResponse MapToSubscriptionResponse(UserMembershipSubscription entity)
        {
            return new UserMembershipSubscriptionResponse
            {
                SubscriptionId = entity.SubscriptionId,
                UserId = entity.UserId,
                Username = entity.User?.Username ?? "",
                PackageId = entity.PackageId,
                PackageName = entity.Package?.PackageName ?? "",
                PackageDescription = entity.Package?.Description,
                PackagePrice = entity.Package?.Price ?? 0,
                PackageDurationDays = entity.Package?.DurationDays ?? 0,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Status = entity.Status.ToString(),
                RemainingGenerations = entity.RemainingGenerations,
                TotalGenerationsUsed = entity.TotalGenerationsUsed,
                CreatedAt = entity.CreatedAt
            };
        }

        #endregion
    }
}