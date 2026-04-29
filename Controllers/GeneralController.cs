using MCLS.Dto;
using MCLS.Generics;
using MCLS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCLS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController(IGeneralService generalService) : ControllerBase
    {
        [Authorize(Roles = "Admin,Captain")]
        [HttpGet("summary")]
        public async Task<IActionResult> DashboardSummary()
        {
            try
            {
                var result = await generalService.DashboardSummary(User);
                if (result.Ok == false)
                {
                    return StatusCode(result.Status, ControllerResponse<string>.Failure(result.Message, null));
                }
                return StatusCode(result.Status, ControllerResponse<DashboardSummaryDto>.Success(result.Message, result.Data));
            }
            catch (Exception)
            {
                StatusCode(500, ControllerResponse<string>.Failure("Internal server error", null));
                throw;
            }
        }
    }
}
