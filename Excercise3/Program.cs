using Microsoft.AspNetCore.Http.Features;
using Microsoft.ML.OnnxRuntime;

using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAntiforgery();
// Add services to the container.
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<InferenceSession>(sp =>
{
    // Put the model file next to the app: "mnist-12.onnx"
    // Download example (GitHub raw; add -L):
    // curl -L -o mnist-12.onnx https://github.com/onnx/models/raw/main/validated/vision/classification/mnist/model/mnist-12.onnx
    var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "mnist-12.onnx");
    if (!File.Exists(modelPath)) throw new FileNotFoundException("Model not found", modelPath);
    var opts = new Microsoft.ML.OnnxRuntime.SessionOptions(); // CPU by default
    return new InferenceSession(modelPath, opts);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapGet("/", () => Results.Text("MNIST ONNX API is running. POST /predict-file or /predict-base64"));

app.MapPost("/predict-file", async (IFormFile image, bool invert, InferenceSession session) =>
{
    if (image is null || image.Length == 0) 
        return Results.BadRequest("Missing 'image' file.");

    await using var stream = image.OpenReadStream();
    using var img = await Image.LoadAsync<Rgba32>(stream);
    var tensor = Preprocess(img, invert);

    var result = Predict(session, tensor);
    return Results.Json(result);
}).DisableAntiforgery();

app.Run();

static DenseTensor<float> Preprocess(Image<Rgba32> img, bool invert)
{
    // 1) Convert to grayscale & resize to 28x28 (letterbox with padding)
    // MNIST expects white digit on black background. If your image is black digit on white,
    // set invert=true (or pass invert flag).
    const int size = 28;

    // Make a square canvas then resize to 28x28
    int maxSide = Math.Max(img.Width, img.Height);
    using var square = new Image<Rgba32>(maxSide, maxSide, new Rgba32(0, 0, 0, 255)); // black bg
    square.Mutate(ctx =>
    {
        // center the original
        int x = (maxSide - img.Width) / 2;
        int y = (maxSide - img.Height) / 2;
        ctx.DrawImage(img, new Point(x, y), 1f);
        ctx.Resize(size, size);
    });

    // 2) Convert to grayscale [0..1]
    var data = new float[size * size];
    int idx = 0;
    for (int y = 0; y < size; y++)
    {
        var rowSpan = square.DangerousGetPixelRowMemory(y).Span;
        for (int x = 0; x < size; x++)
        {
            var p = rowSpan[x];
            // luminance
            float gray = (0.299f * p.R + 0.587f * p.G + 0.114f * p.B) / 255f;
            if (invert) gray = 1f - gray; // swap foreground/background if needed
            data[idx++] = gray;
        }
    }

    // 3) Create NCHW tensor: [1,1,28,28]
    var tensor = new DenseTensor<float>(new[] { 1, 1, size, size });
    // Fill in row-major order (already matches NCHW with single channel)
    Buffer.BlockCopy(data.Select(f => BitConverter.SingleToInt32Bits(f)).ToArray(), 0,
                     tensor.Buffer.Span.ToArray(), 0, sizeof(float) * data.Length);

    // Buffer.BlockCopy with Spans is awkward; simpler manual copy:
    int i = 0;
    for (int yy = 0; yy < size; yy++)
        for (int xx = 0; xx < size; xx++)
            tensor[0, 0, yy, xx] = data[i++];

    return tensor;
}

static object Predict(InferenceSession session, DenseTensor<float> input)
{
    // Use the first input/output names to avoid model-specific naming differences.
    var inputName = session.InputMetadata.Keys.First();
    var outputName = session.OutputMetadata.Keys.First();

    var container = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor(inputName, input)
    };

    using var results = session.Run(container);
    var output = results.First(v => v.Name == outputName).AsEnumerable<float>().ToArray();

    // Some MNIST models output [1,10]; handle both [10] and [1,10]
    float[] scores = output.Length == 10 ? output : output.TakeLast(10).ToArray();

    int predicted = Array.IndexOf(scores, scores.Max());

    return new
    {
        predicted,
        scores
    };
}

record PredictRequest(string? Base64Image, bool? Invert);