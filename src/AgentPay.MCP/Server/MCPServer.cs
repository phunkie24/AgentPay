using AgentPay.MCP.Services;

namespace AgentPay.MCP.Server;

/// <summary>
/// Core MCP (Model Context Protocol) Server implementation
/// Provides tools, resources, and prompts to AI agents
/// </summary>
public class MCPServer
{
    private readonly ILogger<MCPServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _initialized = false;
    private string _protocolVersion = "2024-11-05";

    public MCPServer(ILogger<MCPServer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task<MCPInitializeResponse> InitializeAsync(MCPInitializeRequest request)
    {
        _logger.LogInformation("MCP Server initializing with client: {ClientName} {ClientVersion}",
            request.ClientInfo.Name, request.ClientInfo.Version);

        _initialized = true;

        var capabilities = new MCPServerCapabilities
        {
            Tools = new Dictionary<string, object> { ["listChanged"] = true },
            Resources = new Dictionary<string, object> { ["subscribe"] = true, ["listChanged"] = true },
            Prompts = new Dictionary<string, object> { ["listChanged"] = true }
        };

        return Task.FromResult(new MCPInitializeResponse(
            ProtocolVersion: _protocolVersion,
            Capabilities: capabilities,
            ServerInfo: new MCPServerInfo(
                Name: "AgentPay MCP Server",
                Version: "1.0.0"
            )
        ));
    }

    public async Task<MCPToolsListResponse> ListToolsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var toolRegistry = scope.ServiceProvider.GetRequiredService<MCPToolRegistry>();
        var tools = await toolRegistry.GetAllToolsAsync();

        return new MCPToolsListResponse(Tools: tools);
    }

    public async Task<MCPToolCallResponse> CallToolAsync(MCPToolCallRequest request)
    {
        _logger.LogInformation("Calling tool: {ToolName}", request.Name);

        using var scope = _serviceProvider.CreateScope();
        var toolRegistry = scope.ServiceProvider.GetRequiredService<MCPToolRegistry>();

        try
        {
            var result = await toolRegistry.ExecuteToolAsync(request.Name, request.Arguments);
            return new MCPToolCallResponse(
                Content: new[] { new MCPContent("text", result?.ToString() ?? "") },
                IsError: false
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {ToolName}", request.Name);
            return new MCPToolCallResponse(
                Content: new[] { new MCPContent("text", $"Error: {ex.Message}") },
                IsError: true
            );
        }
    }

    public async Task<MCPResourcesListResponse> ListResourcesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var resourceProvider = scope.ServiceProvider.GetRequiredService<MCPResourceProvider>();
        var resources = await resourceProvider.GetAllResourcesAsync();

        return new MCPResourcesListResponse(Resources: resources);
    }

    public async Task<MCPResourceReadResponse> ReadResourceAsync(MCPResourceReadRequest request)
    {
        _logger.LogInformation("Reading resource: {ResourceUri}", request.Uri);

        using var scope = _serviceProvider.CreateScope();
        var resourceProvider = scope.ServiceProvider.GetRequiredService<MCPResourceProvider>();

        try
        {
            var content = await resourceProvider.ReadResourceAsync(request.Uri);
            return new MCPResourceReadResponse(
                Contents: new[] { new MCPResourceContent(request.Uri, "text/plain", content) }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading resource {ResourceUri}", request.Uri);
            throw;
        }
    }

    public async Task<MCPPromptsListResponse> ListPromptsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var promptProvider = scope.ServiceProvider.GetRequiredService<MCPPromptProvider>();
        var prompts = await promptProvider.GetAllPromptsAsync();

        return new MCPPromptsListResponse(Prompts: prompts);
    }

    public async Task<MCPPromptGetResponse> GetPromptAsync(MCPPromptGetRequest request)
    {
        _logger.LogInformation("Getting prompt: {PromptName}", request.Name);

        using var scope = _serviceProvider.CreateScope();
        var promptProvider = scope.ServiceProvider.GetRequiredService<MCPPromptProvider>();

        var messages = await promptProvider.GetPromptAsync(request.Name, request.Arguments);
        return new MCPPromptGetResponse(Messages: messages);
    }
}

// Response Models
public record MCPInitializeResponse(string ProtocolVersion, MCPServerCapabilities Capabilities, MCPServerInfo ServerInfo);
public class MCPServerCapabilities
{
    public Dictionary<string, object> Tools { get; set; } = new();
    public Dictionary<string, object> Resources { get; set; } = new();
    public Dictionary<string, object> Prompts { get; set; } = new();
}
public record MCPServerInfo(string Name, string Version);

public record MCPToolsListResponse(IEnumerable<MCPTool> Tools);
public record MCPToolCallResponse(MCPContent[] Content, bool IsError);
public record MCPContent(string Type, string Text);

public record MCPResourcesListResponse(IEnumerable<MCPResource> Resources);
public record MCPResourceReadResponse(MCPResourceContent[] Contents);
public record MCPResourceContent(string Uri, string MimeType, string Text);

public record MCPPromptsListResponse(IEnumerable<MCPPrompt> Prompts);
public record MCPPromptGetResponse(MCPPromptMessage[] Messages);
