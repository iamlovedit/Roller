namespace Roller.Infrastructure.Security;

public class RollerTokenHandler(
    IAesEncryptionService aesEncryptionService,
    JwtSecurityTokenHandler jwtSecurityTokenHandler)
    : TokenHandler
{
    public override Task<TokenValidationResult> ValidateTokenAsync(string token,
        TokenValidationParameters validationParameters)
    {
        var decodeToken = aesEncryptionService.Decrypt(token);
        return jwtSecurityTokenHandler.ValidateTokenAsync(decodeToken, validationParameters);
    }
}