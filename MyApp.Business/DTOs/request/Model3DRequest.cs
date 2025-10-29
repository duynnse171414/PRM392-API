using System.ComponentModel.DataAnnotations;

namespace MyApp.Business.DTOs.request
{
    public class Model3DRequest
    {
        [Required(ErrorMessage = "FilePath is required")]
        public string Image { get; set; } = null!;

        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }
    }
}
