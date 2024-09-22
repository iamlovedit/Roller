using Microsoft.Extensions.DependencyInjection;
using Roller.Infrastructure.Seed;

namespace Roller.Infrastructure.SetupExtensions
{
    public static class DatabaseSeedSetup
    {
        public static IServiceCollection AddDatabaseSeedSetup(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddScoped<DatabaseContext>();
            services.AddScoped<DatabaseSeed>();
            return services;
        }
    }
}