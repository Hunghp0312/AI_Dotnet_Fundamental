# Exercise 4 - Semantic Kernel Chat API

This project is a web API that integrates Azure OpenAI services using Microsoft Semantic Kernel for chat completions and text summarization.

## Overview

The API provides two main functionalities:
1. **Chat Completion**: Direct interaction with Azure OpenAI for general questions
2. **Text Summarization**: Specialized endpoint that summarizes text content into Vietnamese bullet points

## Features

- **Azure OpenAI Integration**: Uses Semantic Kernel for seamless AI integration
- **Chat Endpoint**: General-purpose question answering
- **Summarization Service**: Vietnamese text summarization with custom prompts
- **Swagger Documentation**: Interactive API documentation
- **Dependency Injection**: Clean architecture with service layers

## Technologies Used

- **ASP.NET Core 8.0** - Web API framework
- **Microsoft Semantic Kernel** - AI orchestration framework
- **Azure OpenAI** - Language model services
- **Swagger/OpenAPI** - API documentation

## Prerequisites

- .NET 8.0 SDK
- Azure OpenAI service instance
- API key and endpoint configuration

## Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Excercise4
   ```

2. **Configure Azure OpenAI settings**
   Update `appsettings.json` or use environment variables:
   ```json
   {
     "OpenAI": {
       "DeploymentId": "your-deployment-name",
       "Endpoint": "https://your-resource.openai.azure.com/",
       "ApiKey": "your-api-key"
     }
   }
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7001` (or the port shown in console).

## API Endpoints

### POST /api/chat/ask
Sends a question to Azure OpenAI and returns the response.

**Request Body:**
```json
"What is the capital of France?"
```

**Response:**
```json
"Paris is the capital of France."
```

### POST /api/chat/summary
Summarizes the provided text into Vietnamese bullet points.

**Request Body:**
```json
"Long text content that needs to be summarized..."
```

**Response:**
```json
"• Điểm chính thứ nhất\n• Điểm chính thứ hai\n• Điểm chính thứ ba"
```

## Project Structure

```
Excercise4/
├── Controllers/
│   └── ChatController.cs       # API endpoints for chat and summary
├── Services/
│   └── ChatService.cs         # Business logic and AI integration
├── Properties/
│   └── launchSettings.json    # Launch configuration
├── bin/                       # Build output
├── obj/                       # Build artifacts
├── Program.cs                 # Main application entry point
├── Excercise4.csproj         # Project file
├── summarize.skprompt.txt    # Vietnamese summarization prompt
├── appsettings.json          # Configuration file
└── README.md                 # This file
```

## Configuration

### Required Settings

Add these settings to your `appsettings.json`:

```json
{
  "OpenAI": {
    "DeploymentId": "your-gpt-deployment-name",
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key"
  }
}
```

### Environment Variables (Alternative)

You can also use environment variables:
- `OpenAI__DeploymentId`
- `OpenAI__Endpoint`
- `OpenAI__ApiKey`

## Services

### ChatService

The `ChatService` class provides two main methods:

1. **Ask**: Direct chat completion using Azure OpenAI
2. **GetChatSummaryResponseAsync**: Text summarization using a custom Vietnamese prompt

### Custom Prompts

The summarization feature uses a custom prompt template located in `summarize.skprompt.txt`:
```
Hãy tóm tắt đoạn văn sau thành 3 gạch đầu dòng ngắn gọn, bằng tiếng Việt:

{{$input}}
```

## Testing

You can test the API using:

1. **Swagger UI**: Navigate to `/swagger` when running in development mode
2. **cURL Examples**:
   
   **Ask endpoint:**
   ```bash
   curl -X POST "https://localhost:7001/api/chat/ask" \
        -H "Content-Type: application/json" \
        -d "\"What is machine learning?\""
   ```
   
   **Summary endpoint:**
   ```bash
   curl -X POST "https://localhost:7001/api/chat/summary" \
        -H "Content-Type: application/json" \
        -d "\"Machine learning is a method of data analysis that automates analytical model building...\""
   ```

3. **Postman**: Import the OpenAPI specification from `/swagger/v1/swagger.json`

## Semantic Kernel Features

This project demonstrates several Semantic Kernel capabilities:

- **Azure OpenAI Integration**: Seamless connection to Azure OpenAI services
- **Function Creation**: Custom functions from prompt templates
- **Kernel Services**: Dependency injection of AI services
- **Prompt Engineering**: Custom Vietnamese summarization prompts

## Error Handling

The application includes basic error handling for:
- Missing configuration values
- API connection issues
- Invalid request formats

## Security Considerations

- **API Keys**: Store securely using Azure Key Vault or secure environment variables
- **HTTPS**: All communications should use HTTPS in production
- **Rate Limiting**: Consider implementing rate limiting for production use

## Troubleshooting

- **Configuration Errors**: Verify Azure OpenAI settings in `appsettings.json`
- **Connection Issues**: Check network connectivity to Azure OpenAI endpoint
- **Prompt Issues**: Verify the `summarize.skprompt.txt` file exists and is readable