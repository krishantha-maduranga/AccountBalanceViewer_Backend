using AccountBalanceViewer.Application.Services;
using AccountBalanceViewer.Application.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AccountBalanceViewer.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });

            services.AddScoped<IAccountBalanceService, AccountBalanceService>();
            
            return services;
        }
    }
}
