using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AgentPay.AI.Tools;

public interface IToolRegistry
{
    ITool GetTool(string name);
    List<ToolDefinition> GetAvailableTools();
    Task<object> ExecuteAsync(string toolName, Dictionary<string, object> parameters);
}

public interface ITool
{
    string Name { get; }
    string Description { get; }
    Task<object> ExecuteAsync(Dictionary<string, object> parameters);
}

public class ToolDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, ParameterDefinition> Parameters { get; set; }
}

public class ParameterDefinition
{
    public string Type { get; set; }
    public string Description { get; set; }
    public bool Required { get; set; }
}

public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new();

    public void RegisterTool(ITool tool)
    {
        _tools[tool.Name] = tool;
    }

    public ITool GetTool(string name)
    {
        return _tools.GetValueOrDefault(name) 
            ?? throw new ArgumentException($"Tool '{name}' not found");
    }

    public List<ToolDefinition> GetAvailableTools()
    {
        return _tools.Values.Select(t => new ToolDefinition
        {
            Name = t.Name,
            Description = t.Description
        }).ToList();
    }

    public async Task<object> ExecuteAsync(string toolName, Dictionary<string, object> parameters)
    {
        var tool = GetTool(toolName);
        return await tool.ExecuteAsync(parameters);
    }
}
