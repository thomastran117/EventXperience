using backend.Models;

using Microsoft.EntityFrameworkCore;

namespace backend.Resources
{
    public class AppDatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Club> Clubs { get; set; } = null!;
        public DbSet<EventClub> EventClubs { get; set; } = null!;
        public AppDatabaseContext(DbContextOptions<AppDatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.GoogleID)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.MicrosoftID)
                .IsUnique();

            modelBuilder.Entity<Club>()
                .HasOne(c => c.User)
                .WithMany(u => u.Clubs)
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventClub>()
                .HasOne(c => c.Club)
                .WithMany(e => e.EventClubs)
                .HasForeignKey(c => c.ClubId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Club>()
                .HasIndex(c => c.UserId);
        }
    }
}
