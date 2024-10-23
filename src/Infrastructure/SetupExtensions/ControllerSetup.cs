using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Roller.Infrastructure.Filters;

namespace Roller.Infrastructure.SetupExtensions;

public static class ControllerSetup
{
    public static IServiceCollection AddRollerControllers(this IServiceCollection services,
        Action<IMvcBuilder>? configureMvc = null,
        Action<MvcOptions>? mvcOptions = null,
        Action<MvcNewtonsoftJsonOptions>? newtonsoftJsonOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        var mvcBuilder = services.AddControllers(options =>
        {
            options.Filters.Add<GlobalExceptionsFilter>();
            options.Filters.Add<IdempotencyFilter>();
            mvcOptions?.Invoke(options);
        });
        mvcBuilder.AddNewtonsoftJson(
            options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                newtonsoftJsonOptions?.Invoke(options);
            });
        configureMvc?.Invoke(mvcBuilder);
        return services;
    }
}