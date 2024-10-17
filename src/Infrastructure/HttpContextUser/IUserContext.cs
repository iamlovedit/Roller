namespace Roller.Infrastructure.HttpContextUser;

public interface IUserContext<TKey> where TKey : IEquatable<TKey>
{
    TKey? Id { get; set; }

    string Username { get; set; }

    string Name { get; set; }

    string Email { get; set; }

    string[] RoleIds { get; set; }

    string RemoteIpAddress { get; set; }

    JwtTokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims, double? duration, string schemeName);

    IList<Claim> GetClaimsFromUserContext(IUserContext<TKey> userContext, TimeSpan? expiration);
}