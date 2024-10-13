using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Roller.Infrastructure.Security;

public abstract class TokenBuilderBase<TId>(
    IAesEncryptionService aesEncryptionService,
    PermissionRequirement permissionRequirement,
    JwtSecurityTokenHandler jwtSecurityTokenHandler,
    IConfiguration configuration)
    : ITokenBuilder<TId> where TId : IEquatable<TId>
{
    public string DecryptCipherToken(string cipherToken)
    {
        if (string.IsNullOrEmpty(cipherToken))
        {
            throw new ArgumentException($"{nameof(cipherToken)} is null or empty。", nameof(cipherToken));
        }

        return aesEncryptionService.Decrypt(cipherToken);
    }

    public JwtTokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims)
    {
        var jwtToken = new JwtSecurityToken(
            issuer: permissionRequirement.Issuer,
            audience: permissionRequirement.Audience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.Add(permissionRequirement.Expiration),
            signingCredentials: permissionRequirement.SigningCredentials);
        var token = jwtSecurityTokenHandler.WriteToken(jwtToken);
        token = aesEncryptionService.Encrypt(token);
        return new JwtTokenInfo(token, permissionRequirement.Expiration.TotalSeconds,
            JwtBearerDefaults.AuthenticationScheme);
    }

    public double GetTokenExpirationSeconds()
    {
        return permissionRequirement.Expiration.TotalSeconds;
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

    public IList<Claim> GetClaimsFromUserContext(IUserContext<TId> userContext)
    {
        throw new NotImplementedException();
    }

    public bool VerifyToken(string token)
    {
        var key = configuration["AUDIENCE_KEY"];
        var keyBuffer = Encoding.ASCII.GetBytes(key!);
        var signingKey = new SymmetricSecurityKey(keyBuffer);
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var jwt = jwtSecurityTokenHandler.ReadJwtToken(token);
        return jwt.RawSignature ==
               JwtTokenUtilities.CreateEncodedSignature(jwt.RawHeader + "." + jwt.RawPayload, signingCredentials);
    }
}