namespace MCLS.Dto
{
    public class DashboardSummaryDto
    {
        public int TotalVoyages { get; set; }
        public double TotalFuelConsumed { get; set; }
        public double AverageSpeed { get; set; }

        public string? VesselStatus { get; set; }
        public DateTime? LastLogDate { get; set; }

        public List<RecentLogDto> RecentLogs { get; set; } = new();
    }
}
