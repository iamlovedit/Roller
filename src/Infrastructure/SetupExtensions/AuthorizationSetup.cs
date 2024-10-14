using Roller.Infrastructure.Options;

namespace Roller.Infrastructure.SetupExtensions
{
    public static class AuthorizationSetup
    {
        public static IServiceCollection AddAuthorizationSetup(this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);

            ArgumentNullException.ThrowIfNull(configuration);

            var audienceOptions = configuration.GetSection(AudienceOptions.Name).Get<AudienceOptions>();
            if (audienceOptions is null || !audienceOptions.Enable)
            {
                return services;
            }

            var key = configuration["AUDIENCE_KEY"] ?? audienceOptions.Secret;
            ArgumentException.ThrowIfNullOrEmpty(key);
            var buffer = Encoding.UTF8.GetBytes(key);
            var securityKey = new SymmetricSecurityKey(buffer);
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            services.AddSingleton(new JwtOptions(ClaimTypes.Role, audienceOptions.Issuer,
                audienceOptions.Audience,
                TimeSpan.FromSeconds(audienceOptions.Expiration), signingCredentials));
            services.AddAuthorizationBuilder()
                .AddPolicy(PermissionConstants.PolicyName,
                    policy => policy.RequireRole(
                            PermissionConstants.Consumer, PermissionConstants.Administrator,
                            PermissionConstants.SuperAdministrator)
                        .Build());
            return services;
        }
    }
}