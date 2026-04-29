using MCLS.Dto;
using MCLS.Generics;
using System.Security.Claims;

namespace MCLS.IServices
{
    public interface IGeneralService
    {
        Task<ServiceResponse<DashboardSummaryDto>> DashboardSummary(ClaimsPrincipal user);
    }
}
