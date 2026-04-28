using MCLS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MCLS.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Vessel> Vessels { get; set; }
        public DbSet<VoyageLog> VoyageLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<VoyageLog>()
                .HasOne(v => v.User)
                .WithMany(u => u.VoyageLogs)
                .HasForeignKey(v => v.UserId);
            builder.Entity<Vessel>()
                .HasMany(v => v.VoyageLogs)
                .WithOne(vl => vl.Vessel)
                .HasForeignKey(vl => vl.VesselId);
        }

    }
}
