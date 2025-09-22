namespace MyModularMonolith.Shared.Application;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
