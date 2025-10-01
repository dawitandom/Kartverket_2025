using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) { }

        // Existing set
        public DbSet<AdviceDto> Advices { get; set; } = null!;

        // NEW: Reports
        public DbSet<Report> Reports => Set<Report>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing
            modelBuilder.Entity<AdviceDto>()
                .HasKey(a => a.AdviceId);

            // Store enum as int (portable across providers)
            modelBuilder.Entity<Report>()
                .Property(r => r.Type)
                .HasConversion<int>();

            base.OnModelCreating(modelBuilder);
        }
    }
}   