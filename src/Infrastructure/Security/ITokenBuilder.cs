namespace Roller.Infrastructure.Security;

public interface ITokenBuilder
{
    JwtTokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims);

    string DecryptCipherToken(string cipherToken);

    bool VerifyToken(string token);

    double GetTokenExpirationSeconds();

    long ParseUIdFromToken(string token);

    IList<Claim> GetClaimsFromUserContext<TId>(IUserContext<TId> userContext) where TId : IEquatable<TId>;
    
    void SetUserContext(TokenValidatedContext context);
}