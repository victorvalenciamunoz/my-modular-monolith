using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyModularMonolith.Modules.AI.Configuration;
using System.Text.Json;

namespace MyModularMonolith.Modules.AI.Services;

internal class GeminiAIService : IAIService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<GeminiAIService> _logger;
    private readonly AIConfiguration _config;

    public GeminiAIService(
        IChatClient chatClient,
        ILogger<GeminiAIService> logger,
        IOptions<AIConfiguration> config)
    {
        _chatClient = chatClient;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<ErrorOr<string>> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling Gemini AI with prompt length: {Length} characters", prompt.Length);

            var messages = new List<ChatMessage>
        {
            new(ChatRole.System, CreateSystemPrompt()),
            new(ChatRole.User, prompt)
        };

            var options = new ChatOptions
            {
                MaxOutputTokens = _config.Gemini.MaxTokens,
                Temperature = (float)_config.Gemini.Temperature,
                ModelId = _config.Gemini.ModelName
            };

            var response = await _chatClient.GetResponseAsync(messages, options, cancellationToken);

            var responseText = response.Text;
            if (string.IsNullOrWhiteSpace(responseText))
            {
                _logger.LogWarning("Gemini returned empty response");
                return Error.Failure("AI.EmptyResponse", "AI service returned empty response");
            }

            _logger.LogInformation("Gemini response received successfully. Length: {Length} characters",
                responseText.Length);

            return responseText;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Gemini AI request was cancelled");
            return Error.Failure("AI.Cancelled", "AI request was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini AI service");
            return Error.Failure("AI.ServiceError", $"Failed to generate AI response: {ex.Message}");
        }
    }

    public async Task<ErrorOr<T>> GenerateStructuredResponseAsync<T>(string prompt, CancellationToken cancellationToken = default)
    {
        // Añadir instrucciones para respuesta estructurada JSON
        var structuredPrompt = $"""
        {prompt}
        
        Please provide your response as a valid JSON object that can be deserialized to the following C# type:
        {typeof(T).Name}
        
        Ensure the JSON is properly formatted and contains all required properties.
        Only return the JSON object, no additional text or markdown formatting.
        """;

        var response = await GenerateResponseAsync(structuredPrompt, cancellationToken);

        if (response.IsError)
        {
            return response.Errors;
        }

        try
        {
            // Limpiar la respuesta de posible markdown o texto extra
            var jsonText = CleanJsonResponse(response.Value);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = JsonSerializer.Deserialize<T>(jsonText, options);

            if (result == null)
            {
                _logger.LogError("Deserialization resulted in null for type {Type}", typeof(T).Name);
                return Error.Failure("AI.DeserializationError", "Failed to deserialize AI response");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing AI response to {Type}. Response: {Response}",
                typeof(T).Name, response.Value.Substring(0, Math.Min(200, response.Value.Length)));
            return Error.Failure("AI.DeserializationError", $"Failed to parse AI response as {typeof(T).Name}");
        }
    }

    private string CreateSystemPrompt()
    {
        return """
        You are an expert AI assistant specialized in gym and fitness reservation management.
        
        Your expertise includes:
        - Analyzing reservation patterns and user behavior
        - Optimizing class capacity and scheduling
        - Predicting attendance probabilities
        - Recommending management strategies
        - Identifying business optimization opportunities
        
        Always provide:
        - Clear, actionable insights
        - Data-driven recommendations
        - Consideration for both business efficiency and customer satisfaction
        - Practical solutions for gym management
        
        Be concise but thorough. Focus on practical, implementable advice.
        """;
    }

    private static string CleanJsonResponse(string response)
    {
        // Remover posibles markdown code blocks
        var cleaned = response.Trim();

        if (cleaned.StartsWith("```json"))
        {
            cleaned = cleaned.Substring(7);
        }
        else if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.Substring(3);
        }

        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3);
        }

        return cleaned.Trim();
    }
}
