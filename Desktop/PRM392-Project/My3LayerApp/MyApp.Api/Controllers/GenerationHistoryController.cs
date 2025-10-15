using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;
using MyApp.Business.Services;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GenerationHistoryController : ControllerBase
    {
        private readonly IGenerationHistoryService _historyService;

        public GenerationHistoryController(IGenerationHistoryService historyService)
        {
            _historyService = historyService;
        }

        // GET: api/generationhistory
        // Lấy tất cả generation history của user hiện tại
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenerationHistoryResponse>>> GetMyHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim);
            var histories = await _historyService.GetByUserIdAsync(userId);

            return Ok(histories);
        }

        // GET: api/generationhistory/all
        // CHỈ ADMIN mới được xem tất cả histories
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GenerationHistoryResponse>>> GetAllHistories()
        {
            var histories = await _historyService.GetAllAsync();
            return Ok(histories);
        }

        // GET: api/generationhistory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GenerationHistoryResponse>> GetHistory(int id)
        {
            var history = await _historyService.GetByIdAsync(id);
            if (history == null)
            {
                return NotFound(new { message = "Generation history not found" });
            }

            // Kiểm tra quyền: chỉ xem được history của chính mình hoặc là Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(currentUserIdClaim);
            if (history.UserId != currentUserId && currentUserRole != "Admin")
            {
                return Forbid(); // 403 Forbidden
            }

            return Ok(history);
        }

        // GET: api/generationhistory/user/{userId}
        // CHỈ ADMIN hoặc chính user đó mới được xem
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GenerationHistoryResponse>>> GetHistoriesByUser(int userId)
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

            var histories = await _historyService.GetByUserIdAsync(userId);
            return Ok(histories);
        }

        // GET: api/generationhistory/model/{modelId}
        // Lấy tất cả histories của một model
        [HttpGet("model/{modelId}")]
        public async Task<ActionResult<IEnumerable<GenerationHistoryResponse>>> GetHistoriesByModel(int modelId)
        {
            var histories = await _historyService.GetByModelIdAsync(modelId);

            // Kiểm tra quyền: chỉ xem được history của models thuộc về mình hoặc là Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(currentUserIdClaim);

            // Filter histories nếu không phải Admin
            if (currentUserRole != "Admin")
            {
                histories = histories.Where(h => h.UserId == currentUserId);
            }

            return Ok(histories);
        }

        // POST: api/generationhistory
        [HttpPost]
        public async Task<ActionResult<GenerationHistoryResponse>> CreateHistory([FromBody] GenerationHistoryRequest request)
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
                var createdHistory = await _historyService.CreateAsync(request);
                return CreatedAtAction(nameof(GetHistory), new { id = createdHistory.HistoryId }, createdHistory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/generationhistory/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<GenerationHistoryResponse>> UpdateHistory(int id, [FromBody] GenerationHistoryUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra history có tồn tại
            var existingHistory = await _historyService.GetByIdAsync(id);
            if (existingHistory == null)
            {
                return NotFound(new { message = "Generation history not found" });
            }

            // Kiểm tra quyền: chỉ update được history của chính mình hoặc là Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(currentUserIdClaim);
            if (existingHistory.UserId != currentUserId && currentUserRole != "Admin")
            {
                return Forbid();
            }

            try
            {
                var updatedHistory = await _historyService.UpdateAsync(id, request);
                return Ok(updatedHistory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/generationhistory/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            var history = await _historyService.GetByIdAsync(id);
            if (history == null)
            {
                return NotFound(new { message = "Generation history not found" });
            }

            // Kiểm tra quyền: chỉ xóa được history của chính mình hoặc là Admin
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(currentUserIdClaim);
            if (history.UserId != currentUserId && currentUserRole != "Admin")
            {
                return Forbid();
            }

            var result = await _historyService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}