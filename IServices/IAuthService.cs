using MCLS.Dto;
using MCLS.Generics;

namespace MCLS.IServices
{
    public interface IAuthService
    {
        Task<ServiceResponse<List<string>>> Register(RegisterDto data);
        Task<ServiceResponse<string>> Login(RegisterDto data);
    }
}
