using backend.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDatabaseContextFactory : IDesignTimeDbContextFactory<AppDatabaseContext>
{
    public AppDatabaseContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
            ?? "Server=localhost;Port=3306;Database=eventapp;User=root;Password=password123";

        var optionsBuilder = new DbContextOptionsBuilder<AppDatabaseContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new AppDatabaseContext(optionsBuilder.Options);
    }
}
