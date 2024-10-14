﻿using Microsoft.Extensions.Primitives;
using SqlSugar.Extensions;

namespace Roller.Infrastructure.Security
{
    public class JwtBearerOptionsPostConfigureOptions(
        RollerTokenHandler rollerTokenHandler,
        JwtSecurityTokenHandler jwtSecurityTokenHandler)
        : IPostConfigureOptions<JwtBearerOptions>
    {
        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            options.TokenHandlers.Clear();
            options.TokenHandlers.Add(rollerTokenHandler);
            options.Events.OnAuthenticationFailed = failedContext =>
            {
                var token = failedContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(token) || !jwtSecurityTokenHandler.CanReadToken(token))
                {
                    failedContext.Response.Headers.Append(
                        new KeyValuePair<string, StringValues>("token-error", "can't get token"));
                    return Task.CompletedTask;
                }

                if (failedContext.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    failedContext.Response.Headers.Append(
                        new KeyValuePair<string, StringValues>("token-expired", "true"));
                    return Task.CompletedTask;
                }

                if (jwtSecurityTokenHandler.CanReadToken(token))
                {
                    try
                    {
                        var jwtToken = jwtSecurityTokenHandler.ReadJwtToken(token);
                        if (jwtToken.Issuer != options.TokenValidationParameters.ValidIssuer)
                        {
                            failedContext.Response.Headers.Append(
                                new KeyValuePair<string, StringValues>("token-error-issuer", "issuer is wrong"));
                            return Task.CompletedTask;
                        }

                        if (jwtToken.Audiences.FirstOrDefault() != options.TokenValidationParameters.ValidIssuer)
                        {
                            failedContext.Response.Headers.Append(
                                new KeyValuePair<string, StringValues>("token-error-audience", "audience is wrong!"));
                            return Task.CompletedTask;
                        }
                    }
                    catch (Exception)
                    {
                        failedContext.Response.Headers.Append(
                            new KeyValuePair<string, StringValues>("token-error-format", "token format is wrong!"));
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    failedContext.Response.Headers.Append(
                        new KeyValuePair<string, StringValues>("token-error-format", "token format is wrong!"));
                    return Task.CompletedTask;
                }

                return Task.CompletedTask;
            };
        }
    }
}