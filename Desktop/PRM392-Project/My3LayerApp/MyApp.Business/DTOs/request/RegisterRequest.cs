using System.ComponentModel.DataAnnotations;
using MyApp.Data.Entities;

namespace MyApp.Business.DTOs.request
{
    // DTO cho Register
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is not blank")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3-50 characters")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is not blank")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be 6-10 characters")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Email is not blank")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; } = null!;

     


    }
}
