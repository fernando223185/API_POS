using Application.Abstractions.Catalogue;
using Application.Abstractions.CRM;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Web.Api.Configuration
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddDB(
            this IServiceCollection services,
            IConfiguration configuration) 
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<POSDbContext>(options => options.UseSqlServer(connectionString));
            return services;

        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();

            return services;
        }
    }
}
