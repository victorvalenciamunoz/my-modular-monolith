using System.Collections.Concurrent;

namespace MyModularMonolith.Modules.Users.Application.Security;

/// <summary>
/// Información de intentos de login para una cuenta de usuario.
/// TODO: Migrar a Redis cuando esté disponible para soporte multi-instancia.
/// </summary>
public class LoginAttemptInfo
{
    public int FailedAttempts { get; set; }
    public DateTime LastAttemptAt { get; set; }
    public DateTime? LockedUntil { get; set; }
    
    public bool IsLocked(DateTime currentTime)
    {
        return LockedUntil.HasValue && LockedUntil.Value > currentTime;
    }

    public void RecordFailedAttempt(DateTime currentTime)
    {
        FailedAttempts++;
        LastAttemptAt = currentTime;
    }

    public void RecordSuccessfulLogin()
    {
        FailedAttempts = 0;
        LockedUntil = null;
    }

    public void ApplyLockout(DateTime currentTime, TimeSpan lockoutDuration)
    {
        LockedUntil = currentTime.Add(lockoutDuration);
    }
}

/// <summary>
/// Configuración de rate limiting para login.
/// Estos valores se pueden mover a appsettings.json cuando sea necesario.
/// </summary>
public class LoginRateLimitOptions
{
    public int MaxFailedAttempts { get; set; } = 5; // Máximo de intentos fallidos antes de lockout
    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15); // Duración del lockout
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1); // Cada cuánto limpiar entradas antiguas
}
