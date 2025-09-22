using Ardalis.Specification;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Shared.Application;
using MyModularMonolith.Shared.Infrastructure;

namespace MyModularMonolith.Modules.Users.Infrastructure;

internal class RefreshTokenRepository : EfRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenRepository(UsersDbContext context, IDateTimeProvider dateTimeProvider) : base(context)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task RevokeAllBySpecificationAsync(ISpecification<RefreshToken> specification, CancellationToken cancellationToken = default)
    {
        var tokens = await ListAsync(specification, cancellationToken);
        var currentTime = _dateTimeProvider.UtcNow;

        foreach (var token in tokens)
        {
            token.Revoke(currentTime);
        }
    }
}
