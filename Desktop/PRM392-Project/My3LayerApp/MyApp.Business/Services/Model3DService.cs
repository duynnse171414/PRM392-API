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
    // Implementation
    public class Model3DService : IModel3DService
    {
        private readonly AppDbContext _context;

        public Model3DService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Model3DResponse>> GetAllAsync()
        {
            var models = await _context.Models
                .Include(m => m.User)
                .Where(m => m.IsDeleted == false) // Chỉ lấy những model chưa bị xóa
                .ToListAsync();

            return models.Select(m => MapToResponse(m));
        }

        public async Task<Model3DResponse?> GetByIdAsync(int id)
        {
            var model = await _context.Models
                .Include(m => m.User)
                .Where(m => m.IsDeleted == false) // Chỉ lấy model chưa bị xóa
                .FirstOrDefaultAsync(m => m.ModelId == id);

            return model != null ? MapToResponse(model) : null;
        }

        public async Task<IEnumerable<Model3DResponse>> GetByUserIdAsync(int userId)
        {
            var models = await _context.Models
                .Include(m => m.User)
                .Where(m => m.UserId == userId && m.IsDeleted == false)
                .ToListAsync();

            return models.Select(m => MapToResponse(m));
        }

        public async Task<Model3DResponse> CreateAsync(Model3DRequest request)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                throw new ArgumentException("FilePath is required");
            }

            // Map từ Request DTO sang Entity
            var entity = new Model3D
            {
                FilePath = request.FilePath,
                Status = request.Status ?? "Pending",
                
                CreationDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Models.Add(entity);
            await _context.SaveChangesAsync();

            // Load User để có đầy đủ thông tin cho response
            await _context.Entry(entity).Reference(m => m.User).LoadAsync();

            return MapToResponse(entity);
        }

        public async Task<Model3DResponse> UpdateAsync(int id, Model3DUpdateRequest request)
        {
            var existingModel = await _context.Models
                .Include(m => m.User)
                .Where(m => m.IsDeleted == false) // Không cho update model đã xóa
                .FirstOrDefaultAsync(m => m.ModelId == id);

            if (existingModel == null)
            {
                throw new ArgumentException("Model not found");
            }

            // Chỉ update những field không null
            if (!string.IsNullOrWhiteSpace(request.FilePath))
            {
                existingModel.FilePath = request.FilePath;
            }

            if (request.Status != null)
            {
                existingModel.Status = request.Status;
            }

            existingModel.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(existingModel);
        }

        // Soft Delete - Xóa mềm
        public async Task<bool> DeleteAsync(int id)
        {
            var model = await _context.Models
                .Where(m => !m.IsDeleted)
                .FirstOrDefaultAsync(m => m.ModelId == id);

            if (model == null)
            {
                return false;
            }

            // Đánh dấu là đã xóa thay vì xóa thật
            model.IsDeleted = true;
            model.DeletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // Hard Delete - Xóa vĩnh viễn (chỉ admin nên dùng)
        public async Task<bool> HardDeleteAsync(int id)
        {
            var model = await _context.Models.FindAsync(id);
            if (model == null)
            {
                return false;
            }

            _context.Models.Remove(model);
            await _context.SaveChangesAsync();

            return true;
        }

        // Khôi phục model đã xóa mềm
        public async Task<bool> RestoreAsync(int id)
        {
            var model = await _context.Models
                .Where(m => m.IsDeleted)
                .FirstOrDefaultAsync(m => m.ModelId == id);

            if (model == null)
            {
                return false;
            }

            model.IsDeleted = false;
            model.DeletedDate = null;

            await _context.SaveChangesAsync();

            return true;
        }

        // Helper method để map Entity sang Response DTO
        private Model3DResponse MapToResponse(Model3D entity)
        {
            return new Model3DResponse
            {
                ModelId = entity.ModelId,
                FilePath = entity.FilePath,
                CreationDate = entity.CreationDate,
                Status = entity.Status,
                UserId = entity.UserId,
                UserName = entity.User?.Username
               
            };
        }
    }
}