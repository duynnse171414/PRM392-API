using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;
using MyApp.Data;
using MyApp.Data.Entities;

namespace MyApp.Business.Services
{
    public class GenerationHistoryService : IGenerationHistoryService
    {
        private readonly AppDbContext _context;
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public GenerationHistoryService(AppDbContext context)
        {
            _context = context;
        }

        private DateTime GetVietnamTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }

        public async Task<IEnumerable<GenerationHistoryResponse>> GetAllAsync()
        {
            var histories = await _context.GenerationHistories
                .Include(h => h.User)
                .Include(h => h.Model3D)
                .Where(h => h.IsDeleted == false) // Chỉ lấy những history chưa bị xóa
                .ToListAsync();

            return histories.Select(h => MapToResponse(h));
        }

        public async Task<GenerationHistoryResponse?> GetByIdAsync(int id)
        {
            var history = await _context.GenerationHistories
                .Include(h => h.User)
                .Include(h => h.Model3D)
                .Where(h => h.IsDeleted == false) // Chỉ lấy history chưa bị xóa
                .FirstOrDefaultAsync(h => h.HistoryId == id);

            return history != null ? MapToResponse(history) : null;
        }

        public async Task<IEnumerable<GenerationHistoryResponse>> GetByUserIdAsync(int userId)
        {
            var histories = await _context.GenerationHistories
                .Include(h => h.User)
                .Include(h => h.Model3D)
                .Where(h => h.UserId == userId && h.IsDeleted == false)
                .OrderByDescending(h => h.GenerationDate)
                .ToListAsync();

            return histories.Select(h => MapToResponse(h));
        }

        public async Task<IEnumerable<GenerationHistoryResponse>> GetByModelIdAsync(int modelId)
        {
            var histories = await _context.GenerationHistories
                .Include(h => h.User)
                .Include(h => h.Model3D)
                .Where(h => h.ModelId == modelId && h.IsDeleted == false)
                .OrderByDescending(h => h.GenerationDate)
                .ToListAsync();

            return histories.Select(h => MapToResponse(h));
        }

        public async Task<GenerationHistoryResponse> CreateAsync(GenerationHistoryRequest request)
        {
            // Validate User exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == request.UserId);
            if (!userExists)
            {
                throw new ArgumentException("User not found");
            }

            // Validate Model exists
            var modelExists = await _context.Models.AnyAsync(m => m.ModelId == request.ModelId && !m.IsDeleted);
            if (!modelExists)
            {
                throw new ArgumentException("Model not found");
            }

            var entity = new GenerationHistory
            {
                UserId = request.UserId,
                ModelId = request.ModelId,
                InputImages = request.InputImages,
                GenerationDate = GetVietnamTime(),
                IsDeleted = false
            };

            _context.GenerationHistories.Add(entity);
            await _context.SaveChangesAsync();

            // Load related entities
            await _context.Entry(entity).Reference(h => h.User).LoadAsync();
            await _context.Entry(entity).Reference(h => h.Model3D).LoadAsync();

            return MapToResponse(entity);
        }

        public async Task<GenerationHistoryResponse> UpdateAsync(int id, GenerationHistoryUpdateRequest request)
        {
            var existingHistory = await _context.GenerationHistories
                .Include(h => h.User)
                .Include(h => h.Model3D)
                .Where(h => h.IsDeleted == false) // Không cho update history đã xóa
                .FirstOrDefaultAsync(h => h.HistoryId == id);

            if (existingHistory == null)
            {
                throw new ArgumentException("Generation history not found");
            }

            // Update ModelId if provided
            if (request.ModelId.HasValue)
            {
                var modelExists = await _context.Models.AnyAsync(m => m.ModelId == request.ModelId.Value && !m.IsDeleted);
                if (!modelExists)
                {
                    throw new ArgumentException("Model not found");
                }
                existingHistory.ModelId = request.ModelId.Value;
            }

            // Update InputImages if provided
            if (request.InputImages != null)
            {
                existingHistory.InputImages = request.InputImages;
            }

            await _context.SaveChangesAsync();

            // Reload Model3D if it was changed
            if (request.ModelId.HasValue)
            {
                await _context.Entry(existingHistory).Reference(h => h.Model3D).LoadAsync();
            }

            return MapToResponse(existingHistory);
        }

        // Soft Delete - Xóa mềm
        public async Task<bool> DeleteAsync(int id)
        {
            var history = await _context.GenerationHistories
                .Where(h => !h.IsDeleted)
                .FirstOrDefaultAsync(h => h.HistoryId == id);

            if (history == null)
            {
                return false;
            }

            // Đánh dấu là đã xóa thay vì xóa thật
            history.IsDeleted = true;
            history.DeletedDate = GetVietnamTime();

            await _context.SaveChangesAsync();

            return true;
        }

        // Hard Delete - Xóa vĩnh viễn (chỉ admin nên dùng)
        public async Task<bool> HardDeleteAsync(int id)
        {
            var history = await _context.GenerationHistories.FindAsync(id);
            if (history == null)
            {
                return false;
            }

            _context.GenerationHistories.Remove(history);
            await _context.SaveChangesAsync();

            return true;
        }

        // Khôi phục history đã xóa mềm
        public async Task<bool> RestoreAsync(int id)
        {
            var history = await _context.GenerationHistories
                .Where(h => h.IsDeleted)
                .FirstOrDefaultAsync(h => h.HistoryId == id);

            if (history == null)
            {
                return false;
            }

            history.IsDeleted = false;
            history.DeletedDate = null;

            await _context.SaveChangesAsync();

            return true;
        }

        private GenerationHistoryResponse MapToResponse(GenerationHistory entity)
        {
            return new GenerationHistoryResponse
            {
                HistoryId = entity.HistoryId,
                UserId = entity.UserId,
                UserName = entity.User?.Username,
                ModelId = entity.ModelId,
                ModelFilePath = entity.Model3D?.FilePath,
                GenerationDate = entity.GenerationDate.Kind == DateTimeKind.Utc
                    ? TimeZoneInfo.ConvertTimeFromUtc(entity.GenerationDate, VietnamTimeZone)
                    : entity.GenerationDate,
                InputImages = entity.InputImages
            };
        }
    }
}