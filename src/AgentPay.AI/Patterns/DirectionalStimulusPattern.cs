using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 15: Directional Stimulus Prompting
/// Provides directional hints to guide the model toward desired outputs
/// </summary>
public class DirectionalStimulusPattern
{
    private readonly ILLMService _llm;

    public DirectionalStimulusPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<DirectionalResult> ExecuteAsync(
        string task,
        string desiredDirection,
        List<string> hints = null)
    {
        var hintsText = hints != null && hints.Any()
            ? $"\n\nHelpful hints:\n{string.Join("\n", hints.Select((h, i) => $"{i + 1}. {h}"))}"
            : "";

        var prompt = $"""
        Task: {task}

        Direction: Focus on {desiredDirection}
        {hintsText}

        Approach this task with the given direction in mind:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6
        });

        return new DirectionalResult
        {
            Task = task,
            Direction = desiredDirection,
            Hints = hints ?? new List<string>(),
            Output = response.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }
}

public class DirectionalResult
{
    public string Task { get; set; }
    public string Direction { get; set; }
    public List<string> Hints { get; set; }
    public string Output { get; set; }
    public DateTime Timestamp { get; set; }
}
