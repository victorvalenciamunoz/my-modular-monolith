using System.Collections.Concurrent;

namespace MyModularMonolith.AdminUI.Authentication.Services;

public interface ITokenService
{
    Task SetTokensAsync(string sessionId, string accessToken, string refreshToken, DateTime expiresAt);
    Task<TokenData?> GetTokensAsync(string sessionId);
    Task RemoveTokensAsync(string sessionId);
    Task<bool> IsSessionValidAsync(string sessionId);
}

public class TokenService : ITokenService
{
    private readonly ConcurrentDictionary<string, TokenData> _tokens = new();

    public Task SetTokensAsync(string sessionId, string accessToken, string refreshToken, DateTime expiresAt)
    {
        _tokens[sessionId] = new TokenData(accessToken, refreshToken, expiresAt);
        return Task.CompletedTask;
    }

    public Task<TokenData?> GetTokensAsync(string sessionId)
    {
        _tokens.TryGetValue(sessionId, out var tokenData);
        return Task.FromResult(tokenData);
    }

    public Task RemoveTokensAsync(string sessionId)
    {
        _tokens.TryRemove(sessionId, out _);
        return Task.CompletedTask;
    }

    public Task<bool> IsSessionValidAsync(string sessionId)
    {
        if (!_tokens.TryGetValue(sessionId, out var tokenData))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(tokenData.ExpiresAt > DateTime.UtcNow);
    }
}

public record TokenData(string AccessToken, string RefreshToken, DateTime ExpiresAt);
