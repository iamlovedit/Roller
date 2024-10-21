namespace Roller.Infrastructure.Security;

public class JwtTokenInfo(int duration, string tokenType)
{
    public string? Token { get; }

    public int Duration { get; } = duration;

    public string? TokenType { get; } = tokenType;

    public string? RefreshToken { get; set; }

    public int RefreshTokenDuration { get; set; }

    public JwtTokenInfo(string token, int duration, string tokenType) : this(duration, tokenType)
    {
        Token = token;
    }
}