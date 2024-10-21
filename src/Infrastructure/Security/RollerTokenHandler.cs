namespace Roller.Infrastructure.Security;

public class RollerTokenHandler(
    IAesEncryptionService aesEncryptionService,
    JsonWebTokenHandler jsonWebTokenHandler)
    : TokenHandler
{
    public override Task<TokenValidationResult> ValidateTokenAsync(string token,
        TokenValidationParameters validationParameters)
    {
        var decodeToken = aesEncryptionService.Decrypt(token);
        return jsonWebTokenHandler.ValidateTokenAsync(decodeToken, validationParameters);
    }
}