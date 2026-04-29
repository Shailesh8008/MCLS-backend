using MCLS.Dto;
using MCLS.Generics;
using MCLS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCLS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        [Authorize(Roles = "Admin")]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto data)
        {
            try
            {
                var result = await authService.Register(data);
                if (!result.Ok)
                {
                    return StatusCode(result.Status, ControllerResponse<List<string>>.Failure(result.Message, result.Data));
                }
                return StatusCode(result.Status, ControllerResponse<List<string>>.Success(result.Message, result.Data));
            }
            catch (Exception)
            {
                return StatusCode(500, ControllerResponse<string>.Failure("Internal server error", null));
                throw;
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] RegisterDto data)
        {
            try
            {
                var result = await authService.Login(data);
                if (!result.Ok)
                {
                    return StatusCode(result.Status, ControllerResponse<string>.Failure(result.Message, result.Data));
                }
                return StatusCode(result.Status, ControllerResponse<string>.Success(result.Message, result.Data));
            }
            catch (Exception)
            {
                return StatusCode(500, ControllerResponse<string>.Failure("Internal server error", null));
                throw;
            }
        }

        [Authorize(Roles = "Admin,Captain")]
        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> DashboardSummary()
        {

        }
    }
}
