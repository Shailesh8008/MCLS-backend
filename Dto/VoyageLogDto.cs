namespace MCLS.Dto
{
    public class VoyageLogDto
    {
        public Guid Id { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public double FuelConsumed { get; set; } = 0;
        public double SpeedInKiloMeter { get; set; } = 0;
        public double DistanceSailed { get; set; } = 0.0;
        public double Bunkering { get; set; } = 0.0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Guid VesselId { get; set; }
        public string? ReportedBy { get; set; }
    }
}
