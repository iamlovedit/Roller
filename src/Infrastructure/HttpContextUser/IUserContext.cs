namespace Roller.Infrastructure.HttpContextUser;

public interface IUserContext<TKey> where TKey : IEquatable<TKey>
{
    TKey Id { get; }

    string Username { get; }

    string Name { get; }

    string Email { get; }

    string[] RoleIds { get; }

    string RemoteIpAddress { get; }

    JwtTokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims, double? duration, string schemeName);

    IList<Claim> GetClaimsFromUserContext(IUserContext<TKey> userContext, TimeSpan? expiration);
}