using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Roller.Infrastructure.Filters;
using Roller.Infrastructure.Repository;
using Roller.Infrastructure.Security;

namespace Roller.Infrastructure.SetupExtensions;

public static class InfrastructureSetup
{
    public static void AddInfrastructureSetup(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var services = builder.Services;
        var configuration = builder.Configuration;
        services.AddSingleton<JwtSecurityTokenHandler>();
        services.AddSingleton<RollerTokenHandler>();
        services.AddSingleton<IAesEncryptionService, AesEncryptionService>();
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerOptionsPostConfigureOptions>();
        services.AddSingleton<ITokenBuilder, TokenBuilder>();
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddMapster();
        services.AddDatabaseSeedSetup();
        services.AddSingleton(provider =>
            new MapperConfiguration(config => { config.AddProfile(new MappingProfile()); }).CreateMapper());

        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });

        services.AddCorsSetup(configuration);

        services.AddControllers(options => { options.Filters.Add(typeof(GlobalExceptionsFilter)); }).AddNewtonsoftJson(
            options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

        services.AddApiVersionSetup(configuration);

        services.AddSqlSugarSetup(configuration, builder.Environment);

        services.AddRedisCacheSetup(configuration);

        builder.AddSerilogSetup();

        services.AddJwtAuthenticationSetup(configuration);

        services.AddAuthorizationSetup(configuration);
        
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
    }
}