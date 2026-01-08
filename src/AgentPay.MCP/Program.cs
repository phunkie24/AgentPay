using AgentPay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AgentPay.MCP.Server;
using AgentPay.MCP.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR for CQRS
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(AgentPay.Application.Commands.CreateAgentCommand).Assembly);
});

// MCP Server Services
builder.Services.AddSingleton<MCPServer>();
builder.Services.AddScoped<MCPToolRegistry>();
builder.Services.AddScoped<MCPResourceProvider>();
builder.Services.AddScoped<MCPPromptProvider>();

// Ollama LLM Service (FREE!)
builder.Services.AddScoped<AgentPay.AI.Services.ILLMService>(sp =>
{
    var ollamaUrl = builder.Configuration["AI:BaseUrl"] ?? "http://localhost:11434";
    var model = builder.Configuration["AI:Model"] ?? "llama3.2:latest";
    return new AgentPay.AI.Services.OllamaLLMService(ollamaUrl, model);
});

// MCP Client for Ollama Integration
builder.Services.AddHttpClient<AgentPay.MCP.Client.OllamaMCPClient>();
builder.Services.AddScoped<AgentPay.MCP.Client.OllamaMCPClient>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// MCP Protocol endpoints
app.MapPost("/mcp/initialize", async (MCPServer server, HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<MCPInitializeRequest>();
    var response = await server.InitializeAsync(request!);
    return Results.Json(response);
});

app.MapPost("/mcp/tools/list", async (MCPServer server) =>
{
    var response = await server.ListToolsAsync();
    return Results.Json(response);
});

app.MapPost("/mcp/tools/call", async (MCPServer server, HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<MCPToolCallRequest>();
    var response = await server.CallToolAsync(request!);
    return Results.Json(response);
});

app.MapPost("/mcp/resources/list", async (MCPServer server) =>
{
    var response = await server.ListResourcesAsync();
    return Results.Json(response);
});

app.MapPost("/mcp/resources/read", async (MCPServer server, HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<MCPResourceReadRequest>();
    var response = await server.ReadResourceAsync(request!);
    return Results.Json(response);
});

app.MapPost("/mcp/prompts/list", async (MCPServer server) =>
{
    var response = await server.ListPromptsAsync();
    return Results.Json(response);
});

app.MapPost("/mcp/prompts/get", async (MCPServer server, HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<MCPPromptGetRequest>();
    var response = await server.GetPromptAsync(request!);
    return Results.Json(response);
});

app.Run();

// MCP Protocol Models
public record MCPInitializeRequest(string ProtocolVersion, MCPClientInfo ClientInfo, Dictionary<string, object>? Capabilities);
public record MCPClientInfo(string Name, string Version);
public record MCPToolCallRequest(string Name, Dictionary<string, object> Arguments);
public record MCPResourceReadRequest(string Uri);
public record MCPPromptGetRequest(string Name, Dictionary<string, object>? Arguments);
