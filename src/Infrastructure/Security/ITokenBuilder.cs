using System.Security.Claims;

namespace Roller.Infrastructure.Security;

public interface ITokenBuilder
{
    TokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims);

    string DecryptCipherToken(string cipherToken);

    bool VerifyToken(string token);

    double GetTokenExpirationSeconds();

    long ParseUIdFromToken(string token);
}