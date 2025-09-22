using Microsoft.EntityFrameworkCore;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Shared.Application;
using MyModularMonolith.Shared.Infrastructure;

namespace MyModularMonolith.Modules.Users.Infrastructure;

internal class RefreshTokenRepository : EfRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly UsersDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenRepository(UsersDbContext context, IDateTimeProvider dateTimeProvider) : base(context)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var currentTime = _dateTimeProvider.UtcNow;
        return await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAt > currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        var currentTime = _dateTimeProvider.UtcNow;
        foreach (var token in tokens)
        {
            token.Revoke(currentTime);
        }
    }
}