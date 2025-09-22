using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.AI.Agents;
using MyModularMonolith.Modules.AI.Configuration;
using MyModularMonolith.Modules.AI.Presentation.Endpoints;
using MyModularMonolith.Modules.AI.Services;

namespace MyModularMonolith.Modules.AI;

public static class AIModuleExtensions
{
    public static IServiceCollection AddAIModule(this IServiceCollection services, IConfiguration configuration)
    {
        var aiConfig = configuration.GetSection(AIConfiguration.SectionName).Get<AIConfiguration>()
            ?? new AIConfiguration();

        services.Configure<AIConfiguration>(configuration.GetSection(AIConfiguration.SectionName));

        services.AddSingleton<ILoggerFactory>(serviceProvider =>
                                           LoggerFactory.Create(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information)));

        ConfigureGeminiClient(services, aiConfig);
        services.AddScoped<IAIService, GeminiAIService>();
        services.AddScoped<IReservationAgent, ReservationAgent>();

        return services;
    }

    public static IEndpointRouteBuilder MapAIEndpoints(this IEndpointRouteBuilder app)
    {
        AIAnalysisEndpoints.MapAIAnalysisEndpoints(app);
        return app;
    }

    private static void ConfigureGeminiClient(IServiceCollection services, AIConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.Gemini.ApiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is required when AI is enabled. " +
                "Set AI:Gemini:ApiKey in configuration or user secrets.");
        }

        services.AddSingleton<IChatClient>(serviceProvider =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<GeminiChatClient>();

            var geminiClient = new GeminiChatClient(new GeminiDotnet.GeminiClientOptions
            {
                ApiKey = config.Gemini.ApiKey,
                ModelId = config.Gemini.ModelName,
                ApiVersion = GeminiApiVersions.V1Beta,
            });

            logger.LogInformation("Gemini client configured with model: {Model}", config.Gemini.ModelName);

            return new ChatClientBuilder(geminiClient)
            .UseLogging(loggerFactory)
            .UseFunctionInvocation(loggerFactory, c => { c.IncludeDetailedErrors = true; })
            .Build(serviceProvider);
        });
    }
}