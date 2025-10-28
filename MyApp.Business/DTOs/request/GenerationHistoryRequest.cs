using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Business.DTOs.request
{
    public class GenerationHistoryRequest
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "ModelId is required")]
        public int ModelId { get; set; }

        public string? InputImages { get; set; }
    }

    public class GenerationHistoryUpdateRequest
    {
        public int? ModelId { get; set; }
        public string? InputImages { get; set; }
    }
}
