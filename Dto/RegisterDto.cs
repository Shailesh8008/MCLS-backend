namespace MCLS.Dto
{
    public class RegisterDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? VesselId { get; set; }
        public required string Rank { get; set; }

    }
}
