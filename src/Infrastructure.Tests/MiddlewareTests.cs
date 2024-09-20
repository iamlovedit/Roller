using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Roller.Infrastructure;
using Roller.Infrastructure.Middlewares;
using Roller.Infrastructure.Utils;

namespace Infrastructure.Tests;

public class MiddlewareTests
{
    [Fact]
    public async Task NotFoundMiddlewareShouldReturn404()
    {
        using var host = await new HostBuilder().ConfigureWebHost(builder =>
        {
            builder.UseTestServer().Configure(app => { app.UseMiddleware<NotFoundMiddleware>(); });
        }).StartAsync();
        var response = await host.GetTestClient().GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();
        var message = content.Deserialize<MessageData>();
        Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        Assert.Equal(HttpStatusCode.NotFound.GetHashCode(), message.StatusCode);
    }
}