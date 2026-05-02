using MCLS.Dto;
using MCLS.Generics;
using MCLS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCLS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController(IGeneralService generalService, IVoyageLogService voyageLogService) : ControllerBase
    {
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

        [Authorize(Roles = "Admin")]
        [HttpGet("vessels")]
        public async Task<IActionResult> GetVessels([FromQuery] string? search = null)
        {
            try
            {
                var result = await generalService.GetVessels(search);
                if (!result.Ok)
                {
                    return StatusCode(result.Status, ControllerResponse<string>.Failure(result.Message, null));
                }
                return StatusCode(result.Status, ControllerResponse<List<MinimalVesselDto>>.Success(result.Message, result.Data));
            }
            catch (Exception)
            {
                return StatusCode(500, ControllerResponse<string>.Failure("Internal server error", null));
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("vessel/{vid}")]
        public async Task<IActionResult> GetVesselById([FromRoute] string vid)
        {
            try
            {
                var result = await generalService.GetVesselById(vid);
                if (!result.Ok)
                {
                    return StatusCode(result.Status, ControllerResponse<string>.Failure(result.Message, null));
                }
                return StatusCode(result.Status, ControllerResponse<VesselDto>.Success(result.Message, result.Data));
            }
            catch (Exception)
            {
                return StatusCode(500, ControllerResponse<string>.Failure("Internal server error", null));
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add-vessel")]
        public async Task<IActionResult> AddVessel([FromBody] VesselDto vessel)
        {
            try
            {
                var result = await generalService.AddVessel(vessel);
                if (!result.Ok)
                {
                    return StatusCode(result.Status, ControllerResponse<string>.Failure(result.Message, null));
                }
                return StatusCode(result.Status, ControllerResponse<string>.Success(result.Message, null));
            }
            catch (Exception)
            {
                return StatusCode(500, ControllerResponse<string>.Failure("Internal server error", null));
                throw;
            }
        }

        [Authorize(Roles = "Captain")]
        [HttpPost("create-log")]
        public async Task<IActionResult> CreateLog([FromBody] VoyageLogDto voyageLog)
        {
            try
            {
                var result = await voyageLogService.CreateVoyageLog(voyageLog, User);
                if (!result.Ok)
                {
                    return StatusCode(result.Status, ControllerResponse<string>.Failure(result.Message, null));
                }
                return StatusCode(result.Status, ControllerResponse<string>.Success(result.Message, null));
            }
            catch (Exception)
            {
                return StatusCode(500, ControllerResponse<string>.Failure("Internal server error", null));
                throw;
            }
        }

        [Authorize(Roles = "Captain,Admin")]
        [HttpGet("recent-report-pdf/{vesselId}")]
        public async Task<IActionResult> RecentReportPdf([FromRoute] string vesselId)
        {
            try
            {
                var result = await voyageLogService.RecentReportPdf(vesselId);
                if (!result.Ok)
                {
                    return StatusCode(result.Status, ControllerResponse<string>.Failure(result.Message, null));
                }
                return File(result.Data, "application/pdf", "NoonReport.pdf");
            }
            catch (Exception)
            {
                return StatusCode(500, ControllerResponse<string>.Failure("Internal server error", null));
                throw;
            }
        }

        [Authorize(Roles = "Captain,Admin")]
        [HttpGet("specific-report-pdf/{logId}")]
        public async Task<IActionResult> SpecificReportPdf([FromRoute] string logId)
        {
            try
            {
                var result = await voyageLogService.SpecificReportPdf(logId);
                if (!result.Ok)
                {
                    return StatusCode(result.Status, ControllerResponse<string>.Failure(result.Message, null));
                }
                return File(result.Data, "application/pdf", "NoonReport.pdf");
            }
            catch (Exception)
            {
                return StatusCode(500, ControllerResponse<string>.Failure("Internal server error", null));
                throw;
            }
        }
    }
}
