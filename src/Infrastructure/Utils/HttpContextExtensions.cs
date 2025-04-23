namespace Roller.Infrastructure.Utils;

public static class HttpContextExtensions
{
    public static string GetRequestIp(this HttpContext context)
    {
        var ip = context.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                 context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                 context.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrEmpty(ip) && ip.Contains(','))
        {
            ip = ip.Split(',')[0].Trim();
        }

        return ip ?? "unknown";
    }

    public static T? GetRequestHeaderValue<T>(this HttpContext context, string headerName)
    {
        if (!context.Request.Headers.TryGetValue(headerName, out var value))
        {
            return default;
        }

        var valueStr = value.ToString();
        if (!string.IsNullOrEmpty(valueStr) || !string.IsNullOrWhiteSpace(valueStr))
        {
            return (T)Convert.ChangeType(valueStr, typeof(T));
        }

        return default;
    }
}