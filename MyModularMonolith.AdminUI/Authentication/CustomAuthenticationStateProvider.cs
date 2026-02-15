using Microsoft.AspNetCore.Components.Authorization;
using MyModularMonolith.AdminUI.Authentication.Services;
using System.Security.Claims;

namespace MyModularMonolith.AdminUI.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ISessionCookieService _sessionCookieService;
    private readonly IServerSessionService _serverSessionService;
    private readonly ITokenService _tokenService;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ISessionCookieService sessionCookieService,
        IServerSessionService serverSessionService,
        ITokenService tokenService)
    {
        _sessionCookieService = sessionCookieService;
        _serverSessionService = serverSessionService;
        _tokenService = tokenService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Obtener sessionId de la cookie HTTP
            var sessionId = _sessionCookieService.GetSessionId();

            if (string.IsNullOrEmpty(sessionId))
            {
                return new AuthenticationState(_anonymous);
            }

            // Validar que la sesión y tokens aún sean válidos
            var isSessionValid = await _serverSessionService.IsSessionValidAsync(sessionId);
            var isTokenValid = await _tokenService.IsSessionValidAsync(sessionId);

            if (!isSessionValid || !isTokenValid)
            {
                // Sesión expirada, limpiar
                _sessionCookieService.RemoveSessionId();
                await _serverSessionService.RemoveSessionAsync(sessionId);
                await _tokenService.RemoveTokensAsync(sessionId);
                return new AuthenticationState(_anonymous);
            }

            // Obtener datos de sesión del servidor
            var userSession = await _serverSessionService.GetSessionAsync(sessionId);
            if (userSession == null)
            {
                return new AuthenticationState(_anonymous);
            }

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, userSession.UserName),
                new(ClaimTypes.Email, userSession.Email),
                new(ClaimTypes.Role, userSession.Role),
                new("SessionId", sessionId) // Agregar SessionId como claim
            }, "SecureAuth"));

            return new AuthenticationState(claimsPrincipal);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task<string> UpdateAuthenticationStateAsync(
        UserSession? userSession, 
        string? accessToken = null, 
        string? refreshToken = null, 
        DateTime? expiresAt = null)
    {
        ClaimsPrincipal claimsPrincipal;
        string? sessionId = null;

        if (userSession != null)
        {
            // Crear nueva sesión en el servidor
            sessionId = _serverSessionService.CreateSession(userSession);

            // Guardar sessionId en cookie HTTP HttpOnly + Secure
            _sessionCookieService.SetSessionId(sessionId);

            // Guardar tokens si se proporcionaron
            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken) && expiresAt.HasValue)
            {
                await _tokenService.SetTokensAsync(sessionId, accessToken, refreshToken, expiresAt.Value);
            }

            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, userSession.UserName),
                new(ClaimTypes.Email, userSession.Email),
                new(ClaimTypes.Role, userSession.Role),
                new("SessionId", sessionId)
            }, "SecureAuth"));
        }
        else
        {
            // Logout: limpiar sesión
            var existingSessionId = _sessionCookieService.GetSessionId();
            if (!string.IsNullOrEmpty(existingSessionId))
            {
                await _serverSessionService.RemoveSessionAsync(existingSessionId);
                await _tokenService.RemoveTokensAsync(existingSessionId);
            }

            _sessionCookieService.RemoveSessionId();
            claimsPrincipal = _anonymous;
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        return sessionId ?? string.Empty;
    }
}
