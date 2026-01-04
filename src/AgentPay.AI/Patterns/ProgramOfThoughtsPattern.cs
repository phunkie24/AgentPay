using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 16: Program of Thoughts (PoT)
/// Generates executable code to solve reasoning problems
/// </summary>
public class ProgramOfThoughtsPattern
{
    private readonly ILLMService _llm;

    public ProgramOfThoughtsPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<PoTResult> ExecuteAsync(string problem)
    {
        var prompt = $$"""
        Problem: {{problem}}

        Solve this by writing Python code that computes the answer.
        Think step-by-step and write clear, executable code.

        Respond in JSON format:
        {
            "reasoning": "explanation of approach",
            "code": "python code here",
            "answer": "final answer"
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.3,
            ResponseFormat = "json"
        });

        var result = System.Text.Json.JsonSerializer.Deserialize<ProgramOfThoughtsData>(
            response.Text);

        return new PoTResult
        {
            Problem = problem,
            Reasoning = result.Reasoning,
            GeneratedCode = result.Code,
            ComputedAnswer = result.Answer,
            Timestamp = DateTime.UtcNow
        };
    }
}

public class ProgramOfThoughtsData
{
    public string Reasoning { get; set; }
    public string Code { get; set; }
    public string Answer { get; set; }
}

public class PoTResult
{
    public string Problem { get; set; }
    public string Reasoning { get; set; }
    public string GeneratedCode { get; set; }
    public string ComputedAnswer { get; set; }
    public DateTime Timestamp { get; set; }
}
