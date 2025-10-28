using System.ComponentModel.DataAnnotations;

namespace MyApp.Business.DTOs.request
{
    public class Model3DUpdateRequest
    {
        [StringLength(500, ErrorMessage = "FilePath cannot exceed 500 characters")]
        public string? FilePath { get; set; }

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }

        // Không cho phép update UserId và CreationDate
    }
}
