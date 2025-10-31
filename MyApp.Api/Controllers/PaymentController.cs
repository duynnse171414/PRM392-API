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
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IMembershipPackageService _membershipPackageService;

        public PaymentController(
            IVnPayService vnPayService,
            IMembershipPackageService membershipPackageService)
        {
            _vnPayService = vnPayService;
            _membershipPackageService = membershipPackageService;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay cho gói membership
        /// </summary>
        /// <param name="request">Thông tin thanh toán</param>
        /// <returns>URL để redirect đến cổng thanh toán VNPay</returns>
        [HttpPost("create-vnpay-payment")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<VnPayPaymentResponse>> CreateVnPayPayment([FromBody] VnPayPaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Lấy userId từ token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                // Lấy package để validate và hiển thị thông tin
                var package = await _membershipPackageService.GetByIdAsync(request.PackageId);
                if (package == null)
                {
                    return NotFound(new { message = "Package not found" });
                }

                // Lấy IP address
                string ipAddress = VnPayLibrary.GetIpAddress(HttpContext);

                // Tạo payment URL
                string paymentUrl = _vnPayService.CreatePaymentUrl(userId, request, ipAddress);

                return Ok(new VnPayPaymentResponse
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    Message = $"Redirect to VNPay to pay {package.Price:N0} VND for package {package.PackageName}",
                    OrderId = $"{DateTime.Now.Ticks}_{userId}_{request.PackageId}"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = "VNPay configuration error: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred: " + ex.Message });
            }
        }

        /// <summary>
        /// Callback URL từ VNPay sau khi thanh toán (return url)
        /// </summary>
        [HttpGet("vnpay-return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var response = await _vnPayService.ProcessPaymentReturn(Request.Query);

                if (response.Success)
                {
                    // Redirect đến trang success của frontend với thông tin
                    // Bạn có thể thay đổi URL này theo frontend của bạn
                    return Ok(response);
                }
                else
                {
                    // Redirect đến trang fail của frontend
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Payment processing error: " + ex.Message });
            }
        }

        /// <summary>
        /// IPN (Instant Payment Notification) từ VNPay
        /// Webhook để VNPay thông báo kết quả thanh toán
        /// </summary>
        [HttpGet("vnpay-ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayIPN()
        {
            try
            {
                var response = await _vnPayService.ProcessPaymentReturn(Request.Query);

                if (response.Success)
                {
                    // Trả về response cho VNPay theo format của họ
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                else
                {
                    return Ok(new { RspCode = "99", Message = "Confirm Fail" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { RspCode = "99", Message = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái thanh toán bằng orderId
        /// </summary>
        [HttpGet("payment-status/{orderId}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> GetPaymentStatus(string orderId)
        {
            // Parse orderId để lấy userId
            var orderParts = orderId.Split('_');
            if (orderParts.Length != 3)
            {
                return BadRequest(new { message = "Invalid order ID format" });
            }

            int userId = int.Parse(orderParts[1]);

            // Verify user có quyền xem order này không
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Chỉ cho phép user xem order của mình (hoặc admin)
            if (userId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Lấy subscription mới nhất của user
            var subscription = await _membershipPackageService.GetActiveSubscriptionAsync(userId);

            return Ok(new
            {
                orderId,
                hasActiveSubscription = subscription != null,
                subscription
            });
        }
    }
}
