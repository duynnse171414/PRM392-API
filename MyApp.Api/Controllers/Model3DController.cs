using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Business;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;
using MyApp.Business.Services;
using MyApp.Data.Entities;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class Model3DController : ControllerBase
    {
        private readonly IModel3DService _model3DService;

        public Model3DController(IModel3DService model3DService)
        {
            _model3DService = model3DService;
        }

        // GET: api/model3d
        // Lấy tất cả models của user hiện tại
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model3DResponse>>> GetMyModels()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim);
            var models = await _model3DService.GetByUserIdAsync(userId);

            return Ok(models);
        }

        // GET: api/model3d/all
        // CHỈ ADMIN mới được xem tất cả models
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Model3DResponse>>> GetAllModels()
        {
            var models = await _model3DService.GetAllAsync();
            return Ok(models);
        }

        // GET: api/model3d/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Model3DResponse>> GetModel(int id)
        {
            var model = await _model3DService.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound(new { message = "Model not found" });
            }

            // Kiểm tra quyền: chỉ xem được model của chính mình hoặc là Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(currentUserIdClaim);
            if (model.UserId != currentUserId && currentUserRole != "Admin")
            {
                return Forbid(); // 403 Forbidden
            }

            return Ok(model);
        }

        // GET: api/model3d/user/{userId}
        // CHỈ ADMIN hoặc chính user đó mới được xem
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Model3DResponse>>> GetModelsByUser(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(currentUserIdClaim);
            if (currentUserId != userId && currentUserRole != "Admin")
            {
                return Forbid();
            }

            var models = await _model3DService.GetByUserIdAsync(userId);
            return Ok(models);
        }

        // POST: api/model3d
        [HttpPost]
        public async Task<ActionResult<Model3DResponse>> CreateModel([FromBody] Model3DRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Set UserId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            request.UserId = int.Parse(userIdClaim);

            try
            {
                var createdModel = await _model3DService.CreateAsync(request);
                return CreatedAtAction(nameof(GetModel), new { id = createdModel.ModelId }, createdModel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/model3d/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<Model3DResponse>> UpdateModel(int id, [FromBody] Model3DUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra model có tồn tại
            var existingModel = await _model3DService.GetByIdAsync(id);
            if (existingModel == null)
            {
                return NotFound(new { message = "Model not found" });
            }

            // Kiểm tra quyền: chỉ update được model của chính mình hoặc là Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(currentUserIdClaim);
            if (existingModel.UserId != currentUserId && currentUserRole != "Admin")
            {
                return Forbid();
            }

            try
            {
                var updatedModel = await _model3DService.UpdateAsync(id, request);
                return Ok(updatedModel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/model3d/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            var model = await _model3DService.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound(new { message = "Model not found" });
            }

            // Kiểm tra quyền: chỉ xóa được model của chính mình hoặc là Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(currentUserIdClaim);
            if (model.UserId != currentUserId && currentUserRole != "Admin")
            {
                return Forbid();
            }

            var result = await _model3DService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}