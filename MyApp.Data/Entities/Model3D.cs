using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Data.Entities
{
    public class Model3D
    {
        public int ModelId { get; set; } // PK
        public string FilePath { get; set; } = null!;
        public DateTime CreationDate { get; set; }
        public string? Status { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // FK
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // history
        public ICollection<GenerationHistory> GenerationHistories { get; set; } = new List<GenerationHistory>();
    }
}

