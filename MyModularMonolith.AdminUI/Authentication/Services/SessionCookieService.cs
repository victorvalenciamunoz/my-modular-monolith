using Microsoft.AspNetCore.Http;

namespace MyModularMonolith.AdminUI.Authentication.Services;

public interface ISessionCookieService
{
    void SetSessionId(string sessionId);
    string? GetSessionId();
    void RemoveSessionId();
}

public class SessionCookieService : ISessionCookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string SessionCookieName = "AdminUI.SessionId";
    
    // Cache en memoria para el circuito actual
    private string? _cachedSessionId;

    public SessionCookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetSessionId(string sessionId)
    {
        _cachedSessionId = sessionId;
        
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Response.Cookies.Append(SessionCookieName, sessionId, new CookieOptions
            {
                HttpOnly = true,          // ✅ No accesible desde JavaScript
                Secure = true,            // ✅ Solo HTTPS
                SameSite = SameSiteMode.Strict, // ✅ Protección CSRF
                MaxAge = TimeSpan.FromHours(8),  // 8 horas de vida
                IsEssential = true        // ✅ Cookie esencial (no requiere consentimiento GDPR)
            });
        }
    }

    public string? GetSessionId()
    {
        // Primero intentar leer del cache
        if (!string.IsNullOrEmpty(_cachedSessionId))
        {
            return _cachedSessionId;
        }

        // Si no está en cache, leer de la cookie
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Request.Cookies.TryGetValue(SessionCookieName, out var sessionId))
        {
            _cachedSessionId = sessionId;
            return sessionId;
        }

        return null;
    }

    public void RemoveSessionId()
    {
        _cachedSessionId = null;
        
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Response.Cookies.Delete(SessionCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}
