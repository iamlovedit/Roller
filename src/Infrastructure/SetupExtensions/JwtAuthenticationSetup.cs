using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Roller.Infrastructure.Options;
using Roller.Infrastructure.Security;

namespace Roller.Infrastructure.SetupExtensions;

public static class JwtAuthenticationSetup
{
    public static void AddJwtAuthenticationSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var audienceOptions = configuration.GetSection(AudienceOptions.Name).Get<AudienceOptions>();
        if (audienceOptions?.Enable ?? false)
        {
            var key = configuration["AUDIENCE_KEY"] ?? audienceOptions.Secret;
            ArgumentException.ThrowIfNullOrEmpty(key);
            var buffer = Encoding.UTF8.GetBytes(key);
            var securityKey = new SymmetricSecurityKey(buffer);
            
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidIssuer = audienceOptions.Issuer,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = audienceOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(15),
                RequireExpirationTime = true,
                RoleClaimType = ClaimTypes.Role
            };
            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = nameof(RollerAuthenticationHandler);
                options.DefaultForbidScheme = nameof(RollerAuthenticationHandler);
            }).AddScheme<AuthenticationSchemeOptions, RollerAuthenticationHandler>(nameof(RollerAuthenticationHandler),
                options => { }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents()
                {
                    OnChallenge = challengeContext =>
                    {
                        challengeContext.Response.Headers.Append(new KeyValuePair<string, StringValues>("token-error", challengeContext.ErrorDescription));
                        return Task.CompletedTask;
                    },
                };
            });
        }
    }
}