using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Roller.Infrastructure.HttpContextUser;

public class UserContext : IUserContext
{
    private readonly JwtOptions _jwtOptions;
    private readonly IAesEncryptionService _aesEncryptionService;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    public UserContext(IHttpContextAccessor httpContextAccessor,
        JwtOptions jwtOptions,
        IAesEncryptionService aesEncryptionService,
        JwtSecurityTokenHandler jwtSecurityTokenHandler)
    {
        _jwtOptions = jwtOptions;
        _aesEncryptionService = aesEncryptionService;
        _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
        var principal = httpContextAccessor?.HttpContext?.User ??
                        throw new NullReferenceException(nameof(httpContextAccessor.HttpContext.User));
        var idClaim = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.NameId);
        Id = long.Parse(idClaim.Value);
        Username = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value;
        Name = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value;
        Email = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
        RoleIds = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
        RemoteIpAddress = httpContextAccessor.HttpContext.GetRequestIp()!;
    }

    public long Id { get; set; }

    public string Username { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string[] RoleIds { get; set; }

    public string RemoteIpAddress { get; set; }

    public JwtTokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims)
    {
        var jwtToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.Add(_jwtOptions.Expiration),
            signingCredentials: _jwtOptions.SigningCredentials);
        var token = _jwtSecurityTokenHandler.WriteToken(jwtToken);
        token = _aesEncryptionService.Encrypt(token);
        return new JwtTokenInfo(token, _jwtOptions.Expiration.TotalSeconds,
            JwtBearerDefaults.AuthenticationScheme);
    }

    public IList<Claim> GetClaimsFromUserContext(IUserContext userContext)
    {
        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.UniqueName, userContext.Username),
            new(JwtRegisteredClaimNames.NameId, userContext.Id.ToString() ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, userContext.Name),
            new(JwtRegisteredClaimNames.Email, userContext.Email),
            new(JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(DateTime.Now).ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Exp, _jwtOptions.Expiration.ToString())
        };
        claims.AddRange(userContext.RoleIds.Select(rId => new Claim(ClaimTypes.Role, rId)));
        return claims;
    }
}