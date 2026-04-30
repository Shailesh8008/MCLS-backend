namespace MCLS.Dto
{
    public class RecentLogDto
    {
        public Guid Id { get; set; }
        public DateTime LogDate { get; set; }
        public string? ReportedBy { get; set; }
        public string? VesselName { get; set; }
        public double? SpeedInKiloMeter { get; set; }

    }
}
