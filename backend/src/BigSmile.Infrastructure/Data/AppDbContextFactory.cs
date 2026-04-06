using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BigSmile.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var startupProjectDirectory = Path.Combine(currentDirectory, "..", "BigSmile.Api");
            var basePath = Directory.Exists(startupProjectDirectory) ? startupProjectDirectory : currentDirectory;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=BigSmile;Trusted_Connection=True;MultipleActiveResultSets=true";
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options, configuration);
        }
    }
}