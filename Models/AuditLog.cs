namespace MCLS.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string? TableName { get; set; }
        public string? RecordId { get; set; }
        public string? Action { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public string? Changes { get; set; } //json string
    }
}
