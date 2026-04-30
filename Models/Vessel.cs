namespace MCLS.Models
{
    public class Vessel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? IMONumber { get; set; }
        public string? Type { get; set; }
        public string? Flag { get; set; }

        public ICollection<VoyageLog>? VoyageLogs { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
