# Exercise 3 - MNIST Digit Recognition API

This project is a web API that performs handwritten digit recognition using an ONNX model and the MNIST dataset.

## Overview

The API accepts image uploads and predicts which digit (0-9) the image contains using a pre-trained MNIST ONNX model. It preprocesses images to the required 28x28 grayscale format and returns predictions with confidence scores.

## Features

- **File Upload**: Upload image files for digit prediction
- **Image Preprocessing**: Automatic conversion to 28x28 grayscale format
- **ONNX Model Integration**: Uses MNIST-12 ONNX model for predictions
- **Swagger Documentation**: Interactive API documentation
- **Inversion Support**: Handle both black-on-white and white-on-black images

## Technologies Used

- **ASP.NET Core 8.0** - Web API framework
- **Microsoft.ML.OnnxRuntime** - ONNX model inference
- **SixLabors.ImageSharp** - Image processing and manipulation
- **Swagger/OpenAPI** - API documentation

## Prerequisites

- .NET 8.0 SDK
- MNIST ONNX model file (`mnist-12.onnx`)

## Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Excercise3
   ```

2. **Download the MNIST ONNX model**
   ```bash
   curl -L -o mnist-12.onnx https://github.com/onnx/models/raw/main/validated/vision/classification/mnist/model/mnist-12.onnx
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7000` (or the port shown in console).

## API Endpoints

### GET /
Returns a simple status message confirming the API is running.

### POST /predict-file
Predicts the digit in an uploaded image file.

**Parameters:**
- `image` (form-data): Image file containing a handwritten digit
- `invert` (query): Boolean flag to invert colors (use `true` for black digit on white background)

**Response:**
```json
{
  "predicted": 7,
  "scores": [0.001, 0.002, 0.001, 0.003, 0.001, 0.002, 0.001, 0.987, 0.001, 0.001]
}
```

## Image Requirements

- **Format**: Any common image format (PNG, JPG, etc.)
- **Content**: Single handwritten digit
- **Background**: 
  - For white digits on black background: use `invert=false`
  - For black digits on white background: use `invert=true`

## Project Structure

```
Excercise3/
├── Controllers/
│   └── PredictController.cs    # API controller (currently empty)
├── Properties/
│   └── launchSettings.json     # Launch configuration
├── bin/                        # Build output
├── obj/                        # Build artifacts
├── Program.cs                  # Main application entry point
├── Excercise3.csproj          # Project file
├── mnist-12.onnx              # MNIST ONNX model (download required)
└── README.md                  # This file
```

## How It Works

1. **Image Upload**: Client uploads an image via the `/predict-file` endpoint
2. **Preprocessing**: 
   - Image is converted to grayscale
   - Resized to 28x28 pixels with letterboxing
   - Normalized to [0,1] range
   - Optionally inverted based on the `invert` parameter
3. **Model Inference**: ONNX model processes the 28x28 tensor
4. **Post-processing**: Returns the predicted digit and confidence scores for all 10 classes

## Testing

You can test the API using:

1. **Swagger UI**: Navigate to `/swagger` when running in development mode
2. **cURL**: 
   ```bash
   curl -X POST "https://localhost:7000/predict-file?invert=true" \
        -H "Content-Type: multipart/form-data" \
        -F "image=@path/to/your/digit.png"
   ```
3. **Postman**: Import the OpenAPI specification from `/swagger/v1/swagger.json`

## Configuration

The application uses standard ASP.NET Core configuration. Key settings:

- **File Upload Limit**: 10MB (configured in Program.cs)
- **Model Path**: Must be in the application root directory
- **HTTPS Redirection**: Enabled for security

## Troubleshooting

- **Model not found**: Ensure `mnist-12.onnx` is in the project root directory
- **Poor predictions**: Try toggling the `invert` parameter
- **Large file uploads**: Check the 10MB limit configuration