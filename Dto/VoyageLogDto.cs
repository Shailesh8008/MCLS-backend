namespace MCLS.Dto
{
    public class VoyageLogDto
    {
        public Guid Id { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public double FuelConsumed { get; set; }
        public double SpeedInKiloMeter { get; set; }
        public double DistanceSailed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Guid VesselId { get; set; }
        public string? ReportedBy { get; set; }
    }
}
