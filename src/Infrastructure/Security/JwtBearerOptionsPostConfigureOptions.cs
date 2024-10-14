using Microsoft.Extensions.Primitives;
using SqlSugar.Extensions;

namespace Roller.Infrastructure.Security
{
    public class JwtBearerOptionsPostConfigureOptions(
        RollerTokenHandler rollerTokenHandler,
        ITokenBuilder tokenBuilder)
        : IPostConfigureOptions<JwtBearerOptions>
    {
        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            options.TokenHandlers.Clear();
            options.TokenHandlers.Add(rollerTokenHandler);
            options.Events.OnTokenValidated = tokenBuilder.SetUserContext;
        }
    }
}