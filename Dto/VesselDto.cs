namespace MCLS.Dto
{
    public class VesselDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IMONumber { get; set; } = string.Empty;
        public string Flag { get; set; } = string.Empty;
        public string? Type { get; set; }
        public int TotalLogsSubmitted { get; set; }
        public List<RecentLogDto> History { get; set; } = new();
        public List<UserDto> AssignedStaff { get; set; } = new();
        public string LastReportedPosition { get; set; } = "Unknown";


    }
}
