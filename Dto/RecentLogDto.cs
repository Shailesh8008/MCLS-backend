namespace MCLS.Dto
{
    public class RecentLogDto
    {
        public Guid Id { get; set; }
        public string VesselName { get; set; } = string.Empty;
        public string DateDisplay { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double CurrentSpeed { get; set; }
    }
}
