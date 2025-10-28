using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Business.DTOs.request
{
    public class MembershipPackageRequest
    {
        [Required(ErrorMessage = "Package name is required")]
        [StringLength(100, ErrorMessage = "Package name cannot exceed 100 characters")]
        public string PackageName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Duration days is required")]
        [Range(1, 3650, ErrorMessage = "Duration must be between 1 and 3650 days")]
        public int DurationDays { get; set; }
    }
}
