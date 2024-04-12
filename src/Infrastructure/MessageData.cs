namespace Roller.Infrastructure;

public class MessageData<T>
{
    public int StatusCode { get; set; }

    public bool Succeed { get; set; }

    public string? Message { get; set; }

    public T? Response { get; set; }

    public MessageData(bool succeed, string message, int statusCode = 200)
    {
        Succeed = succeed;
        Message = message;
        StatusCode = statusCode;
    }

    public MessageData(bool succeed, string message, T response, int statusCode = 200) : this(succeed, message, statusCode)
    {
        Response = response;
    }
}