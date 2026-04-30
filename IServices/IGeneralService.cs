using MCLS.Dto;
using MCLS.Generics;
using System.Security.Claims;

namespace MCLS.IServices
{
    public interface IGeneralService
    {
        Task<ServiceResponse<DashboardSummaryDto>> DashboardSummary(ClaimsPrincipal user);
        Task<ServiceResponse<List<MinimalVesselDto>>> GetVessels(string? search = null);
        Task<ServiceResponse<string>> AddVessel(VesselDto vessel);
        Task<ServiceResponse<VesselDto>> GetVesselById(string id);
    }
}
