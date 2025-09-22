using Ardalis.Specification;

namespace MyModularMonolith.Modules.Users.Domain.Specifications;

public static class RefreshTokenSpecs
{
    public class ByToken : Specification<RefreshToken>
    {
        public ByToken(string token)
        {
            Query.Where(x => x.Token == token);
        }
    }

    public class ByUserId : Specification<RefreshToken>
    {
        public ByUserId(Guid userId)
        {
            Query.Where(x => x.UserId == userId);
        }
    }

    public class NotRevoked : Specification<RefreshToken>
    {
        public NotRevoked()
        {
            Query.Where(x => !x.IsRevoked);
        }
    }

    public class NotExpired : Specification<RefreshToken>
    {
        public NotExpired(DateTime currentTime)
        {
            Query.Where(x => x.ExpiresAt > currentTime);
        }
    }

    public class WithUser : Specification<RefreshToken>
    {
        public WithUser()
        {
            Query.Include(x => x.User);
        }
    }

    public class Active : Specification<RefreshToken>
    {
        public Active(DateTime currentTime)
        {
            Query.Where(x => !x.IsRevoked && x.ExpiresAt > currentTime);
        }
    }
        
    public class ByTokenWithUser : Specification<RefreshToken>
    {
        public ByTokenWithUser(string token)
        {
            Query
                .Where(x => x.Token == token)
                .Include(x => x.User);
        }
    }

    public class ActiveByTokenWithUser : Specification<RefreshToken>
    {
        public ActiveByTokenWithUser(string token, DateTime currentTime)
        {
            Query
                .Where(x => x.Token == token && !x.IsRevoked && x.ExpiresAt > currentTime)
                .Include(x => x.User);
        }
    }

    public class NotRevokedByUserId : Specification<RefreshToken>
    {
        public NotRevokedByUserId(Guid userId)
        {
            Query.Where(x => x.UserId == userId && !x.IsRevoked);
        }
    }
}