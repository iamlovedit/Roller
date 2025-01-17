using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Roller.Infrastructure.Options;
using Serilog;

namespace Roller.Infrastructure.Middlewares;

public static class InfrastructureMiddleware
{
    public static void UseInfrastructure(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status200OK;
                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();
                var message = new MessageData(false, "服务异常，请联系管理员", 500);
                Log.Logger?.Error(exceptionHandlerPathFeature?.Error,exceptionHandlerPathFeature?.Error.Message!);
                await context.Response.WriteAsync(message.Serialize());
            });
        });

        app.UseRouting();

        app.UseCors(CrossOptions.Name);

        app.UseMiddleware<NotFoundMiddleware>();

        app.UseAuthentication();

        app.UseAuthorization();


        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.UseVersionedSwaggerUi();
        }

        app.MapControllers();

        app.UseSerilogLogging();

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
                diagnosticContext.Set("RemoteIpAddress", httpContext.GetRequestIp());
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