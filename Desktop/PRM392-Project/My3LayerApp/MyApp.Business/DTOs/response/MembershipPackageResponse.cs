using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Business.DTOs.response
{
    public class MembershipPackageResponse
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public int SubscriberCount { get; set; } // Số lượng người đang đăng ký gói này
    }
}
