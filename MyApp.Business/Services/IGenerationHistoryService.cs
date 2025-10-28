using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;

namespace MyApp.Business.Services
{
    public interface IGenerationHistoryService
    {
        Task<IEnumerable<GenerationHistoryResponse>> GetAllAsync();
        Task<GenerationHistoryResponse?> GetByIdAsync(int id);
        Task<IEnumerable<GenerationHistoryResponse>> GetByUserIdAsync(int userId);
        Task<IEnumerable<GenerationHistoryResponse>> GetByModelIdAsync(int modelId);
        Task<GenerationHistoryResponse> CreateAsync(GenerationHistoryRequest request);
        Task<GenerationHistoryResponse> UpdateAsync(int id, GenerationHistoryUpdateRequest request);
        Task<bool> DeleteAsync(int id);
    }
}