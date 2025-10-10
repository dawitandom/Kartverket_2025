using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext
{
    // database-kontekst (hovedinngangen til databasen)
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) { } // Knytter databasen til appen

        // Advices er tabellen i databasen som lagrer alle råd (hver AdviceDto er én rad).

        public DbSet<AdviceDto> Advices { get; set; }

        // Tabell for rapporter
        public DbSet<Report> Reports => Set<Report>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Primærnøkkel for AdviceDto
            modelBuilder.Entity<AdviceDto>()
                .HasKey(a => a.AdviceId);

            // Lagre ReportType som int i databasen
            modelBuilder.Entity<Report>()
                .Property(r => r.Type)
                .HasConversion<int>();

            base.OnModelCreating(modelBuilder); // Behold standard oppførsel
        }
    }
}   