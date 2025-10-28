using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Business.Services;
using MyApp.Data.Entities;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/user/profile
        // Lấy thông tin user hiện tại
        [HttpGet("profile")]
        public async Task<ActionResult<User>> GetProfile()
        {
            // Lấy UserId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim);
            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Không trả về password
            user.Password = string.Empty;
            return Ok(user);
        }

        // GET: api/user
        // CHỈ ADMIN mới được xem danh sách user
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();

            // Ẩn password
            foreach (var user in users)
            {
                user.Password = string.Empty;
            }

            return Ok(users);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            // Kiểm tra quyền: chỉ xem được thông tin của chính mình hoặc là Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(currentUserIdClaim);

            if (currentUserId != id && currentUserRole != "Admin")
            {
                return Forbid(); // 403 Forbidden
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Password = string.Empty;
            return Ok(user);
        }

        // DELETE: api/user/{id}
        // CHỈ ADMIN mới được xóa user
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}