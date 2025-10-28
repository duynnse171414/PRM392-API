using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Data.Entities;

namespace MyApp.Business.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _db.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted)
                .ToListAsync();

        public async Task<User?> GetByIdAsync(int id)
            => await _db.Users
                .Include(u => u.Models)
                .FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);

        public async Task<User> CreateAsync(User user, string passwordPlain)
        {
            user.Password = Hash(passwordPlain);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return false;

            u.IsDeleted = true;
            _db.Users.Update(u);
            await _db.SaveChangesAsync();
            return true;
        }

        // ✅ METHOD MỚI: Xác thực user
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);

            if (user == null) return null;

            // Kiểm tra password
            var hashedPassword = Hash(password);
            if (user.Password != hashedPassword) return null;

            return user;
        }

        // ✅ METHOD MỚI: Lấy user theo username
        public async Task<User?> GetByUsernameAsync(string username)
            => await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);

        // ✅ METHOD MỚI: Kiểm tra username đã tồn tại
        public async Task<bool> UsernameExistsAsync(string username)
            => await _db.Users
                .AnyAsync(u => u.Username == username && !u.IsDeleted);

        // ✅ METHOD MỚI: Kiểm tra email đã tồn tại
        public async Task<bool> EmailExistsAsync(string email)
            => await _db.Users
                .AnyAsync(u => u.Email == email && !u.IsDeleted);

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}