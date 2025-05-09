namespace Roller.Infrastructure.HttpContextUser;

public class UserContext<TKey>(
    IHttpContextAccessor httpContextAccessor,
    JwtOptions jwtOptions,
    IAesEncryptionService aesEncryptionService,
    JsonWebTokenHandler jsonWebTokenHandler) : IUserContext<TKey> where TKey : IEquatable<TKey>
{
    private readonly ClaimsPrincipal principal = httpContextAccessor.HttpContext?.User ??
                                                 throw new ArgumentNullException(
                                                     nameof(httpContextAccessor.HttpContext), "HttpContext is null");

    private TKey? _id;

    private string? _username;

    private string? _name;

    private string? _email;

    private string[]? _roleIds;

    private string? _remoteIpAddress;

    private string[]? _permissions;

    private string[]? _roleNames;

    public TKey? Id
    {
        get => GetIdFromClaims();
        set => _id = value;
    }

    public string Username
    {
        get => _username ??= GetClaimValue(JwtRegisteredClaimNames.UniqueName);
        set => _username = value;
    }

    public string Name
    {
        get => _name ??= GetClaimValue(JwtRegisteredClaimNames.Name);
        set => _name = value;
    }

    public string Email
    {
        get => _email ??= GetClaimValue(JwtRegisteredClaimNames.Email);
        set => _email = value;
    }

    public string[] RoleIds
    {
        get => _roleIds ??= GetClaimValues(ClaimConstants.RoleId);
        set => _roleIds = value;
    }

    public string[] Permissions
    {
        get => _permissions ??= GetClaimValues(ClaimConstants.PermissionCode);
        set => _permissions = value;
    }

    public string[] RoleNames
    {
        get => _roleNames ??= GetClaimValues(ClaimTypes.Role);
        set => _roleNames = value;
    }

    public string RemoteIpAddress
    {
        get => _remoteIpAddress ??= httpContextAccessor.HttpContext?.GetRealIpAddress()!;
        set => _remoteIpAddress = value;
    }

    public JwtTokenInfo GenerateTokenInfo(
        IList<Claim>? claims = null,
        int duration = 0,
        string schemeName = JwtBearerDefaults.AuthenticationScheme)
    {
        if (duration == 0)
        {
            duration = jwtOptions.Duration;
        }

        claims ??= GetClaimsFromUserContext();
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Issuer = jwtOptions.Issuer,
            Audience = jwtOptions.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddSeconds(duration),
            SigningCredentials = jwtOptions.SigningCredentials,
            IssuedAt = DateTime.UtcNow
        };

        var token = jsonWebTokenHandler.CreateToken(tokenDescriptor);
        token = aesEncryptionService.Encrypt(token);
        return new JwtTokenInfo(token, duration, schemeName);
    }

    public IList<Claim> GetClaimsFromUserContext(bool includePermissions = false)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.UniqueName, Username),
            new(JwtRegisteredClaimNames.NameId, Id?.ToString() ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, Name),
            new(JwtRegisteredClaimNames.Email, Email)
        };
        claims.AddRange(RoleIds.Select(rId => new Claim(ClaimConstants.RoleId, rId)));
        claims.AddRange(RoleNames.Select(rName => new Claim(ClaimTypes.Role, rName)));
        if (includePermissions)
        {
            claims.AddRange(Permissions.Select(p => new Claim(ClaimConstants.PermissionCode, p)));
        }

        return claims;
    }

    private TKey GetIdFromClaims()
    {
        if (_id is not null && !_id.Equals(default))
        {
            return _id;
        }

        var idClaim = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.NameId);
        _id = (TKey)Convert.ChangeType(idClaim.Value, typeof(TKey));
        return _id;
    }

    private string GetClaimValue(string claimType)
    {
        return principal.Claims.First(c => c.Type == claimType).Value;
    }

    private string[] GetClaimValues(string claimType)
    {
        return principal.Claims.Where(c => c.Type == claimType).Select(c => c.Value).ToArray();
    }
}