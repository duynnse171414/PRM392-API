using System.ComponentModel.DataAnnotations;

namespace MyApp.Business.DTOs.response

{
    public class Model3DResponse
    {
        public int ModelId { get; set; }
        public string FilePath { get; set; } = null!;
        public DateTime CreationDate { get; set; }
        public string? Status { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int GenerationHistoryCount { get; set; }
    }
}
