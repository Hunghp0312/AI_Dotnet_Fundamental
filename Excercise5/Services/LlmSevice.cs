using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public interface ILlmService
{
    Task<string> ExplainPredictionAsync(int digit, float conf, float[] probs, string image28x28PngBase64);
    Task<string> BuildQuizAsync(List<int> recentMistakes);
}

public class LlmService : ILlmService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    public LlmService(Kernel kernel)
    {
        _kernel = kernel;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> ExplainPredictionAsync(int digit, float conf, float[] probs, string image28x28PngBase64)
    {
        var systemPrompt = """
        You are a helpful tutor that explains MNIST digit predictions in simple, visual language.
        - Be concise (<=120 words).
        - Mention 2-3 visual cues (strokes, loops, corners, symmetry).
        - If confidence < 0.8, note likely confusions and tips to redraw for clarity.
        """;

        var userPrompt = $"""
        Predicted: {digit}
        Confidence: {conf:F2}
        Top-10 probabilities: {string.Join(", ", probs.Select((p,i)=> $"{i}:{p:F2}"))}

        Describe what patterns likely led to this prediction and how to reduce ambiguity if any.
        (Optional tiny image included as base64 PNG.)
        """;

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemPrompt);
        chatHistory.AddUserMessage(userPrompt);

        var settings = new PromptExecutionSettings()
        {
            ExtensionData = new Dictionary<string, object>()
            {
                ["temperature"] = 0.2
            }
        };

        var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, settings);
        return response.Content ?? "";
    }

    public async Task<string> BuildQuizAsync(List<int> recentMistakes)
    {
        var systemPrompt = "You are a coach. Create 3 short drawing tasks to practice confusing MNIST digits.";
        var userPrompt = $"Recent mistakes: {string.Join(", ", recentMistakes)}. Output JSON with fields: instructions[], tips.";

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemPrompt);
        chatHistory.AddUserMessage(userPrompt);

        var settings = new PromptExecutionSettings()
        {
            ExtensionData = new Dictionary<string, object>()
            {
                ["temperature"] = 0.7
            }
        };

        var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, settings);
        return response.Content ?? "";
    }
}
