namespace Roller.Infrastructure.Security
{
    public class JwtBearerOptionsPostConfigureOptions(
        RollerTokenHandler rollerTokenHandler)
        : IPostConfigureOptions<JwtBearerOptions>
    {
        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            options.TokenHandlers.Clear();
            options.TokenHandlers.Add(rollerTokenHandler);
        }
    }
}