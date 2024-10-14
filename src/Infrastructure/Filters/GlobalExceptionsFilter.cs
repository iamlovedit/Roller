using Microsoft.AspNetCore.Mvc.Filters;
using Roller.Infrastructure.Exceptions;

namespace Roller.Infrastructure.Filters;

public class GlobalExceptionsFilter(ILogger<GlobalExceptionsFilter> logger) : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        return Task.Run(() =>
        {
            if (context.ExceptionHandled)
            {
                return;
            }

            var message = default(MessageData);
            if (context.Exception is FriendlyException fe)
            {
                message = fe.ConvertToMessage();
            }
            else
            {
                logger.LogError(context.Exception.Message);
                message = new MessageData(false, context.Exception.Message, 500);
            }

            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = "application/json;charset=utf-8",
                Content = message.Serialize()
            };
            context.ExceptionHandled = true;
        });
    }
}