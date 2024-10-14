namespace Roller.Infrastructure.HttpContextUser;

public interface IUserContext
{
    long Id { get; set; }

    string Username { get; set; }

    string Name { get; set; }

    string Email { get; set; }

    string[] RoleIds { get; set; }

    string RemoteIpAddress { get; set; }
    
    JwtTokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims);
    
    IList<Claim> GetClaimsFromUserContext(IUserContext userContext);
}