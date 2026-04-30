namespace MCLS.Models
{
    public class VoyageLog
    {
        public Guid Id { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public double FuelConsumed { get; set; }
        public double SpeedInKiloMeter { get; set; }
        public double DistanceSailed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;



        public Guid VesselId { get; set; }
        public string? UserId { get; set; }

        public Vessel? Vessel { get; set; }
        public User? User { get; set; }

    }
}
