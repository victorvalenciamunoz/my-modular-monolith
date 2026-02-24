using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace MyModularMonolith.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtSecret = configuration["JWT:Secret"];
        var jwtIssuer = configuration["JWT:Issuer"];
        var jwtAudience = configuration["JWT:Audience"];

        Log.Information("JWT Configuration: Issuer={Issuer}, Audience={Audience}, SecretLength={SecretLength}",
            jwtIssuer, jwtAudience, jwtSecret?.Length ?? 0);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Error("Authentication failed: {Error}", context.Exception.Message);
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        Log.Error("Token has expired");
                    }
                    else if (context.Exception is SecurityTokenInvalidSignatureException)
                    {
                        Log.Error("Token signature is invalid - Secret mismatch!");
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Log.Information("✅ Token validated successfully for user: {User}",
                        context.Principal?.Identity?.Name ?? "Unknown");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Log.Warning("⚠️ Authentication challenge: Error={Error}, ErrorDescription={ErrorDescription}",
                        context.Error ?? "null", context.ErrorDescription ?? "null");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        Log.Debug("📩 Authorization header received: {HeaderStart}...",
                            authHeader.Length > 50 ? authHeader.Substring(0, 50) : authHeader);
                    }
                    else
                    {
                        Log.Warning("⚠️ No Authorization header found in request");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
