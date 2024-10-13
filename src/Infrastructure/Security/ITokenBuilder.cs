using System.Security.Claims;

namespace Roller.Infrastructure.Security;

public interface ITokenBuilder<TId> where TId : IEquatable<TId>
{
    JwtTokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims);

    string DecryptCipherToken(string cipherToken);

    bool VerifyToken(string token);

    double GetTokenExpirationSeconds();

    long ParseUIdFromToken(string token);

    IList<Claim> GetClaimsFromUserContext(IUserContext<TId> userContext);
}