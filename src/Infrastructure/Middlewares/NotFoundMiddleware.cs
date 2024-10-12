namespace Roller.Infrastructure.Middlewares;

public class NotFoundMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.StatusCode == 404)
        {
            var message = new MessageData(false, $"路径: {context.Request.Path.Value} 不存在", 404);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(message.Serialize());
        }
    }
}