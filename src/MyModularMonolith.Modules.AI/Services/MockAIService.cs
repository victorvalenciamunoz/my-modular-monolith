using ErrorOr;
using Microsoft.Extensions.Logging;

namespace MyModularMonolith.Modules.AI.Services;

internal class MockAIService : IAIService
{
    private readonly ILogger<MockAIService> _logger;

    public MockAIService(ILogger<MockAIService> logger)
    {
        _logger = logger;
    }

    public async Task<ErrorOr<string>> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock AI called with prompt: {Prompt}", prompt.Substring(0, Math.Min(100, prompt.Length)));

        // Simular delay de IA
        await Task.Delay(500, cancellationToken);

        // Respuesta mock inteligente basada en el contenido del prompt
        if (prompt.Contains("recommendations"))
        {
            return "Based on the reservation analysis, I recommend confirming the first 80% of reservations based on capacity, placing remaining in waiting list, and considering user registration time for priority.";
        }

        if (prompt.Contains("summary"))
        {
            return "Class shows healthy demand with manageable overbooking. Recommend selective confirmation based on capacity optimization.";
        }

        return "AI analysis completed. The reservation pattern suggests optimal management through selective confirmation strategy.";
    }

    public async Task<ErrorOr<T>> GenerateStructuredResponseAsync<T>(string prompt, CancellationToken cancellationToken = default)
    {
        var response = await GenerateResponseAsync(prompt, cancellationToken);

        if (response.IsError)
        {
            return response.Errors;
        }

        try
        {
            // Para mocks, retornamos un objeto por defecto
            var defaultValue = Activator.CreateInstance<T>();
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating mock response for type {Type}", typeof(T).Name);
            return Error.Failure("AI.MockError", "Failed to create mock response");
        }
    }
}