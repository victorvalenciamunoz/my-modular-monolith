namespace MyModularMonolith.Modules.AI.Configuration;

public class AIConfiguration
{
    public const string SectionName = "AI";

    public GeminiConfiguration Gemini { get; set; } = new();    
    public int TimeoutSeconds { get; set; } = 30;
}

public class GeminiConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gemini-pro";
    public int MaxTokens { get; set; } = 4096;
    public decimal Temperature { get; set; } = 0.7m;
}