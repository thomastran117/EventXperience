using worker.Models;

using Microsoft.EntityFrameworkCore;

namespace worker.Resources
{
    public class WorkerDatabaseContext : DbContext
    {
        public DbSet<Club> Clubs { get; set; } = null!;
        public WorkerDatabaseContext(DbContextOptions<WorkerDatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Club>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Rating)
                    .HasPrecision(2, 1);

                entity.HasIndex(c => c.UserId);
            });

        }
    }
}
