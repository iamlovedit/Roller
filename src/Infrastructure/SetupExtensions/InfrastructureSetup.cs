using Mapster;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Roller.Infrastructure.SetupExtensions;

public static class InfrastructureSetup
{
    public static WebApplicationBuilder AddInfrastructureSetup(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var services = builder.Services;
        var configuration = builder.Configuration;
        services.AddAesEncryption();

        services.AddRollerTokenContext();

        services.AddHttpContextAccessor();

        services.AddRollerUserContext<long>();

        services.AddMapster();

        services.AddDatabaseSeedSetup();

        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });

        services.AddCorsSetup(configuration);

        services.AddSqlSugarSetup(configuration, builder.Environment);

        services.AddRedisCacheSetup(configuration);

        services.AddMongoDbSetup(configuration);

        services.AddSerilogSetup(configuration);

        services.AddJwtAuthenticationSetup(configuration);

        services.AddAuthorizationSetup(configuration);

        services.AddEndpointsApiExplorer();

        services.AddRollerSwaggerGen();

        services.AddApiVersionSetup(configuration);

        services.AddRollerControllers();

        builder.Host.UseSerilog(Log.Logger, true);
        return builder;
    }
}