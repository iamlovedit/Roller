using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Roller.Infrastructure.HttpContextUser;

public class UserContext<TKey>(
    IHttpContextAccessor httpContextAccessor,
    JwtOptions jwtOptions,
    IAesEncryptionService aesEncryptionService,
    JwtSecurityTokenHandler jwtSecurityTokenHandler) : IUserContext<TKey> where TKey : IEquatable<TKey>
{
    private readonly ClaimsPrincipal principal = httpContextAccessor?.HttpContext?.User;
    public TKey Id => GetIdFromClaims();

    public string Username => principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value;

    public string Name => principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value;

    public string Email => principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;

    public string[] RoleIds => principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();

    public string RemoteIpAddress => httpContextAccessor.HttpContext?.GetRequestIp()!;

    public JwtTokenInfo GenerateTokenInfo(
        JwtSecurityToken? securityToken = null,
        double? duration = null,
        string schemeName = JwtBearerDefaults.AuthenticationScheme)
    {
        var claims = GetClaimsFromUserContext();
        securityToken ??= new JwtSecurityToken(
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


    public IList<Claim> GetClaimsFromUserContext()
    {
        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.UniqueName, Username),
            new(JwtRegisteredClaimNames.NameId, Id.ToString() ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, Name),
            new(JwtRegisteredClaimNames.Email, Email),
            new(JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(DateTime.Now).ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Exp, jwtOptions.Expiration.ToString())
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