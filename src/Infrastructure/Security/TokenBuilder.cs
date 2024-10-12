using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Roller.Infrastructure.Security;

public class TokenBuilder(
    IAesEncryptionService aesEncryptionService,
    PermissionRequirement permissionRequirement,
    IConfiguration configuration)
    : ITokenBuilder
{
    public string DecryptCipherToken(string cipherToken)
    {
        if (string.IsNullOrEmpty(cipherToken))
        {
            throw new ArgumentException($"{nameof(cipherToken)} is null or empty。", nameof(cipherToken));
        }

        return aesEncryptionService.Decrypt(cipherToken);
    }

    public TokenInfo GenerateTokenInfo(IReadOnlyCollection<Claim> claims)
    {
        var jwtToken = new JwtSecurityToken(
            issuer: permissionRequirement.Issuer,
            audience: permissionRequirement.Audience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.Add(permissionRequirement.Expiration),
            signingCredentials: permissionRequirement.SigningCredentials);
        var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        token = aesEncryptionService.Encrypt(token);
        return new TokenInfo(token, permissionRequirement.Expiration.TotalSeconds, JwtBearerDefaults.AuthenticationScheme);
    }

    public double GetTokenExpirationSeconds()
    {
        return permissionRequirement.Expiration.TotalSeconds;
    }

    public long ParseUIdFromToken(string token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        if (jwtHandler.CanReadToken(token))
        {
            var jwtToken = jwtHandler.ReadJwtToken(token);
            if (long.TryParse(jwtToken.Id, out var id))
            {
                return id;
            }
        }
        return 0;
    }

    public bool VerifyToken(string token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        var key = configuration["AUDIENCE_KEY"];
        var keyBuffer = Encoding.ASCII.GetBytes(key!);
        var signingKey = new SymmetricSecurityKey(keyBuffer);
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var jwt = jwtHandler.ReadJwtToken(token);
        return jwt.RawSignature == JwtTokenUtilities.CreateEncodedSignature(jwt.RawHeader + "." + jwt.RawPayload, signingCredentials);
    }
}