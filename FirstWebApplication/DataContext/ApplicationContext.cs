
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext
{
    /// <summary>
    /// Database context for Kartverket Obstacle Reporting System.
    /// Håndterer tilkobling til MySQL database og definerer alle tabeller og relasjoner.
    /// Bruker Entity Framework Core for Object-Relational Mapping (ORM).
    /// </summary>
    public class ApplicationContext : DbContext
    {
        /// <summary>
        /// Constructor som tar imot DbContextOptions fra Dependency Injection.
        /// Konfigurasjonen settes opp i Program.cs med connection string.
        /// </summary>
        /// <param name="opt">Database konfigurasjon (connection string, provider, osv.)</param>
        public ApplicationContext(DbContextOptions<ApplicationContext> opt) : base(opt) { }
        
        // DbSet properties representerer tabeller i databasen
        // Hver DbSet gir CRUD-operasjoner (Create, Read, Update, Delete) for sin entity
        
        /// <summary>
        /// Users tabell - inneholder alle brukere (piloter og admins).
        /// </summary>
        public DbSet<User> Users => Set<User>();
        
        /// <summary>
        /// UserRoles tabell - inneholder roller (Admin og User/Pilot).
        /// </summary>
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        
        /// <summary>
        /// ObstacleTypes tabell - inneholder typer hindringer (Crane, Mast, Tower, osv.).
        /// </summary>
        public DbSet<ObstacleTypeEntity> ObstacleTypes => Set<ObstacleTypeEntity>();
        
        /// <summary>
        /// Reports tabell - inneholder alle hindring-rapporter som er sendt inn.
        /// </summary>
        public DbSet<Report> Reports => Set<Report>();

        /// <summary>
        /// Konfigurerer database-modellen (tabeller, kolonner, relasjoner og seed-data).
        /// Kalles automatisk av Entity Framework når databasen opprettes eller migreres.
        /// </summary>
        /// <param name="b">ModelBuilder for å konfigurere entiteter</param>
        protected override void OnModelCreating(ModelBuilder b)
        {
            // ===== UserRole Konfigurasjon =====
            // Definerer struktur for UserRoles tabell
            b.Entity<UserRole>(e =>
            {
                e.HasKey(x => x.UserRoleId);  // Primary Key
                e.Property(x => x.Role).HasMaxLength(30).IsRequired();  // Rolle navn (maks 30 tegn, påkrevd)
            });

            // ===== User Konfigurasjon =====
            // Definerer struktur for Users tabell
            b.Entity<User>(e =>
            {
                e.HasKey(x => x.UserId);  // Primary Key
                e.Property(x => x.UserId).HasColumnType("char(5)");  // Fast lengde 5 tegn (f.eks. "USR01")
                e.Property(x => x.Username).HasMaxLength(30).IsRequired();  // Brukernavn (påkrevd)
                e.Property(x => x.FirstName).HasMaxLength(30);  // Fornavn (valgfritt)
                e.Property(x => x.LastName).HasMaxLength(30);   // Etternavn (valgfritt)
                e.Property(x => x.Mail).HasMaxLength(200).IsRequired();  // E-post (påkrevd)
                e.Property(x => x.Password).HasColumnType("char(60)").IsRequired();  // Passord (fast lengde 60, påkrevd)
                
                // Definerer relasjon: En User har én UserRole, en UserRole kan ha mange Users
                e.HasOne(x => x.UserRole)
                 .WithMany(r => r.Users)
                 .HasForeignKey(x => x.UserRoleId);  // Foreign Key til UserRoles
            });

            // ===== ObstacleType Konfigurasjon =====
            // Definerer struktur for ObstacleTypes tabell
            b.Entity<ObstacleTypeEntity>(e =>
            {
                e.HasKey(x => x.ObstacleId);  // Primary Key
                e.Property(x => x.ObstacleId).HasMaxLength(3).IsRequired();  // 3-bokstavers kode (f.eks. "CRN")
                e.Property(x => x.ObstacleName).HasMaxLength(30).IsRequired();  // Navn på hindring-type
            });

            // ===== Report Konfigurasjon =====
            // Definerer struktur for Reports tabell
            b.Entity<Report>(e =>
            {
                e.HasKey(x => x.ReportId);  // Primary Key
                e.Property(x => x.ReportId).HasColumnType("char(10)");  // Fast lengde 10 tegn
                e.Property(x => x.UserId).HasColumnType("char(5)").IsRequired();  // Foreign Key til Users
                e.Property(x => x.Latitude).HasColumnType("decimal(11,9)");   // Breddegrad (11 siffer totalt, 9 desimaler)
                e.Property(x => x.Longitude).HasColumnType("decimal(12,9)");  // Lengdegrad (12 siffer totalt, 9 desimaler)
                e.Property(x => x.AltitudeFeet).HasColumnType("smallint");    // Høyde i fot (small integer)
                e.Property(x => x.ObstacleId).HasMaxLength(3).IsRequired();   // Foreign Key til ObstacleTypes
                e.Property(x => x.Description).HasColumnType("text").IsRequired();  // Beskrivelse (tekst uten lengdebegrensning)
                e.Property(x => x.DateTime).IsRequired();  // Tidspunkt for rapport
                e.Property(x => x.Status).HasMaxLength(20).IsRequired();  // Status: "Pending", "Approved" eller "Rejected"

                // Definerer relasjon: En Report har én User, en User kan ha mange Reports
                e.HasOne(x => x.User)
                    .WithMany(u => u.Reports)
                    .HasForeignKey(x => x.UserId);

                // Definerer relasjon: En Report har én ObstacleType, en ObstacleType kan ha mange Reports
                e.HasOne(x => x.ObstacleType)
                    .WithMany(o => o.Reports)
                    .HasForeignKey(x => x.ObstacleId);
            });

            // ===== SEED DATA =====
            // Data som automatisk legges inn når databasen opprettes
            
            // Seed UserRoles - to roller: Admin og User (Pilot)
            b.Entity<UserRole>().HasData(
                new UserRole { UserRoleId = 1, Role = "Admin" },
                new UserRole { UserRoleId = 2, Role = "User" }
            );

            // Seed test users - to testbrukere for utvikling/testing
            // OBS: Passord er lagret i plain text her kun for testing!
            // I produksjon skal passord haskes (f.eks. med BCrypt)
            b.Entity<User>().HasData(
                new User 
                { 
                    UserId = "USR01",
                    Username = "testuser",
                    FirstName = "Test",
                    LastName = "User",
                    Mail = "test@example.com",
                    Password = "password123                                             ", // Padding til 60 tegn
                    UserRoleId = 2  // User/Pilot rolle
                },
                new User 
                { 
                    UserId = "ADM01",
                    Username = "admin",
                    FirstName = "Admin",
                    LastName = "User",
                    Mail = "admin@example.com",
                    Password = "admin123                                                ", // Padding til 60 tegn
                    UserRoleId = 1  // Admin rolle
                }
            );

            // Seed obstacle types - 6 forhåndsdefinerte hindring-typer
            // SortedOrder bestemmer rekkefølgen i dropdown-lister
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
}