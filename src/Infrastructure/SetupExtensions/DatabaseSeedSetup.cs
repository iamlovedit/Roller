using Microsoft.Extensions.DependencyInjection;
using Roller.Infrastructure.Seed;

namespace Roller.Infrastructure.SetupExtensions
{
    public static class DatabaseSeedSetup
    {
        public static void AddDatabaseSeedSetup(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddScoped<DatabaseContext>();
            services.AddScoped<DatabaseSeed>();
        }
    }
}
