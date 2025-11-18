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
        public DbSet<Notification> Notifications => Set<Notification>();

        // ========== DbSets for our custom tables ==========

        // Existing tables
        public DbSet<ObstacleTypeEntity> ObstacleTypes => Set<ObstacleTypeEntity>();
        public DbSet<Report> Reports => Set<Report>();

        // NEW: Organizations + join table
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();

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
                e.Property(x => x.HeightFeet).HasColumnType("smallint");
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

            // ===== Organization Configuration =====
            b.Entity<Organization>(e =>
            {
                e.HasKey(x => x.OrganizationId);
                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                e.Property(x => x.ShortCode)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            // ===== OrganizationUser (join table) =====
            b.Entity<OrganizationUser>(e =>
            {
                // Composite primary key: (OrganizationId, UserId)
                e.HasKey(x => new { x.OrganizationId, x.UserId });

                e.HasOne(x => x.Organization)
                    .WithMany(o => o.Members)
                    .HasForeignKey(x => x.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
                    .WithMany(u => u.Organizations)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // ===== Notification Configuration =====
            b.Entity<Notification>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(x => x.Message)
                    .IsRequired()
                    .HasMaxLength(500);

                e.Property(x => x.CreatedAt)
                    .IsRequired();

                e.HasOne(x => x.User)
                    .WithMany() // evt. lag en ICollection<Notification> på ApplicationUser hvis du vil
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Report)
                    .WithMany()
                    .HasForeignKey(x => x.ReportId)
                    .OnDelete(DeleteBehavior.SetNull);
            });


            // ===== Seed ObstacleTypes =====
            b.Entity<ObstacleTypeEntity>().HasData(
                new ObstacleTypeEntity { ObstacleId = "CRN", ObstacleName = "Crane",     SortedOrder = 1 },
                new ObstacleTypeEntity { ObstacleId = "MST", ObstacleName = "Mast",      SortedOrder = 2 },
                new ObstacleTypeEntity { ObstacleId = "PWR", ObstacleName = "PowerLine", SortedOrder = 3 },
                new ObstacleTypeEntity { ObstacleId = "TWR", ObstacleName = "Tower",     SortedOrder = 4 },
                new ObstacleTypeEntity { ObstacleId = "BLD", ObstacleName = "Building",  SortedOrder = 5 },
                new ObstacleTypeEntity { ObstacleId = "OTH", ObstacleName = "Other",     SortedOrder = 9 }
            );

            // ===== Seed Organizations (optional defaults) =====
            b.Entity<Organization>().HasData(
                new Organization { OrganizationId = 1, Name = "Norsk Luftambulanse",         ShortCode = "NLA" },
                new Organization { OrganizationId = 2, Name = "Luftforsvaret",               ShortCode = "LFS" },
                new Organization { OrganizationId = 3, Name = "Kartverket", ShortCode = "KRT" }
            );
        }
    }
