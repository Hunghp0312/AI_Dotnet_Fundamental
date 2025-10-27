# Exercise 5 - MNIST + LLM Integration API

This project combines MNIST digit recognition with Large Language Model (LLM) capabilities to provide intelligent explanations and educational features for digit recognition tasks.

## Overview

The API integrates three main components:
1. **MNIST Digit Recognition**: ONNX-based digit prediction from images
2. **AI-Powered Explanations**: LLM analysis of prediction patterns and visual cues
3. **Educational Quiz Generation**: Personalized practice exercises based on user mistakes

## Features

- **Digit Prediction**: Standard MNIST digit recognition with confidence scores
- **Intelligent Explanations**: AI-generated explanations of prediction patterns
- **Adaptive Learning**: Quiz generation based on recent mistakes
- **Visual Analysis**: LLM interpretation of digit visual characteristics
- **Educational Feedback**: Tips for improving digit drawing clarity

## Technologies Used

- **ASP.NET Core 8.0** - Web API framework
- **Microsoft.ML.OnnxRuntime** - ONNX model inference
- **Microsoft Semantic Kernel** - AI orchestration
- **Azure OpenAI** - Language model services
- **SixLabors.ImageSharp** - Image processing
- **Swagger/OpenAPI** - API documentation

## Prerequisites

- .NET 8.0 SDK
- ONNX model file (`model.onnx`)
- Azure OpenAI service instance
- API key and endpoint configuration

## Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Excercise5
   ```

2. **Configure Azure OpenAI settings**
   Update `appsettings.json`:
   ```json
   {
     "OpenAI": {
       "DeploymentId": "your-deployment-name",
       "Endpoint": "https://your-resource.openai.azure.com/",
       "ApiKey": "your-api-key"
     }
   }
   ```

3. **Add ONNX model**
   Place your ONNX model file as `model.onnx` in the project root.

4. **Restore dependencies**
   ```bash
   dotnet restore
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7002` (or the port shown in console).

## API Endpoints

### GET /
Returns a simple status message confirming the API is running.

### POST /api/mnist/predict
Standard MNIST digit prediction from an uploaded image.

**Parameters:**
- `imageFile` (form-data): Image file containing a handwritten digit

**Response:**
```json
{
  "digit": 7,
  "confidence": 0.987,
  "topK": [
    { "digit": 7, "prob": 0.987 },
    { "digit": 1, "prob": 0.008 },
    { "digit": 9, "prob": 0.003 }
  ]
}
```

### POST /api/mnist/predict-explain
Enhanced prediction with AI-generated explanations.

**Parameters:**
- `imageFile` (form-data): Image file containing a handwritten digit

**Response:**
```json
{
  "digit": 7,
  "confidence": 0.987,
  "topK": [
    { "digit": 7, "prob": 0.987 },
    { "digit": 1, "prob": 0.008 },
    { "digit": 9, "prob": 0.003 }
  ],
  "explanation": "The model confidently recognized this as a 7 due to the distinctive horizontal line at the top and the diagonal stroke. The clear separation between strokes and good contrast made this an easy classification. To maintain clarity, ensure the top horizontal line is distinct from the diagonal."
}
```

### POST /api/mnist/quiz
Generates personalized practice exercises based on recent mistakes.

**Request Body:**
```json
{
  "recentMistakes": [3, 8, 9]
}
```

**Response:**
```json
{
  "instructions": [
    "Practice drawing 3 and 8: Focus on the curves and how they connect",
    "Work on 8 and 9: Pay attention to the loop closure in 8 vs the open curve in 9",
    "Compare 3 and 9: Notice how the middle curves face different directions"
  ],
  "tips": "Use consistent stroke thickness and ensure clear separation between different parts of each digit."
}
```

## Project Structure

```
Excercise5/
├── Services/
│   └── LlmService.cs          # AI explanation and quiz generation
├── Properties/
│   └── launchSettings.json    # Launch configuration
├── bin/                       # Build output
├── obj/                       # Build artifacts
├── Program.cs                 # Main application entry point with endpoints
├── Excercise5.csproj         # Project file
├── model.onnx                # ONNX model (add your model file)
├── appsettings.json          # Configuration file
└── README.md                 # This file
```

## Core Components

### Image Processing (ImageUtils)

- **ToMnistTensor**: Converts uploaded images to 28x28 grayscale tensors
- **To28x28PngBase64**: Creates base64-encoded thumbnails for LLM analysis

### Model Inference (Postprocess)

- **SoftmaxTopK**: Applies softmax normalization and extracts top-k predictions

### LLM Service (LlmService)

- **ExplainPredictionAsync**: Generates educational explanations of predictions
- **BuildQuizAsync**: Creates personalized practice exercises

## Configuration

### Required Settings

```json
{
  "OpenAI": {
    "DeploymentId": "your-gpt-deployment-name",
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key"
  }
}
```

### LLM Prompts

The service uses carefully crafted prompts for:

1. **Explanation Generation**:
   - Describes visual patterns and cues
   - Provides confidence analysis
   - Suggests improvements for ambiguous cases

2. **Quiz Generation**:
   - Analyzes common mistake patterns
   - Creates targeted practice exercises
   - Provides actionable drawing tips

## Testing

### Basic Prediction Test
```bash
curl -X POST "https://localhost:7002/api/mnist/predict" \
     -H "Content-Type: multipart/form-data" \
     -F "imageFile=@digit.png"
```

### Explanation Test
```bash
curl -X POST "https://localhost:7002/api/mnist/predict-explain" \
     -H "Content-Type: multipart/form-data" \
     -F "imageFile=@digit.png"
```

### Quiz Generation Test
```bash
curl -X POST "https://localhost:7002/api/mnist/quiz" \
     -H "Content-Type: application/json" \
     -d '{"recentMistakes": [3, 8, 9]}'
```

## Educational Features

### Intelligent Explanations

The AI analyzes predictions and provides:
- **Visual Pattern Analysis**: Describes key visual features
- **Confidence Assessment**: Explains prediction certainty
- **Improvement Suggestions**: Tips for clearer digit writing

### Personalized Quizzes

Based on mistake patterns, the system generates:
- **Targeted Exercises**: Focus on commonly confused digits
- **Practice Instructions**: Step-by-step drawing guidance
- **Learning Tips**: General advice for digit clarity

## Model Requirements

The ONNX model should:
- Accept input shape: `[1, 1, 28, 28]` (float32)
- Output 10 class probabilities
- Be named with input tensor "Input3" (or update the code accordingly)

## Error Handling

The application handles:
- Missing or invalid image files
- ONNX model loading failures
- Azure OpenAI connection issues
- Invalid quiz request formats

## Performance Considerations

- **Image Processing**: Optimized for 28x28 conversion
- **Model Inference**: Single-image processing
- **LLM Calls**: Cached where appropriate
- **Memory Management**: Proper disposal of image resources

## Troubleshooting

- **Model Loading**: Ensure `model.onnx` exists and is valid
- **Azure OpenAI**: Verify configuration and connectivity
- **Image Processing**: Check image format compatibility
- **Quiz Generation**: Ensure request includes valid mistake arrays