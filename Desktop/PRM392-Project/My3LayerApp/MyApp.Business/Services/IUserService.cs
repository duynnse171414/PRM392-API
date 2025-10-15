using System.Collections.Generic;
using System.Threading.Tasks;
using MyApp.Data.Entities;  // ← ĐÃ THÊM DÒNG NÀY

namespace MyApp.Business.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user, string passwordPlain);
        Task<bool> DeleteAsync(int id);

        Task<User?> AuthenticateAsync(string username, string password);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }
}