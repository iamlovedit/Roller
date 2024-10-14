using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Roller.Infrastructure.Security;

public abstract class TokenBuilderBase<TId>(
    IAesEncryptionService aesEncryptionService,
    JwtOptions jwtOptions,
    JwtSecurityTokenHandler jwtSecurityTokenHandler)
    : ITokenBuilder where TId : IEquatable<TId>
{
    public virtual string DecryptCipherToken(string cipherToken)
    {
        if (string.IsNullOrEmpty(cipherToken))
        {
            throw new ArgumentException($"{nameof(cipherToken)} is null or empty。", nameof(cipherToken));
        }

        return aesEncryptionService.Decrypt(cipherToken);
    }

    public virtual JwtTokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims)
    {
        var jwtToken = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.Add(jwtOptions.Expiration),
            signingCredentials: jwtOptions.SigningCredentials);
        var token = jwtSecurityTokenHandler.WriteToken(jwtToken);
        token = aesEncryptionService.Encrypt(token);
        return new JwtTokenInfo(token, jwtOptions.Expiration.TotalSeconds,
            JwtBearerDefaults.AuthenticationScheme);
    }

    public double GetTokenExpirationSeconds()
    {
        return jwtOptions.Expiration.TotalSeconds;
    }

    public long ParseUIdFromToken(string token)
    {
        if (jwtSecurityTokenHandler.CanReadToken(token))
        {
            var jwtToken = jwtSecurityTokenHandler.ReadJwtToken(token);
            if (long.TryParse(jwtToken.Id, out var id))
            {
                return id;
            }
        }

        return 0;
    }

    public virtual IList<Claim> GetClaimsFromUserContext<TId1>(IUserContext<TId1> userContext)
        where TId1 : IEquatable<TId1>
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
            new(JwtRegisteredClaimNames.Exp, jwtOptions.Expiration.ToString())
        };
        claims.AddRange(userContext.RoleIds.Select(rId => new Claim(ClaimTypes.Role, rId)));
        return claims;
    }

    public void SetUserContext(TokenValidatedContext context)
    {
        var userContext =
            context.HttpContext.RequestServices.GetService(typeof(IUserContext<TId>)) as IUserContext<TId> ??
            throw new NullReferenceException(nameof(IUserContext<TId>));
        var principal = context.Principal ?? throw new NullReferenceException(nameof(context.Principal));
        var idClaim = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.NameId);
        userContext.Id = (TId)Convert.ChangeType(idClaim.Value, typeof(TId));
        userContext.Username =
            principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value;
        userContext.Name = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value;
        userContext.Email = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
        userContext.RoleIds = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
        userContext.RemoteIpAddress = context.HttpContext.GetRequestIp()!;
    }

    public bool VerifyToken(string token)
    {
        var jwt = jwtSecurityTokenHandler.ReadJwtToken(token);
        return jwt.RawSignature ==
               JwtTokenUtilities.CreateEncodedSignature(jwt.RawHeader + "." + jwt.RawPayload,
                   jwtOptions.SigningCredentials);
    }
}