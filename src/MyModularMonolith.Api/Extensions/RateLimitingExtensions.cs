using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Threading.RateLimiting;

namespace MyModularMonolith.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Rate Limiting por IP (Protección contra fuerza bruta desde una IP)
            // Política para el endpoint de login: máximo 20 intentos por minuto por IP
            options.AddFixedWindowLimiter("login", opt =>
            {
                opt.PermitLimit = 20;                     // 20 requests por ventana
                opt.Window = TimeSpan.FromMinutes(1);     // Ventana de 1 minuto
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;                       // Sin cola (rechazar inmediatamente)
            });

            // Respuesta personalizada cuando se excede el límite
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                Log.Warning(
                    "Rate limit exceeded for IP {IpAddress} on endpoint {Endpoint}. Lease: {Lease}",
                    clientIp,
                    context.HttpContext.Request.Path,
                    context.Lease.GetAllMetadata());

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "TooManyRequests",
                        message = "Too many login attempts. Please try again later.",
                        retryAfter = $"{retryAfter.TotalSeconds} seconds"
                    }, cancellationToken);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "TooManyRequests",
                        message = "Too many login attempts. Please try again later."
                    }, cancellationToken);
                }
            };
        });

        return services;
    }
}
