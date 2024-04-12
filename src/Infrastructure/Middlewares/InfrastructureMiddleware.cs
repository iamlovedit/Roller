using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Roller.Infrastructure.Seed;
using Serilog;

namespace Roller.Infrastructure.Middlewares;

public static class InfrastructureMiddleware
{
    public static void UseInfrastructure(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                var message = new MessageData<Exception>(false, "An exception was thrown", 500);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(message));
            });
        });
        app.UseCors("cors");

        app.UseAuthentication();

        app.UseRouting();

        app.UseSerilogLogging();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    public static void UseSerilogLogging(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "[{RemoteIpAddress}] [{RequestScheme}] [{RequestHost}] [{RequestMethod}] [{RequestPath}] responded [{StatusCode}] in [{Elapsed:0.0000}] ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var value))
                {
                    diagnosticContext.Set("RemoteIpAddress", value.ToString());
                }
                else
                {
                    diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.MapToIPv4());
                }
            };
        });
    }

    public static void GenerateSeed(this IApplicationBuilder app, Action<DatabaseSeed> seedBuilder)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(seedBuilder);
        using var scope = app.ApplicationServices.CreateScope();
        var databaseSeed = scope.ServiceProvider.GetRequiredService<DatabaseSeed>();
        seedBuilder.Invoke(databaseSeed);
    }
}