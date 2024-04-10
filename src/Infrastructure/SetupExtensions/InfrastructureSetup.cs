using AutoMapper;
using Infrastructure.Filters;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Infrastructure.SetupExtensions;

public static class InfrastructureSetup
{
    public static void AddInfrastructureSetup(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var services = builder.Services;
        var configuration = builder.Configuration;
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton(provider =>
            new MapperConfiguration(config => { config.AddProfile(new MappingProfile()); }).CreateMapper());

        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });

        services.AddCors(options =>
        {
            options.AddPolicy("cors", policy => { policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin(); });
        });

        services.AddControllers(options => { options.Filters.Add(typeof(GlobalExceptionsFilter)); }).AddNewtonsoftJson(
            options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
        
        services.AddSqlSugarSetup(configuration, builder.Environment);
        
        services.AddRedisCacheSetup(configuration);
        
    }
}