using System.Net.Http.Headers;
using System.Text.Json;
using MyModularMonolith.AdminUI.Authentication.Models;
using MyModularMonolith.AdminUI.Authentication.Services;

namespace MyModularMonolith.AdminUI.Authentication.Handlers;

public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly ISessionCookieService _sessionCookieService;
    private readonly ITokenService _tokenService;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthenticatedHttpClientHandler(
        ISessionCookieService sessionCookieService,
        ITokenService tokenService,
        IHttpClientFactory httpClientFactory)
    {
        _sessionCookieService = sessionCookieService;
        _tokenService = tokenService;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Obtener sessionId de la cookie HTTP
        var sessionId = _sessionCookieService.GetSessionId();

        if (string.IsNullOrEmpty(sessionId))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var tokenData = await _tokenService.GetTokensAsync(sessionId);

        if (tokenData == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // Verificar si el token ha expirado
        if (tokenData.ExpiresAt <= DateTime.UtcNow.AddMinutes(5)) // Renovar 5 min antes de expirar
        {
            tokenData = await RefreshTokenAsync(sessionId, tokenData.RefreshToken);
            if (tokenData == null)
            {
                // No se pudo renovar, eliminar sesión
                _sessionCookieService.RemoveSessionId();
                await _tokenService.RemoveTokensAsync(sessionId);
                return await base.SendAsync(request, cancellationToken);
            }
        }

        // Agregar AccessToken al header
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<TokenData?> RefreshTokenAsync(string sessionId, string refreshToken)
    {
        try
        {
            // Crear un cliente HTTP sin este handler para evitar recursión
            var httpClient = _httpClientFactory.CreateClient("BackendClient");

            var refreshRequest = new { RefreshToken = refreshToken };
            var response = await httpClient.PostAsJsonAsync("api/auth/refresh", refreshRequest);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                if (authResponse?.AccessToken != null && authResponse.RefreshToken != null)
                {
                    var expiresAt = DateTime.UtcNow.AddSeconds(authResponse.RefreshTokenExpires);
                    var newTokenData = new TokenData(
                        authResponse.AccessToken,
                        authResponse.RefreshToken,
                        expiresAt);

                    await _tokenService.SetTokensAsync(sessionId, 
                        authResponse.AccessToken, 
                        authResponse.RefreshToken, 
                        expiresAt);

                    return newTokenData;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
