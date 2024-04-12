using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roller.Infrastructure.Options;

namespace Roller.Infrastructure.SetupExtensions;

public static class JwtAuthenticationSetup
{
    public static void AddJwtAuthenticationSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var audienceOptions = configuration.GetSection(AudienceOptions.Name).Get<AudienceOptions>();
        if (audienceOptions?.Enable ?? false)
        {
            
        }
    }
}