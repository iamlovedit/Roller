namespace Roller.Infrastructure.Security;

public class JwtTokenInfo(double expiredIn, string tokenType)
{
    public string? Token { get; }

    public double? ExpiredIn { get; } = expiredIn;

    public string? TokenType { get; } = tokenType;

    public string? RefreshToken { get; set; }

    public double? RefreshExpiredIn { get; set; }

    public JwtTokenInfo(string token, double expiredIn, string tokenType) : this(expiredIn, tokenType)
    {
        Token = token;
    }
}