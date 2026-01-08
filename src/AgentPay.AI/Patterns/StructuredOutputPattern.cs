using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 23: Structured Output
/// Ensures responses follow specific formats (JSON, XML, etc.)
/// </summary>
public class StructuredOutputPattern
{
    private readonly ILLMService _llm;

    public StructuredOutputPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<StructuredOutputResult> ExecuteAsync(
        string task,
        string outputFormat,
        string schema = null)
    {
        var schemaText = schema != null
            ? $"\n\nExpected schema:\n{schema}"
            : "";

        var prompt = $"""
        Task: {task}

        Provide your response in {outputFormat} format.{schemaText}

        Response:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.3,
            ResponseFormat = outputFormat.ToLower() == "json" ? "json" : null
        });

        return new StructuredOutputResult
        {
            Task = task,
            Format = outputFormat,
            Schema = schema,
            Output = response.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<T> ExecuteTypedAsync<T>(string task, string schema = null)
    {
        var result = await ExecuteAsync(task, "JSON", schema);
        return System.Text.Json.JsonSerializer.Deserialize<T>(result.Output);
    }
}

public class StructuredOutputResult
{
    public string Task { get; set; }
    public string Format { get; set; }
    public string Schema { get; set; }
    public string Output { get; set; }
    public DateTime Timestamp { get; set; }
}
