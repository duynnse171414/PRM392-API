using System.ComponentModel.DataAnnotations;

namespace MyApp.Business.DTOs.request
{
    public class VnPayPaymentRequest
    {
        [Required]
        public int PackageId { get; set; }
        
        public string? BankCode { get; set; } // VNPAYQR, VNBANK, INTCARD
        
        public string Locale { get; set; } = "vn"; // vn hoặc en
    }
}
