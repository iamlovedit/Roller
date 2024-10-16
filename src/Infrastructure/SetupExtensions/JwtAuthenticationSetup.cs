﻿using Microsoft.AspNetCore.Authentication;
using Roller.Infrastructure.Options;

namespace Roller.Infrastructure.SetupExtensions;

public static class JwtAuthenticationSetup
{
    public static IServiceCollection AddJwtAuthenticationSetup(this IServiceCollection services,
        IConfiguration configuration, Action<AuthenticationBuilder>? builderAction = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var audienceOptions = configuration.GetSection(AudienceOptions.Name).Get<AudienceOptions>();
        if (audienceOptions is null || !audienceOptions.Enable)
        {
            return services;
        }

        services.TryAddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerOptionsPostConfigureOptions>();
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
            ClockSkew = TimeSpan.FromSeconds(300),
            RequireExpirationTime = true,
            RoleClaimType = ClaimTypes.Role
        };

        var builder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = nameof(RollerAuthenticationHandler);
                options.DefaultForbidScheme = nameof(RollerAuthenticationHandler);
            }).AddScheme<AuthenticationSchemeOptions, RollerAuthenticationHandler>(nameof(RollerAuthenticationHandler),
                options => { })
            .AddJwtBearer(options => { options.TokenValidationParameters = tokenValidationParameters; });
        builderAction?.Invoke(builder);

        return services;
    }
}