using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext;

/// <summary>
/// Database context for Kartverket Obstacle Reporting System.
/// Uses ASP.NET Core Identity for user management.
/// </summary>
public class ApplicationContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

    // DbSets for our custom tables
    public DbSet<ObstacleTypeEntity> ObstacleTypes => Set<ObstacleTypeEntity>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // IMPORTANT: Call base method for Identity tables
        base.OnModelCreating(b);

        // ===== ObstacleType Configuration =====
        b.Entity<ObstacleTypeEntity>(e =>
        {
            e.HasKey(x => x.ObstacleId);
            e.Property(x => x.ObstacleId).HasMaxLength(3).IsRequired();
            e.Property(x => x.ObstacleName).HasMaxLength(30).IsRequired();
        });

        // ===== Report Configuration =====
        b.Entity<Report>(e =>
        {
            e.HasKey(x => x.ReportId);
            e.Property(x => x.ReportId).HasColumnType("char(10)");
            e.Property(x => x.UserId).IsRequired(); // FK to AspNetUsers
            e.Property(x => x.Latitude).HasColumnType("decimal(11,9)");
            e.Property(x => x.Longitude).HasColumnType("decimal(12,9)");
            e.Property(x => x.AltitudeFeet).HasColumnType("smallint");
            e.Property(x => x.ObstacleId).HasMaxLength(3).IsRequired();
            e.Property(x => x.Description).HasColumnType("text").IsRequired();
            e.Property(x => x.DateTime).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();

            // Relation: Report -> ApplicationUser (AspNetUsers)
            e.HasOne(x => x.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation: Report -> ObstacleType
            e.HasOne(x => x.ObstacleType)
                .WithMany(o => o.Reports)
                .HasForeignKey(x => x.ObstacleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Seed ObstacleTypes =====
        b.Entity<ObstacleTypeEntity>().HasData(
            new ObstacleTypeEntity { ObstacleId = "CRN", ObstacleName = "Crane", SortedOrder = 1 },
            new ObstacleTypeEntity { ObstacleId = "MST", ObstacleName = "Mast", SortedOrder = 2 },
            new ObstacleTypeEntity { ObstacleId = "PWR", ObstacleName = "PowerLine", SortedOrder = 3 },
            new ObstacleTypeEntity { ObstacleId = "TWR", ObstacleName = "Tower", SortedOrder = 4 },
            new ObstacleTypeEntity { ObstacleId = "BLD", ObstacleName = "Building", SortedOrder = 5 },
            new ObstacleTypeEntity { ObstacleId = "OTH", ObstacleName = "Other", SortedOrder = 9 }
        );
    }
}