namespace MCLS.Models
{
    public class VoyageLog
    {
        public Guid Id { get; set; }
        public DateTime LogDate { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public double FuelConsumed { get; set; }
        public double DistanceSailed { get; set; }


        public Guid VesselId { get; set; }
        public string? UserId { get; set; }

        public Vessel? Vessel { get; set; }
        public User? User { get; set; }

    }
}
