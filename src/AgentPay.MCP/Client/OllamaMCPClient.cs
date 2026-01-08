using AgentPay.AI.Services;
using System.Text.Json;

namespace AgentPay.MCP.Client;

/// <summary>
/// MCP Client that integrates with Ollama for AI agent interactions
/// Enables Ollama-based agents to use AgentPay MCP tools
/// </summary>
public class OllamaMCPClient
{
    private readonly HttpClient _httpClient;
    private readonly ILLMService _llmService;
    private readonly ILogger<OllamaMCPClient> _logger;
    private readonly string _mcpServerUrl;

    public OllamaMCPClient(
        HttpClient httpClient,
        ILLMService llmService,
        IConfiguration configuration,
        ILogger<OllamaMCPClient> logger)
    {
        _httpClient = httpClient;
        _llmService = llmService;
        _logger = logger;
        _mcpServerUrl = configuration["MCP:ServerUrl"] ?? "http://localhost:8080";
    }

    /// <summary>
    /// Initialize connection to MCP Server
    /// </summary>
    public async Task<MCPInitializeResponse> InitializeAsync()
    {
        _logger.LogInformation("Initializing MCP Client with Ollama backend");

        var request = new
        {
            protocolVersion = "2024-11-05",
            clientInfo = new
            {
                name = "AgentPay Ollama Client",
                version = "1.0.0"
            },
            capabilities = new
            {
                tools = new { },
                resources = new { },
                prompts = new { }
            }
        };

        var response = await _httpClient.PostAsJsonAsync($"{_mcpServerUrl}/mcp/initialize", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MCPInitializeResponse>()
            ?? throw new Exception("Failed to initialize MCP connection");
    }

    /// <summary>
    /// Execute an AI agent task using Ollama with MCP tools
    /// </summary>
    public async Task<AgentExecutionResult> ExecuteAgentTaskAsync(string task, Dictionary<string, object>? context = null)
    {
        _logger.LogInformation("Executing agent task: {Task}", task);

        // Step 1: Get available tools from MCP Server
        var tools = await GetAvailableToolsAsync();
        _logger.LogInformation("Available MCP tools: {ToolCount}", tools.Count);

        // Step 2: Build system prompt with tool descriptions
        var systemPrompt = BuildSystemPromptWithTools(tools);

        // Step 3: Build user prompt with task and context
        var userPrompt = BuildUserPrompt(task, context);

        // Step 4: Get LLM response from Ollama
        var fullPrompt = $"{systemPrompt}\n\n{userPrompt}";
        var llmResponse = await _llmService.GenerateAsync(fullPrompt, new LLMOptions
        {
            Temperature = 0.7,
            MaxTokens = 4096
        });

        _logger.LogInformation("LLM Response received: {Length} characters", llmResponse.Text.Length);

        // Step 5: Parse tool calls from LLM response
        var toolCalls = ParseToolCalls(llmResponse.Text);

        // Step 6: Execute tool calls via MCP Server
        var toolResults = new List<object>();
        foreach (var toolCall in toolCalls)
        {
            var result = await ExecuteToolAsync(toolCall.Name, toolCall.Arguments);
            toolResults.Add(result);
        }

        // Step 7: If tools were executed, get final response from LLM
        string finalResponse = llmResponse.Text;
        if (toolResults.Any())
        {
            var toolResultsText = JsonSerializer.Serialize(toolResults, new JsonSerializerOptions { WriteIndented = true });
            var finalPrompt = $"{fullPrompt}\n\nTool Execution Results:\n{toolResultsText}\n\nProvide a final summary based on these results:";

            var finalLlmResponse = await _llmService.GenerateAsync(finalPrompt, new LLMOptions
            {
                Temperature = 0.7,
                MaxTokens = 2048
            });

            finalResponse = finalLlmResponse.Text;
        }

        return new AgentExecutionResult
        {
            Task = task,
            Response = finalResponse,
            ToolsUsed = toolCalls.Select(t => t.Name).ToList(),
            ToolResults = toolResults,
            Success = true,
            CompletedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get available tools from MCP Server
    /// </summary>
    private async Task<List<MCPToolInfo>> GetAvailableToolsAsync()
    {
        var response = await _httpClient.PostAsync($"{_mcpServerUrl}/mcp/tools/list", null);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MCPToolsListResponse>();
        return result?.Tools.Select(t => new MCPToolInfo
        {
            Name = t.Name,
            Description = t.Description,
            InputSchema = JsonSerializer.Serialize(t.InputSchema)
        }).ToList() ?? new List<MCPToolInfo>();
    }

    /// <summary>
    /// Execute a tool via MCP Server
    /// </summary>
    private async Task<object> ExecuteToolAsync(string toolName, Dictionary<string, object> arguments)
    {
        _logger.LogInformation("Executing MCP tool: {ToolName}", toolName);

        var request = new
        {
            name = toolName,
            arguments = arguments
        };

        var response = await _httpClient.PostAsJsonAsync($"{_mcpServerUrl}/mcp/tools/call", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MCPToolCallResponse>();
        return result?.Content.FirstOrDefault()?.Text ?? "No result";
    }

    /// <summary>
    /// Build system prompt with available tools
    /// </summary>
    private string BuildSystemPromptWithTools(List<MCPToolInfo> tools)
    {
        var toolDescriptions = string.Join("\n", tools.Select(t =>
            $"- {t.Name}: {t.Description}\n  Schema: {t.InputSchema}"));

        return $"""
            You are an AI agent with access to AgentPay blockchain payment tools.
            You can execute blockchain transactions, query agent data, and manage payments autonomously.

            Available Tools:
            {toolDescriptions}

            When you need to use a tool, respond in this format:
            TOOL_CALL: tool_name
            ARGUMENTS: {{"arg1": "value1", "arg2": "value2"}}

            You can make multiple tool calls by repeating this format.
            After tool execution, you will receive results to include in your final response.

            Always think step-by-step and explain your reasoning.
            """;
    }

    /// <summary>
    /// Build user prompt with task and context
    /// </summary>
    private string BuildUserPrompt(string task, Dictionary<string, object>? context)
    {
        var contextText = context != null
            ? $"\n\nContext:\n{JsonSerializer.Serialize(context, new JsonSerializerOptions { WriteIndented = true })}"
            : "";

        return $"""
            Task: {task}{contextText}

            Please complete this task using the available tools as needed.
            Provide a clear explanation of your actions and results.
            """;
    }

    /// <summary>
    /// Parse tool calls from LLM response
    /// </summary>
    private List<ToolCall> ParseToolCalls(string llmResponse)
    {
        var toolCalls = new List<ToolCall>();
        var lines = llmResponse.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("TOOL_CALL:", StringComparison.OrdinalIgnoreCase))
            {
                var toolName = lines[i].Substring("TOOL_CALL:".Length).Trim();

                // Look for ARGUMENTS on the next line
                if (i + 1 < lines.Length && lines[i + 1].StartsWith("ARGUMENTS:", StringComparison.OrdinalIgnoreCase))
                {
                    var argumentsJson = lines[i + 1].Substring("ARGUMENTS:".Length).Trim();
                    try
                    {
                        var arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(argumentsJson)
                            ?? new Dictionary<string, object>();

                        toolCalls.Add(new ToolCall
                        {
                            Name = toolName,
                            Arguments = arguments
                        });
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse tool arguments: {Arguments}", argumentsJson);
                    }
                }
            }
        }

        return toolCalls;
    }
}

// Supporting Types
public class MCPToolInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InputSchema { get; set; } = string.Empty;
}

public class ToolCall
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Arguments { get; set; } = new();
}

public class AgentExecutionResult
{
    public string Task { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public List<string> ToolsUsed { get; set; } = new();
    public List<object> ToolResults { get; set; } = new();
    public bool Success { get; set; }
    public DateTime CompletedAt { get; set; }
}

// Response types (shared with server)
public class MCPInitializeResponse
{
    public string ProtocolVersion { get; set; } = string.Empty;
    public object Capabilities { get; set; } = new();
    public object ServerInfo { get; set; } = new();
}

public class MCPToolsListResponse
{
    public List<MCPToolDto> Tools { get; set; } = new();
}

public class MCPToolDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object InputSchema { get; set; } = new();
}

public class MCPToolCallResponse
{
    public List<MCPContentDto> Content { get; set; } = new();
    public bool IsError { get; set; }
}

public class MCPContentDto
{
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
