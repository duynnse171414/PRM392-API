using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;
using MyApp.Data;
using MyApp.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Business.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IMembershipPackageService _membershipPackageService;

        public VnPayService(
            IConfiguration configuration, 
            AppDbContext context,
            IMembershipPackageService membershipPackageService)
        {
            _configuration = configuration;
            _context = context;
            _membershipPackageService = membershipPackageService;
        }

        public string CreatePaymentUrl(int userId, VnPayPaymentRequest request, string ipAddress)
        {
            // Lấy thông tin package
            var package = _context.MembershipPackages
                .FirstOrDefault(p => p.PackageId == request.PackageId && !p.IsDeleted);

            if (package == null)
            {
                throw new ArgumentException("Package not found");
            }

            // Lấy config VNPay
            string vnp_Url = _configuration["VnPay:Url"] ?? throw new InvalidOperationException("VnPay:Url not configured");
            string vnp_TmnCode = _configuration["VnPay:TmnCode"] ?? throw new InvalidOperationException("VnPay:TmnCode not configured");
            string vnp_HashSecret = _configuration["VnPay:HashSecret"] ?? throw new InvalidOperationException("VnPay:HashSecret not configured");
            string vnp_ReturnUrl = _configuration["VnPay:ReturnUrl"] ?? throw new InvalidOperationException("VnPay:ReturnUrl not configured");

            // Tạo orderId (dùng timestamp + userId + packageId)
            string orderId = $"{DateTime.Now.Ticks}_{userId}_{request.PackageId}";

            // Lưu transaction vào DB với status Pending
            var paymentTransaction = new PaymentTransaction
            {
                OrderId = orderId,
                UserId = userId,
                PackageId = request.PackageId,
                Amount = package.Price,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.PaymentTransactions.Add(paymentTransaction);
            _context.SaveChanges();

            // Tạo VnPay request
            VnPayLibrary vnpay = new VnPayLibrary();
            
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            
            // Số tiền nhân 100 (VNPay yêu cầu)
            long amount = (long)(package.Price * 100);
            vnpay.AddRequestData("vnp_Amount", amount.ToString());
            
            // Bank code (nếu có)
            if (!string.IsNullOrEmpty(request.BankCode))
            {
                vnpay.AddRequestData("vnp_BankCode", request.BankCode);
            }

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", request.Locale);
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan goi {package.PackageName} - UserId: {userId}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId);

            // Tạo payment URL
            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return paymentUrl;
        }

        public async Task<VnPayReturnResponse> ProcessPaymentReturn(IQueryCollection queryParams)
        {
            string vnp_HashSecret = _configuration["VnPay:HashSecret"] ?? throw new InvalidOperationException("VnPay:HashSecret not configured");

            VnPayLibrary vnpay = new VnPayLibrary();

            // Lấy tất cả query params
            foreach (var param in queryParams)
            {
                if (!string.IsNullOrEmpty(param.Key) && param.Key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(param.Key, param.Value.ToString());
                }
            }

            // Lấy thông tin từ response
            string orderId = vnpay.GetResponseData("vnp_TxnRef");
            long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            string vnp_SecureHash = queryParams["vnp_SecureHash"].ToString();
            long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
            string? bankCode = queryParams["vnp_BankCode"].ToString();

            var response = new VnPayReturnResponse
            {
                OrderId = 0,
                VnPayTransactionId = vnpayTranId,
                ResponseCode = vnp_ResponseCode,
                TransactionStatus = vnp_TransactionStatus,
                Amount = vnp_Amount,
                BankCode = bankCode,
                PaymentDate = DateTime.Now
            };

            // Validate signature
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
            if (!checkSignature)
            {
                response.Success = false;
                response.Message = "Chữ ký không hợp lệ";
                return response;
            }

            // Parse orderId để lấy userId và packageId
            // Format: {timestamp}_{userId}_{packageId}
            var orderParts = orderId.Split('_');
            if (orderParts.Length != 3)
            {
                response.Success = false;
                response.Message = "Mã đơn hàng không hợp lệ";
                return response;
            }

            int userId = int.Parse(orderParts[1]);
            int packageId = int.Parse(orderParts[2]);

            var transaction = await _context.PaymentTransactions
                .FirstOrDefaultAsync(t => t.OrderId == orderId && !t.IsDeleted);

            if (transaction == null)
            {
                response.Success = false;
                response.Message = "Không tìm thấy giao dịch";
                return response;
            }

            if (transaction.Status == PaymentStatus.Completed)
            {
                response.Success = true;
                response.Message = "Giao dịch đã được xử lý trước đó";
                
                var existingSubscription = await _context.UserMembershipSubscriptions
                    .Include(s => s.User)
                    .Include(s => s.Package)
                    .Where(s => s.UserId == userId && s.PackageId == packageId)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();

                if (existingSubscription != null)
                {
                    response.Subscription = new UserMembershipSubscriptionResponse
                    {
                        SubscriptionId = existingSubscription.SubscriptionId,
                        UserId = existingSubscription.UserId,
                        Username = existingSubscription.User?.Username ?? "",
                        PackageId = existingSubscription.PackageId,
                        PackageName = existingSubscription.Package?.PackageName ?? "",
                        StartDate = existingSubscription.StartDate,
                        EndDate = existingSubscription.EndDate,
                        Status = existingSubscription.Status.ToString(),
                        RemainingGenerations = existingSubscription.RemainingGenerations,
                        TotalGenerationsUsed = existingSubscription.TotalGenerationsUsed,
                        CreatedAt = existingSubscription.CreatedAt
                    };
                }
                
                return response;
            }

            // Kiểm tra trạng thái thanh toán
            if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
            {
                // Thanh toán thành công - tạo subscription
                try
                {
                    var subscription = await _membershipPackageService.PurchasePackageAsync(userId, packageId);
                    
                    transaction.Status = PaymentStatus.Completed;
                    transaction.VnPayTransactionId = vnpayTranId.ToString();
                    transaction.VnPayResponseCode = vnp_ResponseCode;
                    transaction.CompletedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    
                    response.Success = true;
                    response.Message = "Thanh toán thành công";
                    response.Subscription = subscription;
                }
                catch (Exception ex)
                {
                    transaction.Status = PaymentStatus.Failed;
                    transaction.VnPayResponseCode = vnp_ResponseCode;
                    transaction.CompletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    response.Success = false;
                    response.Message = $"Lỗi khi tạo subscription: {ex.Message}";
                }
            }
            else
            {
                // Thanh toán thất bại - cập nhật transaction
                transaction.Status = vnp_ResponseCode == "24" ? PaymentStatus.Cancelled : PaymentStatus.Failed;
                transaction.VnPayResponseCode = vnp_ResponseCode;
                transaction.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                response.Success = false;
                response.Message = $"Thanh toán thất bại - Mã lỗi: {vnp_ResponseCode}";
            }

            return response;
        }
    }
}
