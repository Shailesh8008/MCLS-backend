using MCLS.Data;
using MCLS.Dto;
using MCLS.Generics;
using MCLS.IServices;
using MCLS.Models;
using System.Security.Claims;

namespace MCLS.Services
{
    public class VoyageLogService(AppDbContext _context) : IVoyageLogService
    {
        public async Task<ServiceResponse<string>> CreateVoyageLog(VoyageLogDto voyageLog, ClaimsPrincipal user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(voyageLog.Latitude) ||
                    string.IsNullOrWhiteSpace(voyageLog.Longitude) ||
                    double.IsNaN(voyageLog.FuelConsumed) ||
                    double.IsNaN(voyageLog.DistanceSailed) ||
                    double.IsNaN(voyageLog.SpeedInKiloMeter)
                    )
                {
                    return ServiceResponse<string>.Failure("Invalid request body, (latitute, longitude are required and Fuel, Distance, Speed should be valid numbers)", null, 400);
                }

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid vesselId;
                if (!Guid.TryParse(user.FindFirstValue("VesselId"), out vesselId))
                {
                    return ServiceResponse<string>.Failure("No vessel assigned or Invalid vesselId assigned to you, please contact admin", null, 400);
                }

                var newVoyageLog = new VoyageLog
                {
                    Id = Guid.NewGuid(),
                    VesselId = vesselId,
                    UserId = userId,
                    Latitude = voyageLog.Latitude,
                    Longitude = voyageLog.Longitude,
                    SpeedInKiloMeter = voyageLog.SpeedInKiloMeter,
                    FuelConsumed = voyageLog.FuelConsumed,
                    DistanceSailed = voyageLog.DistanceSailed,
                };

                await _context.VoyageLogs.AddAsync(newVoyageLog);
                await _context.SaveChangesAsync();
                return ServiceResponse<string>.Success("Voyage Log created successfully", null, 201);
            }
            catch (Exception)
            {
                return ServiceResponse<string>.Failure("Internal server error", null, 500);
                throw;
            }
        }
    }
}
