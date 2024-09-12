namespace Roller.Infrastructure.Exceptions;

public class FriendlyException(string message, int code = 500) : Exception
{
    public string Message { get; } = message;

    public int Code { get; } = code;

    public MessageData ConvertToMessage()
    {
        return new MessageData(false, Message, Code);
    }
}