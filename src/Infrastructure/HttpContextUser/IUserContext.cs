namespace Roller.Infrastructure.HttpContextUser;

public interface IUserContext<out TKey> where TKey : IEquatable<TKey>
{
    TKey Id { get; }

    string Username { get; }

    string Name { get; }

    string Email { get; }

    string[] RoleIds { get; }

    string RemoteIpAddress { get; }

    JwtTokenInfo GenerateTokenInfo(JwtSecurityToken jwtSecurityToken,double? duration, string schemeName);

    IList<Claim> GetClaimsFromUserContext();
}