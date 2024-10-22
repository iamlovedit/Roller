namespace Roller.Infrastructure.Security
{
    public class JwtOptions(
        string claimType,
        string issuer,
        string audience,
        int duration,
        SigningCredentials credentials)
    {
        public string ClaimType { get; } = claimType;

        public string Issuer { get; } = issuer;

        public string Audience { get; } = audience;

        public int Duration { get; } = duration;

        public SigningCredentials SigningCredentials { get; } = credentials;
    }
}
