using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using Application.Abstractions.Behaviours;


namespace Application
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            });
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;

        }
    }
}
