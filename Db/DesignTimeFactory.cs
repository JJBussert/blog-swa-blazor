using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Db
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SimpleDbContext>
    {
        public SimpleDbContext CreateDbContext(string[] args)
        {
            // Get the parent directory path
            var parentDirectory = Directory.GetParent(Directory.GetCurrentDirectory());
            var envFilePath = Path.Combine(parentDirectory!.FullName, ".env");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvFile(envFilePath)
                .AddEnvironmentVariables();

            IConfiguration configuration = configurationBuilder.Build();

            string? connectionString = configuration["DB_CONNECTION_STRING"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Unable to find DB_CONNECTION_STRING in .env file or EnvironmentVariables.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<SimpleDbContext>();
            
            // Set CommandTimeout, e.g., 180 seconds (3 minutes)
            optionsBuilder.UseSqlServer(connectionString, options => options.CommandTimeout(180));


            return new SimpleDbContext(optionsBuilder.Options);
        }
    }
}
