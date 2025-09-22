using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Shared.Infrastructure;


public class EfRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T> where T : class
{
    public EfRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
