using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Data.Entities
{
    public class GenerationHistory
    {
        public int HistoryId { get; set; } // PK
        public int UserId { get; set; } // FK
        public User User { get; set; } = null!;
        public int ModelId { get; set; } // FK
        public Model3D Model3D { get; set; } = null!;
        public DateTime GenerationDate { get; set; }
        public string? InputImages { get; set; } // maybe JSON or CSV list of image paths
    }
}
