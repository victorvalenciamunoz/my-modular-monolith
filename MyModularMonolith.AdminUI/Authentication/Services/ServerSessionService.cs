using System.Collections.Concurrent;

namespace MyModularMonolith.AdminUI.Authentication.Services;

public interface IServerSessionService
{
    string CreateSession(UserSession userSession);
    Task<UserSession?> GetSessionAsync(string sessionId);
    Task RemoveSessionAsync(string sessionId);
    Task<bool> IsSessionValidAsync(string sessionId);
}

public class ServerSessionService : IServerSessionService
{
    private readonly ConcurrentDictionary<string, UserSession> _sessions = new();

    public string CreateSession(UserSession userSession)
    {
        var sessionId = Guid.NewGuid().ToString();
        _sessions[sessionId] = userSession;
        return sessionId;
    }

    public Task<UserSession?> GetSessionAsync(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public Task RemoveSessionAsync(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
        return Task.CompletedTask;
    }

    public Task<bool> IsSessionValidAsync(string sessionId)
    {
        return Task.FromResult(_sessions.ContainsKey(sessionId));
    }
}

public record UserSession(string UserName, string Email, string Role);
