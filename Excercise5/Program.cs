using Microsoft.SemanticKernel;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddKernel();
builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: builder.Configuration["OpenAI:DeploymentId"] ?? throw new InvalidOperationException("OpenAI:DeploymentId is required"),
    endpoint: builder.Configuration["OpenAI:Endpoint"] ?? throw new InvalidOperationException("OpenAI:Endpoint is required"),
    apiKey: builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is required")
);

// Register LlmService
builder.Services.AddScoped<ILlmService, LlmService>();

// Register ONNX session
builder.Services.AddSingleton<InferenceSession>(provider =>
{
    // You'll need to add the path to your ONNX model file
    var modelPath = "mnist-12.onnx"; // Update this path to your actual model file
    return new InferenceSession(modelPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok("MNIST+LLM API running"));

// --- Endpoints ---
app.MapPost("/api/mnist/predict", async (IFormFile imageFile, InferenceSession session) =>
{
    if (imageFile == null || imageFile.Length == 0)
    {
        return Results.BadRequest("No image file provided");
    }

    using var stream = imageFile.OpenReadStream();
    var input = ImageUtils.ToMnistTensor(stream); // [1,1,28,28] float32
    var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", input) };
    using var results = session.Run(inputs);
    var output = results.First().AsEnumerable<float>().ToArray();

    var (digit, probs) = Postprocess.SoftmaxTopK(output, k: 3);
    var response = new PredictionResponse(
        digit.index,
        digit.prob,
        probs.Select((p, i) => new TopKPrediction(i, p))
             .OrderByDescending(x => x.Prob)
             .Take(3)
    );
    return Results.Ok(response);
}).DisableAntiforgery();;

app.MapPost("/api/mnist/predict-explain", async (IFormFile imageFile, InferenceSession session, ILlmService llmService) =>
{
    if (imageFile == null || imageFile.Length == 0)
    {
        return Results.BadRequest("No image file provided");
    }

    using var stream = imageFile.OpenReadStream();
    var input = ImageUtils.ToMnistTensor(stream);
    var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", input) };
    using var results = session.Run(inputs);
    var output = results.First().AsEnumerable<float>().ToArray();

    var (best, probs) = Postprocess.SoftmaxTopK(output, 3);

    // Base64 a tiny 28x28 PNG (optional) to help the LLM describe strokes
    stream.Position = 0;
    var smallPng = ImageUtils.To28x28PngBase64(stream);

    var explanation = await llmService.ExplainPredictionAsync(
        best.index, best.prob, probs, smallPng);

    var response = new PredictionWithExplanationResponse(
        best.index,
        best.prob,
        probs.Select((p, i) => new TopKPrediction(i, p))
             .OrderByDescending(x => x.Prob)
             .Take(3),
        explanation
    );
    return Results.Ok(response);
}).DisableAntiforgery();;

app.MapPost("/api/mnist/quiz", async (QuizRequest request, ILlmService llmService) =>
{
    if (request?.RecentMistakes == null || !request.RecentMistakes.Any())
    {
        return Results.BadRequest("RecentMistakes list is required and cannot be empty");
    }

    var quiz = await llmService.BuildQuizAsync(request.RecentMistakes);
    
    // Try to parse as JSON, otherwise return as plain text
    try
    {
        var jsonElement = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(quiz);
        return Results.Ok(jsonElement);
    }
    catch
    {
        return Results.Ok(new { response = quiz });
    }
}).DisableAntiforgery();;

app.Run();

// --- Helpers ---
static class ImageUtils
{
    public static DenseTensor<float> ToMnistTensor(Stream imageStream)
    {
        // Expect any RGB/Gray image; convert → 28x28 grayscale, normalize to 0..1, invert if white background
        imageStream.Position = 0;
        using var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.L8>(imageStream);
        image.Mutate(x => x.Resize(28, 28));

        var tensor = new DenseTensor<float>(new[] { 1, 1, 28, 28 });
        for (int y = 0; y < 28; y++)
        for (int x = 0; x < 28; x++)
        {
            var v = image[x, y].PackedValue / 255f; // 0 black .. 1 white
            tensor[0, 0, y, x] = 1f - v;            // MNIST digits are white on black
        }
        return tensor;
    }

    public static string To28x28PngBase64(Stream imageStream)
    {
        imageStream.Position = 0;
        using var image = SixLabors.ImageSharp.Image.Load(imageStream);
        image.Mutate(x => x.Resize(28, 28).BackgroundColor(SixLabors.ImageSharp.Color.White));
        using var outMs = new MemoryStream();
        image.SaveAsPng(outMs);
        return Convert.ToBase64String(outMs.ToArray());
    }
}

static class Postprocess
{
    public static ( (int index, float prob) best, float[] probs ) SoftmaxTopK(float[] scores, int k)
    {
        var max = scores.Max();
        var exps = scores.Select(s => MathF.Exp(s - max)).ToArray();
        var sum = exps.Sum();
        var probs = exps.Select(e => e / sum).ToArray();
        var bestIdx = Array.IndexOf(probs, probs.Max());
        return ((bestIdx, probs[bestIdx]), probs);
    }
}

// --- Request/Response Models ---
public record QuizRequest(List<int> RecentMistakes);

public record PredictionResponse(
    int Digit,
    float Confidence,
    IEnumerable<TopKPrediction> TopK
);

public record PredictionWithExplanationResponse(
    int Digit,
    float Confidence,
    IEnumerable<TopKPrediction> TopK,
    string Explanation
);

public record TopKPrediction(int Digit, float Prob);

// --- Helpers ---

