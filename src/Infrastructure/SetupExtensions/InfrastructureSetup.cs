using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Roller.Infrastructure.Filters;
using Roller.Infrastructure.Repository;
using Serilog;

namespace Roller.Infrastructure.SetupExtensions;

public static class InfrastructureSetup
{
    public static WebApplicationBuilder AddInfrastructureSetup(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var services = builder.Services;
        var configuration = builder.Configuration;
        services.AddSingleton<JwtSecurityTokenHandler>();
        services.AddSingleton<RollerTokenHandler>();
        services.AddSingleton<IAesEncryptionService, AesEncryptionService>();
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerOptionsPostConfigureOptions>();
        services.AddSingleton(typeof(ITokenBuilder), typeof(TokenBuilderBase<>));
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
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

        services.AddSerilogSetup(configuration);

        services.AddJwtAuthenticationSetup(configuration);

        services.AddAuthorizationSetup(configuration);

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });
        });
        services.AddApiVersionSetup(configuration);

        services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionsFilter>();
                options.Filters.Add<IdempotencyFilter>();
            })
            .AddNewtonsoftJson(
                options =>
                {
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });
        builder.Host.UseSerilog(Log.Logger, true);
        return builder;
    }
}