using Microsoft.EntityFrameworkCore;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;
using MyApp.Data;
using MyApp.Data.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Business.Services
{
    // Implementation
    public class Model3DService : IModel3DService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IBackblazeStorageService _backblazeService;
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public Model3DService(AppDbContext context, HttpClient httpClient, IBackblazeStorageService backblazeStorageService)
        {
            _context = context;
            _httpClient = httpClient;
            _backblazeService = backblazeStorageService;
        }

        // Helper method để lấy giờ Việt Nam
        private DateTime GetVietnamTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }

        public async Task<IEnumerable<Model3DResponse>> GetAllAsync()
        {
            var models = await _context.Models
                .Include(m => m.User)
                .Include(m => m.GenerationHistories) // Include GenerationHistories để đếm
                .Where(m => m.IsDeleted == false) // Chỉ lấy những model chưa bị xóa
                .ToListAsync();

            return models.Select(m => MapToResponse(m));
        }

        public async Task<Model3DResponse?> GetByIdAsync(int id)
        {
            var model = await _context.Models
                .Include(m => m.User)
                .Include(m => m.GenerationHistories) // Include GenerationHistories để đếm
                .Where(m => m.IsDeleted == false) // Chỉ lấy model chưa bị xóa
                .FirstOrDefaultAsync(m => m.ModelId == id);

            return model != null ? MapToResponse(model) : null;
        }

        public async Task<IEnumerable<Model3DResponse>> GetByUserIdAsync(int userId)
        {
            var models = await _context.Models
                .Include(m => m.User)
                .Include(m => m.GenerationHistories) // Include GenerationHistories để đếm
                .Where(m => m.UserId == userId && m.IsDeleted == false)
                .ToListAsync();

            return models.Select(m => MapToResponse(m));
        }

        public async Task<Model3DResponse> CreateAsync(String base64Img, int userId)
        {
            // 1️⃣ Validate input
            if (string.IsNullOrWhiteSpace(base64Img))
                throw new ArgumentException("Image is required");

            if (userId <= 0)
                throw new ArgumentException("UserId is required and must be greater than 0");

            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
                throw new ArgumentException($"User with ID {userId} does not exist");

            // 2️⃣ Gửi ảnh base64 sang AI model để tạo file 3D
            var glbBase64 = await UploadToAIModel(base64Img);

            // 3️⃣ Lưu file glb tạm và upload lên Backblaze
            var fileUrl = await SaveAndUploadGlbToBackblaze(glbBase64, userId);

            // 4️⃣ Lưu vào DB
            var entity = new Model3D
            {
                FilePath = fileUrl,
                Status = "Successfully",
                UserId = userId,
                CreationDate = GetVietnamTime(),
                IsDeleted = false
            };

            _context.Models.Add(entity);
            await _context.SaveChangesAsync();

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

            existingModel.UpdatedDate = GetVietnamTime();

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
            model.DeletedDate = GetVietnamTime();

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

        private async Task<string> UploadToAIModel(string base64Image)
        {
            var apiUrl = "https://nestable-lucille-nonruinously.ngrok-free.dev/generate"; // ví dụ endpoint
            var requestBody = new { image = base64Image };

            // 🧩 Serialize JSON bằng System.Text.Json
            var json = JsonConvert.SerializeObject(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");


            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            // 🧩 Đọc binary GLB trả về
            var glbBytes = await response.Content.ReadAsByteArrayAsync();

            // 🧩 Encode sang Base64 để tái sử dụng
            var base64Glb = Convert.ToBase64String(glbBytes);

            return base64Glb;
        }



        private async Task<string> SaveAndUploadGlbToBackblaze(string glbData, int userId)
        {
            byte[] fileBytes;

            // 🧩 1️⃣ Xử lý dữ liệu GLB
            if (glbData.StartsWith("glTF"))
            {
                // Nếu response là raw text bắt đầu bằng glTF → đây là binary (bị ép text)
                fileBytes = Encoding.UTF8.GetBytes(glbData);
            }
            else
            {
                try
                {
                    // Nếu là chuỗi base64 hợp lệ
                    fileBytes = Convert.FromBase64String(glbData);
                }
                catch (FormatException)
                {
                    // Nếu không phải base64 cũng không có prefix glTF → có thể là raw bytes (đã decode trước)
                    fileBytes = Encoding.UTF8.GetBytes(glbData);
                }
            }

            // 🧩 2️⃣ Lưu file tạm
            var fileName = $"model_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.glb";
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);
            await File.WriteAllBytesAsync(tempPath, fileBytes);

            // 🧩 3️⃣ Upload lên Backblaze
            var fileUrl = await _backblazeService.UploadFileAsync(tempPath, "models/" + fileName);

            // 🧩 4️⃣ Dọn file tạm
            try
            {
                File.Delete(tempPath);
            }
            catch { /* ignore */ }

            return fileUrl;
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
