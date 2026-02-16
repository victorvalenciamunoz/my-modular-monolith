using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MyModularMonolith.Modules.Users.Application.Security;

public interface ILoginAttemptService
{
    /// <summary>
    /// Verifica si una cuenta está bloqueada por demasiados intentos fallidos.
    /// </summary>
    Task<bool> IsAccountLockedAsync(string email, DateTime currentTime);

    /// <summary>
    /// Registra un intento de login fallido para una cuenta.
    /// Retorna true si la cuenta quedó bloqueada después de este intento.
    /// </summary>
    Task<bool> RecordFailedAttemptAsync(string email, DateTime currentTime);

    /// <summary>
    /// Registra un login exitoso y limpia los intentos fallidos previos.
    /// </summary>
    Task RecordSuccessfulLoginAsync(string email);

    /// <summary>
    /// Obtiene información sobre el estado de intentos de una cuenta.
    /// </summary>
    Task<LoginAttemptInfo?> GetAttemptInfoAsync(string email);
}

/// <summary>
/// Servicio de tracking de intentos de login en memoria.
/// TODO: [REDIS-MIGRATION] Reemplazar ConcurrentDictionary con IDistributedCache cuando Redis esté disponible.
/// 
/// Implementación actual:
/// - Almacenamiento en memoria (ConcurrentDictionary)
/// - Limpieza automática de entradas antiguas
/// - Solo funciona en single-instance
/// 
/// Migración a Redis:
/// 1. Inyectar IDistributedCache en lugar del ConcurrentDictionary
/// 2. Usar keys como: "login:attempts:{email}"
/// 3. Configurar TTL automático (15 minutos)
/// 4. Considerar usar Redis Lua scripts para operaciones atómicas
/// </summary>
public class InMemoryLoginAttemptService : ILoginAttemptService
{
    // TODO: [REDIS-MIGRATION] Reemplazar con IDistributedCache
    private readonly ConcurrentDictionary<string, LoginAttemptInfo> _attempts = new();
    
    private readonly LoginRateLimitOptions _options;
    private readonly ILogger<InMemoryLoginAttemptService> _logger;
    private DateTime _lastCleanup = DateTime.UtcNow;

    public InMemoryLoginAttemptService(
        IOptions<LoginRateLimitOptions> options,
        ILogger<InMemoryLoginAttemptService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<bool> IsAccountLockedAsync(string email, DateTime currentTime)
    {
        CleanupOldEntriesIfNeeded(currentTime);

        if (!_attempts.TryGetValue(email.ToLowerInvariant(), out var info))
        {
            return Task.FromResult(false);
        }

        var isLocked = info.IsLocked(currentTime);
        
        if (isLocked)
        {
            var remainingTime = info.LockedUntil!.Value - currentTime;
            _logger.LogWarning(
                "Account {Email} is locked. Remaining time: {RemainingMinutes:F1} minutes",
                email, remainingTime.TotalMinutes);
        }

        return Task.FromResult(isLocked);
    }

    public Task<bool> RecordFailedAttemptAsync(string email, DateTime currentTime)
    {
        var key = email.ToLowerInvariant();
        
        var info = _attempts.GetOrAdd(key, _ => new LoginAttemptInfo());
        
        info.RecordFailedAttempt(currentTime);

        _logger.LogWarning(
            "Failed login attempt for {Email}. Total attempts: {FailedAttempts}",
            email, info.FailedAttempts);

        // Si alcanzó el máximo de intentos, aplicar lockout
        if (info.FailedAttempts >= _options.MaxFailedAttempts && !info.IsLocked(currentTime))
        {
            info.ApplyLockout(currentTime, _options.LockoutDuration);
            
            _logger.LogWarning(
                "Account {Email} locked after {FailedAttempts} failed attempts. Locked until {LockedUntil:u}",
                email, info.FailedAttempts, info.LockedUntil);

            return Task.FromResult(true); // Cuenta bloqueada
        }

        return Task.FromResult(false); // No bloqueada aún
    }

    public Task RecordSuccessfulLoginAsync(string email)
    {
        var key = email.ToLowerInvariant();
        
        if (_attempts.TryGetValue(key, out var info))
        {
            info.RecordSuccessfulLogin();
            _logger.LogDebug("Cleared failed attempts for {Email} after successful login", email);
        }

        return Task.CompletedTask;
    }

    public Task<LoginAttemptInfo?> GetAttemptInfoAsync(string email)
    {
        var key = email.ToLowerInvariant();
        _attempts.TryGetValue(key, out var info);
        return Task.FromResult(info);
    }

    /// <summary>
    /// Limpia entradas antiguas del diccionario para prevenir memory leaks.
    /// TODO: [REDIS-MIGRATION] No necesario con Redis (usa TTL automático)
    /// </summary>
    private void CleanupOldEntriesIfNeeded(DateTime currentTime)
    {
        // Solo limpiar cada X tiempo para no impactar performance
        if (currentTime - _lastCleanup < _options.CleanupInterval)
        {
            return;
        }

        _lastCleanup = currentTime;

        var keysToRemove = _attempts
            .Where(kvp =>
            {
                var info = kvp.Value;
                // Remover si:
                // 1. No está bloqueada Y no tiene intentos fallidos
                // 2. El lockout ya expiró hace más de 1 hora
                var shouldRemove = (!info.IsLocked(currentTime) && info.FailedAttempts == 0) ||
                                   (info.LockedUntil.HasValue && info.LockedUntil.Value.AddHours(1) < currentTime);
                return shouldRemove;
            })
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _attempts.TryRemove(key, out _);
        }

        if (keysToRemove.Count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} old login attempt entries", keysToRemove.Count);
        }
    }
}
