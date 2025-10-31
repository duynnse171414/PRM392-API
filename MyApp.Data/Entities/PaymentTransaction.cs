using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Data.Entities
{
    public class PaymentTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string OrderId { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PackageId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

        [MaxLength(50)]
        public string? VnPayTransactionId { get; set; }

        [MaxLength(10)]
        public string? VnPayResponseCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("PackageId")]
        public virtual MembershipPackage? Package { get; set; }
    }

    public static class PaymentStatus
    {
        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Cancelled = "Cancelled";
    }
}
