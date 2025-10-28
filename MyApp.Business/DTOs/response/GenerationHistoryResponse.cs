using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Business.DTOs.response
{
    public class GenerationHistoryResponse
    {
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int ModelId { get; set; }
        public string? ModelFilePath { get; set; }
        public DateTime GenerationDate { get; set; }
        public string? InputImages { get; set; }
    }
}
