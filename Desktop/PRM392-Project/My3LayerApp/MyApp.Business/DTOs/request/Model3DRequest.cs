using System.ComponentModel.DataAnnotations;

namespace MyApp.Business.DTOs.request
{
    public class Model3DRequest
    {
        [Required(ErrorMessage = "FilePath is required")]
        [StringLength(500, ErrorMessage = "FilePath cannot exceed 500 characters")]
        public string FilePath { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }
    }
}
