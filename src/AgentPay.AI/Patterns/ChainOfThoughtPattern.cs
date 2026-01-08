using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Agents.Base;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 1: Chain of Thought (CoT)
/// Encourages step-by-step reasoning before providing an answer
/// </summary>
public class ChainOfThoughtPattern
{
    private readonly ILLMService _llm;

    public ChainOfThoughtPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<CoTResult> ExecuteAsync(string problem, LLMOptions? options = null)
    {
        var prompt = $"""
        Let's approach this step-by-step:

        Problem: {problem}

        Think through this carefully:
        1) First, identify what we need to solve
        2) Break it down into manageable steps
        3) Work through each step
        4) Verify the solution makes sense

        Your reasoning:
        """;

        var response = await _llm.GenerateAsync(prompt, options ?? new LLMOptions
        {
            Temperature = 0.7,
            MaxTokens = 1000
        });

        return new CoTResult
        {
            Problem = problem,
            Reasoning = response.Text,
            Timestamp = DateTime.UtcNow
        };
    }
}

public class CoTResult
{
    public string Problem { get; set; }
    public string Reasoning { get; set; }
    public DateTime Timestamp { get; set; }
}
