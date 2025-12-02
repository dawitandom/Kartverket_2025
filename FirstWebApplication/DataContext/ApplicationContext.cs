    using FirstWebApplication.Models;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    namespace FirstWebApplication.DataContext;

    /// <summary>
    /// Databasekontekst for Kartverket hindringsrapporteringssystem.
    /// Håndterer tilgang til alle databasetabeller i systemet, inkludert brukere, rapporter, organisasjoner og varsler.
    /// Bruker ASP.NET Core Identity for brukerhåndtering og autentisering.
    /// </summary>
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Oppretter en ny instans av databasekonteksten med de angitte innstillingene.
        /// </summary>
        /// <param name="options">Innstillingene for databasekonteksten, inkludert tilkoblingsstreng og databaseleverandør</param>
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        /// <summary>
        /// Tilgang til varseltabellen i databasen. Varsler sendes til brukere når rapporter endrer status eller andre viktige hendelser skjer.
        /// </summary>
        public DbSet<Notification> Notifications => Set<Notification>();

        // ========== DbSets for egendefinerte tabeller ==========

        /// <summary>
        /// Tilgang til hindertypetabellen i databasen. Inneholder de forskjellige typene hindringer som kan rapporteres,
        /// for eksempel kraner, master, kraftledninger og bygninger.
        /// </summary>
        public DbSet<ObstacleTypeEntity> ObstacleTypes => Set<ObstacleTypeEntity>();
        
        /// <summary>
        /// Tilgang til rapportertabellen i databasen. Inneholder alle rapporter som brukere har opprettet,
        /// inkludert posisjon, hindertype, beskrivelse og status.
        /// </summary>
        public DbSet<Report> Reports => Set<Report>();

        /// <summary>
        /// Tilgang til organisasjonstabellen i databasen. Inneholder alle organisasjoner som brukere kan tilhøre,
        /// for eksempel Norsk Luftambulanse, Luftforsvaret og Kartverket.
        /// </summary>
        public DbSet<Organization> Organizations => Set<Organization>();
        
        /// <summary>
        /// Tilgang til koblingstabellen mellom brukere og organisasjoner. Lar brukere tilhøre flere organisasjoner
        /// og organisasjoner ha flere medlemmer.
        /// </summary>
        public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();

        /// <summary>
        /// Konfigurerer hvordan databasemodellen skal bygges opp, inkludert nøkler, relasjoner mellom tabeller,
        /// datatyper og standarddata som skal legges inn når databasen opprettes.
        /// Kaller først base-metoden for å konfigurere Identity-tabellene, deretter konfigureres de egendefinerte tabellene.
        /// </summary>
        /// <param name="b">ModelBuilder som brukes til å konfigurere databasemodellen</param>
        protected override void OnModelCreating(ModelBuilder b)
        {
            // VIKTIG: Kall base-metoden for Identity-tabeller
            base.OnModelCreating(b);

            // ===== Konfigurasjon av ObstacleType =====
            b.Entity<ObstacleTypeEntity>(e =>
            {
                e.HasKey(x => x.ObstacleId);
                e.Property(x => x.ObstacleId).HasMaxLength(3).IsRequired();
                e.Property(x => x.ObstacleName).HasMaxLength(30).IsRequired();
            });

            // ===== Konfigurasjon av Report =====
            b.Entity<Report>(e =>
            {
                e.HasKey(x => x.ReportId);
                e.Property(x => x.ReportId).HasColumnType("char(10)");
                e.Property(x => x.UserId).IsRequired(); // Fremmednøkkel til AspNetUsers
                e.Property(x => x.Latitude).HasColumnType("decimal(11,9)");
                e.Property(x => x.Longitude).HasColumnType("decimal(12,9)");
                e.Property(x => x.HeightFeet).HasColumnType("smallint");
                e.Property(x => x.ObstacleId).HasMaxLength(3);
                e.Property(x => x.Description).HasColumnType("text");
                e.Property(x => x.DateTime).IsRequired();
                e.Property(x => x.Status).HasMaxLength(20).IsRequired();

                // Relasjon: Report -> ApplicationUser (AspNetUsers)
                e.HasOne(x => x.User)
                    .WithMany(u => u.Reports)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relasjon: Report -> ObstacleType
                e.HasOne(x => x.ObstacleType)
                    .WithMany(o => o.Reports)
                    .HasForeignKey(x => x.ObstacleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Konfigurasjon av Organization =====
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

            // ===== OrganizationUser (koblingstabell) =====
            b.Entity<OrganizationUser>(e =>
            {
                // Sammensatt primærnøkkel: (OrganizationId, UserId)
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
            
            // ===== Konfigurasjon av Notification =====
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
                    .WithMany() // evt. lag en ICollection<Notification> på ApplicationUser hvis ønskelig
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Report)
                    .WithMany()
                    .HasForeignKey(x => x.ReportId)
                    .OnDelete(DeleteBehavior.SetNull);
            });


            // ===== Standarddata for ObstacleTypes =====
            b.Entity<ObstacleTypeEntity>().HasData(
                new ObstacleTypeEntity { ObstacleId = "CRN", ObstacleName = "Crane",     SortedOrder = 1 },
                new ObstacleTypeEntity { ObstacleId = "MST", ObstacleName = "Mast",      SortedOrder = 2 },
                new ObstacleTypeEntity { ObstacleId = "PWR", ObstacleName = "PowerLine", SortedOrder = 3 },
                new ObstacleTypeEntity { ObstacleId = "TWR", ObstacleName = "Tower",     SortedOrder = 4 },
                new ObstacleTypeEntity { ObstacleId = "BLD", ObstacleName = "Building",  SortedOrder = 5 },
                new ObstacleTypeEntity { ObstacleId = "OTH", ObstacleName = "Other",     SortedOrder = 9 }
            );

            // ===== Standarddata for Organizations =====
            b.Entity<Organization>().HasData(
                new Organization { OrganizationId = 1, Name = "Norsk Luftambulanse",         ShortCode = "NLA" },
                new Organization { OrganizationId = 2, Name = "Luftforsvaret",               ShortCode = "LFS" },
                new Organization { OrganizationId = 3, Name = "Kartverket", ShortCode = "KRT" }
            );
        }
    }
