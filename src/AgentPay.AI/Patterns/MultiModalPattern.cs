using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 25: Multi-Modal Reasoning
/// Processes multiple input modalities (text, code, data)
/// </summary>
public class MultiModalPattern
{
    private readonly ILLMService _llm;

    public MultiModalPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<MultiModalResult> ExecuteAsync(
        string task,
        Dictionary<string, string> modalityInputs)
    {
        var inputsText = string.Join("\n\n", modalityInputs.Select(kvp =>
            $"[{kvp.Key}]\n{kvp.Value}"));

        var prompt = $"""
        Task: {task}

        You have access to multiple types of information:
        {inputsText}

        Analyze all modalities together and provide a comprehensive response:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6
        });

        return new MultiModalResult
        {
            Task = task,
            Modalities = modalityInputs,
            IntegratedOutput = response.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }
}

public class MultiModalResult
{
    public string Task { get; set; }
    public Dictionary<string, string> Modalities { get; set; }
    public string IntegratedOutput { get; set; }
    public DateTime Timestamp { get; set; }
}
