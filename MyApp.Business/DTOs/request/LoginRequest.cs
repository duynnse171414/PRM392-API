using System.ComponentModel.DataAnnotations;

namespace MyApp.Business.DTOs.request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is not blank")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is not blank")]
        public string Password { get; set; } = null!;
    }
}
