using Microsoft.AspNetCore.Identity;

namespace MCLS.Models
{
    public class User : IdentityUser
    {
        public string? Name { get; set; }
        public string? Rank { get; set; }
        public ICollection<VoyageLog>? VoyageLogs { get; set; }

    }
}
