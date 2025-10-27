using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: builder.Configuration["OpenAI:DeploymentId"],
    endpoint: builder.Configuration["OpenAI:Endpoint"],
    apiKey: builder.Configuration["OpenAI:ApiKey"]
);
builder.Services.AddSingleton((serviceProvider) =>
{
    return new Kernel(serviceProvider);
});
builder.Services.AddSingleton<IChatService, ChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
