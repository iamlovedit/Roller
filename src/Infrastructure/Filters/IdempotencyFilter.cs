using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roller.Infrastructure.Cache;
using Roller.Infrastructure.Utils;

namespace Roller.Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class IdempotencyAttribute(int seconds = 5) : Attribute
{
    public int Seconds { get; set; } = seconds;
}

public class IdempotencyFilter(ILogger<IdempotencyFilter> logger, IRedisBasketRepository redis) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            var idempotencyAttribute =
                controllerActionDescriptor.MethodInfo.GetCustomAttribute<IdempotencyAttribute>();
            if (idempotencyAttribute is null)
            {
                await next();
            }

            var request = context.HttpContext.Request;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            var hashBytes = MD5.HashData(Encoding.ASCII.GetBytes(body));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            var redisKey = $"{request.Path.Value}:{hashString}";

            if (await redis.Exist(redisKey))
            {
                logger.LogWarning("路径 {path} 请求频繁，请求ip：{ip}", request.Path,
                    context.HttpContext.GetRequestIp());
                var message = new MessageData(false, "非法请求", 409);
                context.Result = new ObjectResult(message) { StatusCode = 200 };
            }
            else
            {
                await redis.Set(redisKey, 0, TimeSpan.FromSeconds(idempotencyAttribute!.Seconds));
                await next();
            }
        }
    }
}