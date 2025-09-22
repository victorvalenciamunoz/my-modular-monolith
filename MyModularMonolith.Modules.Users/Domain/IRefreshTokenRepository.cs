using Ardalis.Specification;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Modules.Users.Domain;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{    
    Task RevokeAllBySpecificationAsync(ISpecification<RefreshToken> specification, CancellationToken cancellationToken = default);
}
