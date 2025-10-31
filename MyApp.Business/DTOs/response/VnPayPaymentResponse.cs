namespace MyApp.Business.DTOs.response
{
    public class VnPayPaymentResponse
    {
        public bool Success { get; set; }
        public string? PaymentUrl { get; set; }
        public string? Message { get; set; }
        public string? OrderId { get; set; }
    }

    public class VnPayReturnResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long OrderId { get; set; }
        public long VnPayTransactionId { get; set; }
        public string ResponseCode { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? BankCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public UserMembershipSubscriptionResponse? Subscription { get; set; }
    }
}
