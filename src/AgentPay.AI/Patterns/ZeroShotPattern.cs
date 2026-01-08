using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 6: Zero-Shot Learning
/// Direct task instruction without examples
/// </summary>
public class ZeroShotPattern
{
    private readonly ILLMService _llm;

    public ZeroShotPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<ZeroShotResult> ExecuteAsync(
        string instruction,
        string input,
        LLMOptions? options = null)
    {
        var prompt = $"""
        {instruction}

        Input: {input}

        Output:
        """;

        var response = await _llm.GenerateAsync(prompt, options ?? new LLMOptions
        {
            Temperature = 0.5,
            MaxTokens = 1000
        });

        return new ZeroShotResult
        {
            Instruction = instruction,
            Input = input,
            Output = response.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<ZeroShotResult> ExecuteWithContextAsync(
        string instruction,
        string context,
        string input,
        LLMOptions? options = null)
    {
        var prompt = $"""
        Context: {context}

        Task: {instruction}

        Input: {input}

        Output:
        """;

        var response = await _llm.GenerateAsync(prompt, options ?? new LLMOptions
        {
            Temperature = 0.5,
            MaxTokens = 1000
        });

        return new ZeroShotResult
        {
            Instruction = instruction,
            Input = input,
            Context = context,
            Output = response.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }
}

public class ZeroShotResult
{
    public string Instruction { get; set; }
    public string? Context { get; set; }
    public string Input { get; set; }
    public string Output { get; set; }
    public DateTime Timestamp { get; set; }
}
