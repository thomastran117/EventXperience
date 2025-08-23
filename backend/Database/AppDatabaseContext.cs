using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Databases;

public class AppDatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public AppDatabaseContext(DbContextOptions<AppDatabaseContext> options) : base(options) { }
}
