using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;
using MyApp.Business.Services;
using System.Security.Claims;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipPackageController : ControllerBase
    {
        private readonly IMembershipPackageService _membershipPackageService;
        private readonly IVnPayService _vnPayService; 
        public MembershipPackageController(IMembershipPackageService membershipPackageService, IVnPayService vnPayService)
        {
            _membershipPackageService = membershipPackageService;
            _vnPayService = vnPayService;
        }

        #region Package Management (CRUD)

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

        #endregion

        #region Subscription Management

        // POST: api/membershippackage/purchase
        // Customer mua gói membership
        [HttpPost("purchase")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<object>> PurchasePackage([FromBody] PurchaseMembershipRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                // Kiểm tra package
                var package = await _membershipPackageService.GetByIdAsync(request.PackageId);
                if (package == null)
                {
                    return NotFound(new { message = "Package not found" });
                }

                // ✅ NẾU LÀ GÓI FREE → Kích hoạt luôn
                if (package.Price == 0)
                {
                    var subscription = await _membershipPackageService.PurchasePackageAsync(userId, request.PackageId);

                    return Ok(new
                    {
                        success = true,
                        message = "Free package activated successfully",
                        packageType = "free",
                        subscription
                    });
                }

                // === PAID: tạo URL thanh toán VNPay ===
                // Lấy IP client để truyền cho service (đúng chữ ký interface)
                var ipAddress = HttpContext.Connection.RemoteIpAddress?
                                    .MapToIPv4().ToString() ?? "127.0.0.1";

                // Map request sang DTO mà IVnPayService đang dùng
                var payReq = new VnPayPaymentRequest
                {
                    PackageId = request.PackageId, // lấy từ request hiện có
                    BankCode = "VNBANK",          // mặc định: VNPay ATM nội địa; đổi nếu cần
                    Locale = "vn"               // mặc định: tiếng Việt
                };

                // GỌI ĐÚNG CHỮ KÝ (3 tham số)
                var paymentUrl = _vnPayService.CreatePaymentUrl(userId, payReq, ipAddress);

                return Ok(new
                {
                    success = true,
                    message = "Please complete payment via VNPAY",
                    packageType = "paid",
                    paymentUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/membershippackage/my-subscription
        // Lấy subscription hiện tại của user đang login
        [HttpGet("my-subscription")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<UserMembershipSubscriptionResponse>> GetMyActiveSubscription()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var subscription = await _membershipPackageService.GetActiveSubscriptionAsync(userId);

                if (subscription == null)
                {
                    return NotFound(new { message = "No active subscription found" });
                }

                return Ok(subscription);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/membershippackage/my-subscription/history
        // Lấy lịch sử subscription của user
        [HttpGet("my-subscription/history")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<IEnumerable<UserMembershipSubscriptionResponse>>> GetMySubscriptionHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var subscriptions = await _membershipPackageService.GetUserSubscriptionHistoryAsync(userId);
                return Ok(subscriptions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/membershippackage/subscription/{subscriptionId}/cancel
        // Hủy subscription
        [HttpPost("subscription/{subscriptionId}/cancel")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> CancelSubscription(int subscriptionId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _membershipPackageService.CancelSubscriptionAsync(userId, subscriptionId);

                if (!result)
                {
                    return NotFound(new { message = "Subscription not found" });
                }

                return Ok(new { message = "Subscription cancelled successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/membershippackage/subscriptions/all
        // ADMIN - Xem tất cả subscriptions
        [HttpGet("subscriptions/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserMembershipSubscriptionResponse>>> GetAllSubscriptions()
        {
            var subscriptions = await _membershipPackageService.GetAllSubscriptionsAsync();
            return Ok(subscriptions);
        }

        // GET: api/membershippackage/subscriptions/user/{userId}
        // ADMIN - Xem subscription history của một user cụ thể
        [HttpGet("subscriptions/user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserMembershipSubscriptionResponse>>> GetUserSubscriptions(int userId)
        {
            try
            {
                var subscriptions = await _membershipPackageService.GetUserSubscriptionHistoryAsync(userId);
                return Ok(subscriptions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Generation Management

        // GET: api/membershippackage/generation-stats
        // Xem thống kê số lượt gen của user hiện tại
        [HttpGet("generation-stats")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<UserGenerationStatsResponse>> GetMyGenerationStats()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var stats = await _membershipPackageService.GetUserGenerationStatsAsync(userId);
                return Ok(stats);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/membershippackage/can-generate
        // Kiểm tra user có thể gen model không
        [HttpGet("can-generate")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<object>> CanGenerate()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var canGenerate = await _membershipPackageService.CanUserGenerateAsync(userId);
                var stats = await _membershipPackageService.GetUserGenerationStatsAsync(userId);

                return Ok(new
                {
                    canGenerate,
                    remainingGenerations = stats.RemainingGenerations,
                    message = canGenerate ? "You can generate models" : stats.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
    }
}