using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Business.DTOs;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;
using MyApp.Business.Services;
using MyApp.Data.Entities;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IMembershipPackageService _membershipPackageService; // ✅ Thêm service

        public AuthController(
            IUserService userService,
            IJwtService jwtService,
            IMembershipPackageService membershipPackageService) // ✅ Inject service
        {
            _userService = userService;
            _jwtService = jwtService;
            _membershipPackageService = membershipPackageService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            // Kiểm tra username đã tồn tại
            if (await _userService.UsernameExistsAsync(request.Username))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Username already in use"
                });
            }

            // Kiểm tra email đã tồn tại
            if (await _userService.EmailExistsAsync(request.Email))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Email already in use"
                });
            }

            // Tạo user mới
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = UserRole.Customer // Mặc định là Customer
            };

            var createdUser = await _userService.CreateAsync(user, request.Password);

            // ✅ TỰ ĐỘNG ASSIGN FREE TRIAL PACKAGE (PackageId = 1)
            try
            {
                await _membershipPackageService.PurchasePackageAsync(createdUser.UserId, 1);
            }
            catch (Exception ex)
            {
                // Log error nhưng vẫn cho register thành công
                Console.WriteLine($"Warning: Could not assign free trial package: {ex.Message}");
            }

            return Ok(new RegisterResponse
            {
                Success = true,
                Message = "Register Successful. Free trial package activated!",
                User = new UserInfo
                {
                    UserId = createdUser.UserId,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    Role = createdUser.Role.ToString()
                }
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // Xác thực user
            var user = await _userService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Incorrect username or password"
                });
            }

            // Generate token
            var token = _jwtService.GenerateToken(user);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login Successful",
                Token = token,
                User = new UserInfo
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            });
        }
    }
}