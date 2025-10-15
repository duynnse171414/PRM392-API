using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;
using MyApp.Business.Services;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipPackageController : ControllerBase
    {
        private readonly IMembershipPackageService _membershipPackageService;

        public MembershipPackageController(IMembershipPackageService membershipPackageService)
        {
            _membershipPackageService = membershipPackageService;
        }

        // GET: api/membershippackage
        // PUBLIC - Tất cả mọi người có thể xem danh sách gói
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MembershipPackageResponse>>> GetAllPackages()
        {
            var packages = await _membershipPackageService.GetAllAsync();
            return Ok(packages);
        }

        // GET: api/membershippackage/{id}
        // PUBLIC - Xem chi tiết một gói
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<MembershipPackageResponse>> GetPackage(int id)
        {
            var package = await _membershipPackageService.GetByIdAsync(id);
            if (package == null)
            {
                return NotFound(new { message = "Package not found" });
            }

            return Ok(package);
        }

        // POST: api/membershippackage
        // CHỈ ADMIN được tạo gói mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MembershipPackageResponse>> CreatePackage([FromBody] MembershipPackageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdPackage = await _membershipPackageService.CreateAsync(request);
                return CreatedAtAction(nameof(GetPackage), new { id = createdPackage.PackageId }, createdPackage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/membershippackage/{id}
        // CHỈ ADMIN được cập nhật gói
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MembershipPackageResponse>> UpdatePackage(int id, [FromBody] MembershipPackageUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedPackage = await _membershipPackageService.UpdateAsync(id, request);
                return Ok(updatedPackage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/membershippackage/{id}
        // CHỈ ADMIN được xóa gói
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            try
            {
                var result = await _membershipPackageService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Package not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
