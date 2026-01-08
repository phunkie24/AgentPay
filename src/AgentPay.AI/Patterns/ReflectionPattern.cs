using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 8: Reflection
/// Evaluates and improves previous outputs through self-critique
/// </summary>
public class ReflectionPattern
{
    private readonly ILLMService _llm;

    public ReflectionPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<ReflectionResult> ExecuteAsync(
        string task,
        string initialOutput,
        int maxIterations = 3)
    {
        var iterations = new List<ReflectionIteration>();
        var currentOutput = initialOutput;

        for (int i = 0; i < maxIterations; i++)
        {
            // Critique phase
            var critique = await CritiqueOutputAsync(task, currentOutput);

            // Improvement phase
            var improvedOutput = await ImproveOutputAsync(task, currentOutput, critique);

            iterations.Add(new ReflectionIteration
            {
                IterationNumber = i + 1,
                Output = currentOutput,
                Critique = critique,
                ImprovedOutput = improvedOutput
            });

            // Check if improvement is significant
            if (critique.QualityScore > 0.9)
            {
                break; // Good enough
            }

            currentOutput = improvedOutput;
        }

        return new ReflectionResult
        {
            Task = task,
            InitialOutput = initialOutput,
            FinalOutput = currentOutput,
            Iterations = iterations,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<Critique> CritiqueOutputAsync(string task, string output)
    {
        var prompt = $$"""
        Task: {{task}}
        Output: {{output}}

        Critically evaluate this output:
        1. Does it fully address the task?
        2. Is it accurate and well-reasoned?
        3. Are there any errors or improvements needed?
        4. What's the quality score (0-1)?

        Respond in JSON format:
        {
            "strengths": ["..."],
            "weaknesses": ["..."],
            "suggestions": ["..."],
            "qualityScore": 0.85
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.3,
            ResponseFormat = "json"
        });

        return System.Text.Json.JsonSerializer.Deserialize<Critique>(response.Text);
    }

    private async Task<string> ImproveOutputAsync(string task, string output, Critique critique)
    {
        var prompt = $"""
        Task: {task}
        Current Output: {output}

        Weaknesses identified:
        {string.Join("\n", critique.Weaknesses)}

        Suggestions for improvement:
        {string.Join("\n", critique.Suggestions)}

        Generate an improved version that addresses these issues:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class Critique
{
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public double QualityScore { get; set; }
}

public class ReflectionIteration
{
    public int IterationNumber { get; set; }
    public string Output { get; set; }
    public Critique Critique { get; set; }
    public string ImprovedOutput { get; set; }
}

public class ReflectionResult
{
    public string Task { get; set; }
    public string InitialOutput { get; set; }
    public string FinalOutput { get; set; }
    public List<ReflectionIteration> Iterations { get; set; }
    public DateTime Timestamp { get; set; }
}
