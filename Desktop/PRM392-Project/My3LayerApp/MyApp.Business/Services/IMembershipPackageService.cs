using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;


namespace MyApp.Business.Services
{
    public interface IMembershipPackageService
    {
        Task<IEnumerable<MembershipPackageResponse>> GetAllAsync();
        Task<MembershipPackageResponse?> GetByIdAsync(int id);
        Task<MembershipPackageResponse> CreateAsync(MembershipPackageRequest request);
        Task<MembershipPackageResponse> UpdateAsync(int id, MembershipPackageUpdateRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
