using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 5: Few-Shot Learning
/// Provides examples to guide the model's behavior
/// </summary>
public class FewShotPattern
{
    private readonly ILLMService _llm;

    public FewShotPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<FewShotResult> ExecuteAsync(
        string task,
        List<Example> examples,
        LLMOptions? options = null)
    {
        var examplesText = string.Join("\n\n", examples.Select((ex, i) =>
            $"Example {i + 1}:\nInput: {ex.Input}\nOutput: {ex.Output}"));

        var prompt = $"""
        Here are some examples of the task:

        {examplesText}

        Now, apply the same pattern to this new input:
        Input: {task}
        Output:
        """;

        var response = await _llm.GenerateAsync(prompt, options ?? new LLMOptions
        {
            Temperature = 0.3,
            MaxTokens = 500
        });

        return new FewShotResult
        {
            Input = task,
            Examples = examples,
            Output = response.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }
}

public class Example
{
    public string Input { get; set; }
    public string Output { get; set; }
    public string? Explanation { get; set; }
}

public class FewShotResult
{
    public string Input { get; set; }
    public List<Example> Examples { get; set; }
    public string Output { get; set; }
    public DateTime Timestamp { get; set; }
}
