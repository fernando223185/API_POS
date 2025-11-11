using Application.Abstractions.CRM;
using Application.Abstractions.Catalogue;
using Application.Abstractions.Sales_Orders;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Api.Configuration
{
    public static class RepositoryConfiguration
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // ? CORRECTA: Todos los repositorios como Scoped
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISalesRepository, SalesRepository>();

            return services;
        }
    }
}