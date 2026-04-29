using MCLS.Data;
using MCLS.Dto;
using MCLS.Generics;
using MCLS.IServices;
using MCLS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MCLS.Services
{
    public class GeneralService(AppDbContext _context) : IGeneralService
    {
        public async Task<ServiceResponse<DashboardSummaryDto>> DashboardSummary(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                IQueryable<VoyageLog> query = _context.VoyageLogs.Include(v => v.Vessel);

                if (user.IsInRole("Captain"))
                {
                    var vesselIdClaim = user.FindFirstValue("VesselId");
                    if (string.IsNullOrEmpty(vesselIdClaim))
                    {
                        return ServiceResponse<DashboardSummaryDto>.Failure("No vessel assigned to this user", null, 404);
                    }
                    var vesselId = Guid.Parse(vesselIdClaim);
                    query = query.Where(l => l.VesselId == vesselId);
                }

                var summary = new DashboardSummaryDto
                {
                    TotalVoyages = await query.CountAsync(),
                    TotalFuelConsumed = await query.AnyAsync() ? await query.SumAsync(l => l.FuelConsumed) : 0,
                    AverageSpeed = await query.AnyAsync() ? await query.AverageAsync(l => l.SpeedInKiloMeter) : 0,

                    RecentLogs = await query
                        .OrderByDescending(l => l.createdAt)
                        .Take(5)
                        .Select(l => new RecentLogDto
                        {
                            Id = l.Id,
                            VesselName = l.Vessel.Name,
                            DateDisplay = l.createdAt.ToString("dd MMM yyyy"),
                            Status = "Submitted",
                            CurrentSpeed = l.SpeedInKiloMeter
                        }).ToListAsync()
                };
                return ServiceResponse<DashboardSummaryDto>.Success(null, summary, 200);
            }
            catch (Exception)
            {
                ServiceResponse<DashboardSummaryDto>.Success("Internal Server error", null, 500);
                throw;
            }
        }
    }
}
