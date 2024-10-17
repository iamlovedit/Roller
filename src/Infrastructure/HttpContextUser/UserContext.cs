using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Roller.Infrastructure.HttpContextUser;

public class UserContext<TKey>(
    IHttpContextAccessor httpContextAccessor,
    JwtOptions jwtOptions,
    IAesEncryptionService aesEncryptionService,
    JwtSecurityTokenHandler jwtSecurityTokenHandler) : IUserContext<TKey> where TKey : IEquatable<TKey>
{
    private readonly ClaimsPrincipal principal = httpContextAccessor?.HttpContext?.User;

    private TKey? _id;

    private string? _username;

    private string? _name;

    private string? _email;

    private string[]? _roleIds;

    private string? _remoteIpAddress;

    public TKey? Id
    {
        get => _id ??= GetIdFromClaims();
        set => _id = value;
    }

    public string Username
    {
        get => _username ??= principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value;
        set => _username = value;
    }

    public string Name
    {
        get => _name ??= principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value;
        set => _name = value;
    }

    public string Email
    {
        get => _email ??= principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
        set => _email = value;
    }

    public string[] RoleIds
    {
        get => _roleIds ??= principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
        set => _roleIds = value;
    }

    public string RemoteIpAddress
    {
        get => _remoteIpAddress ??= httpContextAccessor.HttpContext?.GetRequestIp()!;
        set => _remoteIpAddress = value;
    }

    public JwtTokenInfo GenerateTokenInfo(
        IReadOnlyCollection<Claim> claims,
        double? duration = null,
        string schemeName = JwtBearerDefaults.AuthenticationScheme)
    {
        var securityToken = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.Add(jwtOptions.Expiration),
            signingCredentials: jwtOptions.SigningCredentials);
        var token = jwtSecurityTokenHandler.WriteToken(securityToken);
        token = aesEncryptionService.Encrypt(token);
        return new JwtTokenInfo(token, duration ?? jwtOptions.Expiration.TotalSeconds,
            schemeName);
    }


    public IList<Claim> GetClaimsFromUserContext(TimeSpan? expiration)
    {
        expiration ??= jwtOptions.Expiration;
        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.UniqueName, Username),
            new(JwtRegisteredClaimNames.NameId, Id?.ToString()),
            new(JwtRegisteredClaimNames.Name, Name),
            new(JwtRegisteredClaimNames.Email, Email),
            new(JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(DateTime.Now).ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Exp, expiration.ToString()!)
        };
        claims.AddRange(RoleIds.Select(rId => new Claim(ClaimTypes.Role, rId)));
        return claims;
    }

    private TKey GetIdFromClaims()
    {
        var idClaim = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.NameId);
        return (TKey)Convert.ChangeType(idClaim.Value, typeof(TKey));
    }
}