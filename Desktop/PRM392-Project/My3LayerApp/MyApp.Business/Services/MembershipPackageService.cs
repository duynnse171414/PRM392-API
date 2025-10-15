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

        public async Task<IEnumerable<MembershipPackageResponse>> GetAllAsync()
        {
            var packages = await _context.MembershipPackages
                .Include(p => p.Subscribers)
                .Where(p => !p.IsDeleted) // ✅ Chỉ lấy package chưa bị xóa
                .ToListAsync();

            return packages.Select(p => MapToResponse(p));
        }

        public async Task<MembershipPackageResponse?> GetByIdAsync(int id)
        {
            var package = await _context.MembershipPackages
                .Include(p => p.Subscribers)
                .FirstOrDefaultAsync(p => p.PackageId == id && !p.IsDeleted); // ✅ Kiểm tra IsDeleted

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

            // Kiểm tra trùng tên package (chỉ trong các package chưa xóa)
            var existingPackage = await _context.MembershipPackages
                .FirstOrDefaultAsync(p => p.PackageName == request.PackageName && !p.IsDeleted); // ✅

            if (existingPackage != null)
            {
                throw new ArgumentException("Package name already exists");
            }

            // Map từ Request DTO sang Entity
            var entity = new MembershipPackage
            {
                PackageName = request.PackageName,
                Description = request.Description,
                Price = request.Price,
                DurationDays = request.DurationDays,
                IsDeleted = false // ✅ Mặc định là chưa xóa
            };

            _context.MembershipPackages.Add(entity);
            await _context.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<MembershipPackageResponse> UpdateAsync(int id, MembershipPackageUpdateRequest request)
        {
            var existingPackage = await _context.MembershipPackages
                .Include(p => p.Subscribers)
                .FirstOrDefaultAsync(p => p.PackageId == id && !p.IsDeleted); // ✅

            if (existingPackage == null)
            {
                throw new ArgumentException("Package not found");
            }

            // Chỉ update những field không null
            if (!string.IsNullOrWhiteSpace(request.PackageName))
            {
                // Kiểm tra trùng tên (ngoại trừ chính nó, chỉ trong package chưa xóa)
                var duplicateName = await _context.MembershipPackages
                    .AnyAsync(p => p.PackageName == request.PackageName
                                && p.PackageId != id
                                && !p.IsDeleted); // ✅

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

            await _context.SaveChangesAsync();

            return MapToResponse(existingPackage);
        }

        // ✅ XÓA MỀM: Đánh dấu IsDeleted = true thay vì xóa thật
        public async Task<bool> DeleteAsync(int id)
        {
            var package = await _context.MembershipPackages
                .Include(p => p.Subscribers)
                .FirstOrDefaultAsync(p => p.PackageId == id && !p.IsDeleted);

            if (package == null)
            {
                return false;
            }

            // Kiểm tra xem có user nào đang subscribe không
            var activeSubscribers = package.Subscribers.Count(s => !s.IsDeleted);
            if (activeSubscribers > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete package. There are {activeSubscribers} active subscribers.");
            }

            // Xóa mềm: chỉ đánh dấu IsDeleted = true
            package.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        // Helper method để map Entity sang Response DTO
        private MembershipPackageResponse MapToResponse(MembershipPackage entity)
        {
            return new MembershipPackageResponse
            {
                PackageId = entity.PackageId,
                PackageName = entity.PackageName,
                Description = entity.Description,
                Price = entity.Price,
                DurationDays = entity.DurationDays,
                SubscriberCount = entity.Subscribers?.Count(s => !s.IsDeleted) ?? 0 // ✅ Chỉ đếm subscriber chưa xóa
            };
        }
    }
}