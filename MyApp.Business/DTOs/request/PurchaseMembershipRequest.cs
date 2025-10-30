using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Business.DTOs.request
{
    public class PurchaseMembershipRequest
    {
        [Required(ErrorMessage = "Package ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid package ID")]
        public int PackageId { get; set; }
    }
}
