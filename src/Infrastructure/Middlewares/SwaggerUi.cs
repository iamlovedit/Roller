using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Roller.Infrastructure.Options;

namespace Roller.Infrastructure.Middlewares;

public static class SwaggerUi
{
    public static void UseVersionedSwaggerUI(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        var apiVersionDescriptionProvider =
            app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var serviceInfo = configuration.GetSection(ServiceInfoOptions.Name).Get<ServiceInfoOptions>();
        if (serviceInfo is not { Enable: true })
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
            });
        }
        else
        {
            app.UseSwagger(
                options =>
                {
                    options.RouteTemplate = $"{serviceInfo.ServiceName}" + "/swagger/{documentName}/swagger.json";
                }
            );
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/{serviceInfo.ServiceName}/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                    options.RoutePrefix = $"{serviceInfo.ServiceName}/swagger";
                }
            });
        }
    }
}