using Microsoft.AspNetCore.Http;
using MyApp.Business.DTOs.request;
using MyApp.Business.DTOs.response;

namespace MyApp.Business.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(int userId, VnPayPaymentRequest request, string ipAddress);
        Task<VnPayReturnResponse> ProcessPaymentReturn(IQueryCollection queryParams);
    }
}
