using MCLS.Dto;
using MCLS.Generics;
using System.Security.Claims;

namespace MCLS.IServices
{
    public interface IVoyageLogService
    {
        Task<ServiceResponse<string>> CreateVoyageLog(VoyageLogDto voyageLog, ClaimsPrincipal user);
    }
}
