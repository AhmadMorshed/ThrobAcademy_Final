using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Throb.Data.DbContext
{
    public class ThrobDbContextFactory : IDesignTimeDbContextFactory<ThrobDbContext>
    {
        public ThrobDbContext CreateDbContext(string[] args)
        {
            // تحميل appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ThrobDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            Console.WriteLine(">>> Factory invoked <<<");

            return new ThrobDbContext(optionsBuilder.Options);

        }
    }
}
