using ErrorOr;

namespace MyModularMonolith.Modules.AI.Services;

public interface IAIService
{
    Task<ErrorOr<string>> GenerateResponseAsync(
        string prompt,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<T>> GenerateStructuredResponseAsync<T>(
        string prompt,
        CancellationToken cancellationToken = default);
}