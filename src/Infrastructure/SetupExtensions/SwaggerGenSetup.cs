using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Roller.Infrastructure.SetupExtensions;

public static class SwaggerGenSetup
{
    public static IServiceCollection AddRollerSwaggerGen(this IServiceCollection services,
        Action<SwaggerGenOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddSwaggerGen(options =>
        {
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var filePath in Directory.GetFiles(directory, "*.xml"))
            {
                try
                {
                    options.IncludeXmlComments(filePath, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
            }

            configureOptions?.Invoke(options);
        });
        return services;
    }
}