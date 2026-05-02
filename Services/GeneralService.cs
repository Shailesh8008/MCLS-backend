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
                        .OrderByDescending(l => l.CreatedAt)
                        .Take(5)
                        .Select(l => new RecentLogDto
                        {
                            Id = l.Id,
                            VesselName = l.Vessel.Name,
                            LogDate = l.CreatedAt,
                            SpeedInKiloMeter = l.SpeedInKiloMeter,
                            ReportedBy = l.User.Name
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

        public async Task<ServiceResponse<List<MinimalVesselDto>>> GetVessels(string? search = null)
        {
            try
            {
                var query = _context.Vessels.AsQueryable();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(v => v.Name.Contains(search) || v.IMONumber.Contains(search));
                }

                var vessels = await query.Select(v => new MinimalVesselDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    IMONumber = v.IMONumber
                })
                .OrderBy(v => v.Name)
                .ToListAsync();
                return ServiceResponse<List<MinimalVesselDto>>.Success(null, vessels, 200);
            }
            catch (Exception)
            {
                return ServiceResponse<List<MinimalVesselDto>>.Failure("Internal server error", null, 500);
                throw;
            }
        }

        public async Task<ServiceResponse<string>> AddVessel(VesselDto vessel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vessel.Name) ||
                    string.IsNullOrWhiteSpace(vessel.IMONumber) ||
                    string.IsNullOrWhiteSpace(vessel.Type) ||
                    string.IsNullOrWhiteSpace(vessel.Flag))
                {
                    return ServiceResponse<string>.Failure("Name, IMONumber, Type, and Flag are required.", null, 400);
                }
                var newVessel = new Vessel
                {
                    Id = Guid.NewGuid(),
                    Name = vessel.Name,
                    IMONumber = vessel.IMONumber,
                    Type = vessel.Type,
                    Flag = vessel.Flag
                };
                var result = await _context.Vessels.AddAsync(newVessel);
                await _context.SaveChangesAsync();
                return ServiceResponse<string>.Success("Vessel Added successfully", null, 201);
            }
            catch (Exception)
            {
                return ServiceResponse<string>.Failure("Internal server error", null, 500);
                throw;
            }
        }

        public async Task<ServiceResponse<VesselDto>> GetVesselById(string id)
        {
            try
            {
                Guid vesselId;
                if (!Guid.TryParse(id, out vesselId))
                {
                    return ServiceResponse<VesselDto>.Failure("Invalid vessel id", null, 400);
                }

                var vesselDetails = await _context.Vessels
                    .Include(v => v.Users)
                    .Include(v => v.VoyageLogs)
                    .Where(v => v.Id == vesselId)
                    .Select(v => new VesselDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        IMONumber = v.IMONumber,
                        Type = v.Type,
                        Flag = v.Flag,
                        AssignedStaff = v.Users.Select(u => new MinimalUserDto
                        {
                            Name = u.Name,
                            Email = u.Email,
                            Rank = u.Rank
                        }).ToList(),
                        History = v.VoyageLogs
                            .OrderByDescending(l => l.CreatedAt)
                            .Take(10)
                            .Select(l => new RecentLogDto
                            {
                                Id = l.Id,
                                LogDate = l.CreatedAt,
                                ReportedBy = l.User.Name,
                                SpeedInKiloMeter = l.SpeedInKiloMeter,
                                VesselName = l.Vessel.Name
                            }).ToList(),

                        TotalLogsSubmitted = v.VoyageLogs.Count(),
                        LastReportedPosition = v.VoyageLogs.Select(l => l.Latitude + ", " + l.Longitude).FirstOrDefault() ?? "Position not reported"
                    }).FirstOrDefaultAsync();



                return ServiceResponse<VesselDto>.Success(null, vesselDetails, 200);
            }
            catch (Exception)
            {
                return ServiceResponse<VesselDto>.Failure("Internal server error", null, 500);
                throw;
            }
        }


    }
}
