using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Api.Configuration
{
    public static class DatabaseConfiguration
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Forzar UTF-8 en la connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            // Agregar Application Name para debugging y CharSet para UTF-8
            if (!connectionString!.Contains("Application Name"))
                connectionString += ";Application Name=API_POS";
            
            services.AddDbContext<POSDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly("Infrastructure")
                ),
                ServiceLifetime.Scoped  
            );

            return services;
        }
    }
}