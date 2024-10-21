namespace Roller.Infrastructure.HttpContextUser;

public interface IUserContext<TKey> where TKey : IEquatable<TKey>
{
    TKey? Id { get; set; }

    string Username { get; set; }

    string Name { get; set; }

    string Email { get; set; }

    string[] RoleIds { get; set; }

    string[] Permissions { get; set; }

    string[] RoleNames { get; set; }

    string RemoteIpAddress { get; set; }

    JwtTokenInfo GenerateTokenInfo(
        IList<Claim> claims,
        double duration = 0,
        string schemeName = JwtBearerDefaults.AuthenticationScheme);

    IList<Claim> GetClaimsFromUserContext(bool includePermissions = false);
}