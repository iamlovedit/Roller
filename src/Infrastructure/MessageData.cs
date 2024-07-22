namespace Roller.Infrastructure;

public class MessageData(bool succeed, string message, int statusCode = 200)
{
    public int StatusCode { get; set; } = statusCode;

    public bool Succeed { get; set; } = succeed;

    public string? Message { get; set; } = message;
}

public class MessageData<T> : MessageData
{
    public T? Response { get; set; }

    public MessageData(bool succeed, string message, T response, int statusCode = 200) : base(succeed, message,
        statusCode)
    {
        Response = response;
    }
}