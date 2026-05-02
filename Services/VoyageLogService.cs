using MCLS.Data;
using MCLS.Dto;
using MCLS.Generics;
using MCLS.IServices;
using MCLS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace MCLS.Services
{
    public class VoyageLogService(AppDbContext _context, IConverter _converter) : IVoyageLogService
    {
        public async Task<ServiceResponse<string>> CreateVoyageLog(VoyageLogDto voyageLog, ClaimsPrincipal user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(voyageLog.Latitude) ||
                    string.IsNullOrWhiteSpace(voyageLog.Longitude))

                {
                    return ServiceResponse<string>.Failure("Latitute, and Longitude are required", null, 400);
                }

                if (voyageLog.FuelConsumed <= 0 ||
                    voyageLog.DistanceSailed <= 0 ||
                    voyageLog.SpeedInKiloMeter <= 0)
                    return ServiceResponse<string>.Failure("FuelConsumed, DistanceSailed, and SpeedInKiloMeter are require and should be positive numbers", null, 400);

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid vesselId;
                if (!Guid.TryParse(user.FindFirstValue("VesselId"), out vesselId))
                {
                    return ServiceResponse<string>.Failure("No vessel assigned or Invalid vesselId assigned to you, please contact admin", null, 400);
                }

                var fuelEffeciency = voyageLog.FuelConsumed / voyageLog.DistanceSailed;
                var prevVoyageLog = await _context.VoyageLogs.OrderByDescending(v => v.CreatedAt).FirstOrDefaultAsync(x => x.VesselId == vesselId);
                double rob = voyageLog.Bunkering;
                if (prevVoyageLog != null)
                {
                    rob += prevVoyageLog.ROB - voyageLog.FuelConsumed;
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
                    FuelEfficiency = fuelEffeciency,
                    ROB = rob < 0 ? 0 : rob,
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

        public async Task<ServiceResponse<byte[]>> RecentReportPdf(string vid)
        {
            try
            {
                Guid vesselId;
                if (!Guid.TryParse(vid, out vesselId))
                {
                    return ServiceResponse<byte[]>.Failure("Invalid vesselId", null, 400);
                }
                var vessel = await _context.Vessels.Include(v => v.VoyageLogs.OrderByDescending(l => l.CreatedAt).Take(2)).FirstOrDefaultAsync(v => v.Id == vesselId);
                if (vessel == null)
                {
                    return ServiceResponse<byte[]>.Failure("Vessel not found", null, 404);
                }
                var latestLog = vessel.VoyageLogs?.FirstOrDefault();
                if (latestLog == null)
                {
                    return ServiceResponse<byte[]>.Failure("No voyage logs found for this vessel", null, 404);
                }

                var htmlContent = $@"<html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <title>Vessel Noon Report</title>
                        <style>
                            body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 800px; margin: 20px auto; padding: 20px; border: 1px solid #eee; }}
                            .header {{ text-align: center; border-bottom: 2px solid #1a365d; padding-bottom: 10px; margin-bottom: 20px; }}
                            .header h1 {{ margin: 0; color: #1a365d; text-transform: uppercase; }}
        
                            .section {{ margin-bottom: 25px; }}
                            .section-title {{ background: #f4f7f9; padding: 8px 12px; font-weight: bold; border-left: 4px solid #1a365d; margin-bottom: 15px; }}
        
                            .grid-container {{ display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px; }}
                            .field {{ display: flex; justify-content: space-between; border-bottom: 1px solid #f0f0f0; padding: 5px 0; }}
                            .label {{ color: #666; font-size: 0.9em; }}
                            .value {{ font-weight: 600; }}
        
                            .footer {{ margin-top: 10px; font-size: 0.8em; text-align: center; color: #888; border-top: 1px solid #eee; padding-top: 10px; }}
                        </style>
                    </head>
                    <body>

                        <div class=""header"">
                            <h1>Noon Report</h1>
                            <p>Operational Status Update</p>
                        </div>

                        <!-- Vessel Identity -->
                        <div class=""section"">
                            <div class=""section-title"">Vessel Particulars</div>
                            <div class=""grid-container"">
                                <div class=""field""><span class=""label"">Vessel Name:</span> <span class=""value"">{vessel.Name}</span></div>
                                <div class=""field""><span class=""label"">IMO Number:</span> <span class=""value"">{vessel.IMONumber}</span></div>
                                <div class=""field""><span class=""label"">Flag:</span> <span class=""value"">{vessel.Flag}</span></div>
                                <div class=""field""><span class=""label"">Report Date:</span> <span class=""value"">{latestLog.CreatedAt.ToString("dd MMM yyyy")}</span></div>
                            </div>
                        </div>

                        <!-- Position Data -->
                        <div class=""section"">
                            <div class=""section-title"">Position & Voyage Details</div>
                            <div class=""grid-container"">
                                <div class=""field""><span class=""label"">Latitude:</span> <span class=""value"">{latestLog.Latitude}</span></div>
                                <div class=""field""><span class=""label"">Longitude:</span> <span class=""value"">{latestLog.Longitude}</span></div>
                                <div class=""field""><span class=""label"">Distance Sailed:</span> <span class=""value"">{latestLog.DistanceSailed} km</span></div>
                                <div class=""field""><span class=""label"">Avg Speed:</span> <span class=""value"">{latestLog.SpeedInKiloMeter} km/h</span></div>
                            </div>
                        </div>

                        <!-- Consumption Data -->
                        <div class=""section"">
                            <div class=""section-title"">Consumption & Efficiency</div>
                            <div class=""grid-container"">
                                <div class=""field""><span class=""label"">Fuel Consumed:</span> <span class=""value"">{latestLog.FuelConsumed} MT</span></div>
                                <div class=""field""><span class=""label"">Fuel Efficiency:</span> <span class=""value"">{latestLog.FuelEfficiency} MT/km</span></div>
                                <div class=""field""><span class=""label"">ROB (Remaining on Board):</span> <span class=""value"">{latestLog.ROB} MT</span></div>
                                <div class=""field""><span class=""label"">Reported By:</span> <span class=""value"">Captain {latestLog.User.Name}</span></div>
                            </div>
                        </div>

                        <div class=""footer"">
                            Generated by Maritime Logistics System | Vessel ID: {vesselId} | Log ID: {latestLog.Id.ToString()}
                        </div>

                    </body>
                    </html>";
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                    },
                    Objects = {
                        new ObjectSettings() {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8", Background = true },
                        HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                        FooterSettings = { FontName = "Arial", FontSize = 9, Center = "Maritime Logistics System" }
                        }
                    }
                };
                var data = _converter.Convert(doc);
                return ServiceResponse<byte[]>.Success("bytes generated successfully", data, 200);

            }
            catch (Exception)
            {
                return ServiceResponse<byte[]>.Failure("Internal server error", null, 500);
                throw;
            }
        }

        public async Task<ServiceResponse<byte[]>> SpecificReportPdf(string logId)
        {
            try
            {
                Guid validLogId;
                if (!Guid.TryParse(logId, out validLogId))
                {
                    return ServiceResponse<byte[]>.Failure("Invalid Voyage Log id", null, 400);
                }

                var voyageLog = await _context.VoyageLogs.FirstOrDefaultAsync(l => l.Id == validLogId);
                if (voyageLog == null)
                {
                    return ServiceResponse<byte[]>.Failure("Voyage Log not found", null, 404);
                }

                var htmlContent = $@"<html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <title>Vessel Noon Report</title>
                        <style>
                            body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 800px; margin: 20px auto; padding: 20px; border: 1px solid #eee; }}
                            .header {{ text-align: center; border-bottom: 2px solid #1a365d; padding-bottom: 10px; margin-bottom: 20px; }}
                            .header h1 {{ margin: 0; color: #1a365d; text-transform: uppercase; }}
        
                            .section {{ margin-bottom: 25px; }}
                            .section-title {{ background: #f4f7f9; padding: 8px 12px; font-weight: bold; border-left: 4px solid #1a365d; margin-bottom: 15px; }}
        
                            .grid-container {{ display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px; }}
                            .field {{ display: flex; justify-content: space-between; border-bottom: 1px solid #f0f0f0; padding: 5px 0; }}
                            .label {{ color: #666; font-size: 0.9em; }}
                            .value {{ font-weight: 600; }}
        
                            .footer {{ margin-top: 10px; font-size: 0.8em; text-align: center; color: #888; border-top: 1px solid #eee; padding-top: 10px; }}
                        </style>
                    </head>
                    <body>

                        <div class=""header"">
                            <h1>Noon Report</h1>
                            <p>Operational Status Update</p>
                        </div>

                        <!-- Vessel Identity -->
                        <div class=""section"">
                            <div class=""section-title"">Vessel Particulars</div>
                            <div class=""grid-container"">
                                <div class=""field""><span class=""label"">Vessel Name:</span> <span class=""value"">{voyageLog.Vessel.Name}</span></div>
                                <div class=""field""><span class=""label"">IMO Number:</span> <span class=""value"">{voyageLog.Vessel.IMONumber}</span></div>
                                <div class=""field""><span class=""label"">Flag:</span> <span class=""value"">{voyageLog.Vessel.Flag}</span></div>
                                <div class=""field""><span class=""label"">Report Date:</span> <span class=""value"">{voyageLog.CreatedAt.ToString("dd MMM yyyy")}</span></div>
                            </div>
                        </div>

                        <!-- Position Data -->
                        <div class=""section"">
                            <div class=""section-title"">Position & Voyage Details</div>
                            <div class=""grid-container"">
                                <div class=""field""><span class=""label"">Latitude:</span> <span class=""value"">{voyageLog.Latitude}</span></div>
                                <div class=""field""><span class=""label"">Longitude:</span> <span class=""value"">{voyageLog.Longitude}</span></div>
                                <div class=""field""><span class=""label"">Distance Sailed:</span> <span class=""value"">{voyageLog.DistanceSailed} km</span></div>
                                <div class=""field""><span class=""label"">Avg Speed:</span> <span class=""value"">{voyageLog.SpeedInKiloMeter} km/h</span></div>
                            </div>
                        </div>

                        <!-- Consumption Data -->
                        <div class=""section"">
                            <div class=""section-title"">Consumption & Efficiency</div>
                            <div class=""grid-container"">
                                <div class=""field""><span class=""label"">Fuel Consumed:</span> <span class=""value"">{voyageLog.FuelConsumed} MT</span></div>
                                <div class=""field""><span class=""label"">Fuel Efficiency:</span> <span class=""value"">{voyageLog.FuelEfficiency} MT/km</span></div>
                                <div class=""field""><span class=""label"">ROB (Remaining on Board):</span> <span class=""value"">{voyageLog.ROB} MT</span></div>
                                <div class=""field""><span class=""label"">Reported By:</span> <span class=""value"">Captain {voyageLog.User.Name}</span></div>
                            </div>
                        </div>

                        <div class=""footer"">
                            Generated by Maritime Logistics System | Vessel ID: {voyageLog.Vessel.Id} | Log ID: {voyageLog.Id.ToString()}
                        </div>

                    </body>
                    </html>";
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                    },
                    Objects = {
                        new ObjectSettings() {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8", Background = true },
                        HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                        FooterSettings = { FontName = "Arial", FontSize = 9, Center = "Maritime Logistics System" }
                        }
                    }
                };
                var data = _converter.Convert(doc);
                return ServiceResponse<byte[]>.Success("bytes generated successfully", data, 200);
            }
            catch (Exception)
            {
                return ServiceResponse<byte[]>.Failure("Internal Server error", null, 500);
                throw;
            }
        }
    }
}
