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
    public interface IModel3DService
    {
        Task<IEnumerable<Model3DResponse>> GetAllAsync();
        Task<Model3DResponse?> GetByIdAsync(int id);
        Task<IEnumerable<Model3DResponse>> GetByUserIdAsync(int userId);
        Task<Model3DResponse> CreateAsync(Model3DRequest request);
        Task<Model3DResponse> UpdateAsync(int id, Model3DUpdateRequest request);
        Task<bool> DeleteAsync(int id);
       
    }
}