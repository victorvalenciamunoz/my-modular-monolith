﻿using Ardalis.Specification;

namespace MyModularMonolith.Shared.Application;

public interface IRepository<T> : IRepositoryBase<T> where T : class
{
}

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class
{
}
