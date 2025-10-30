using System.ComponentModel.DataAnnotations;

namespace MyApp.Business.DTOs.request
{
    public class Model3DRequest
    {
        [Required(ErrorMessage = "Image is required")]
        public string Image { get; set; } = null!;
    }
}
