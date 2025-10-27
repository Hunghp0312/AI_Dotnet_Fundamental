using Microsoft.SemanticKernel;

public interface IChatService
{
    Task<string> GetChatSummaryResponseAsync(string chatRequest);
    Task<string> Ask(string chatRequest);
}

public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    public ChatService(Kernel kernel)
    {
        _kernel = kernel;

    }

    public async Task<string> GetChatSummaryResponseAsync(string chatRequest)
    {
        var summarizeKernelFunction = _kernel.CreateFunctionFromPrompt(
                promptTemplate: File.ReadAllText("./Data/summarize.skprompt.txt"),
                functionName: "SummarizeText");
        var result = await _kernel.InvokeAsync(summarizeKernelFunction, new() { ["input"] = chatRequest });
        return result.GetValue<string>() ?? "";

    }
    public async Task<string> Ask(string chatRequest)
    {
        var result = await _kernel.InvokePromptAsync<string>(chatRequest);
        return result ?? "";
    }
}