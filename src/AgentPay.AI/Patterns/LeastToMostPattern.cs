using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 20: Least-to-Most Prompting
/// Breaks problems into simpler subproblems, solving from easiest to hardest
/// </summary>
public class LeastToMostPattern
{
    private readonly ILLMService _llm;

    public LeastToMostPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<LeastToMostResult> ExecuteAsync(string complexProblem)
    {
        // Decompose into ordered subproblems
        var subproblems = await DecomposeIntoSubproblemsAsync(complexProblem);

        // Solve each subproblem in order, building on previous solutions
        var solutions = new List<SubproblemSolution>();
        var context = "";

        foreach (var subproblem in subproblems)
        {
            var solution = await SolveSubproblemAsync(subproblem, context);
            solutions.Add(new SubproblemSolution
            {
                Subproblem = subproblem,
                Solution = solution
            });

            context += $"\nSubproblem: {subproblem}\nSolution: {solution}\n";
        }

        // Synthesize final answer
        var finalAnswer = await SynthesizeFinalAnswerAsync(complexProblem, solutions);

        return new LeastToMostResult
        {
            ComplexProblem = complexProblem,
            Subproblems = subproblems,
            Solutions = solutions,
            FinalAnswer = finalAnswer,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<List<string>> DecomposeIntoSubproblemsAsync(string problem)
    {
        var prompt = $$"""
        Complex Problem: {{problem}}

        Break this down into simpler subproblems, ordered from easiest to hardest.
        Each subproblem should build on previous ones.

        Respond in JSON format:
        {
            "subproblems": ["easiest subproblem", "next subproblem", "hardest subproblem"]
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5,
            ResponseFormat = "json"
        });

        var data = System.Text.Json.JsonSerializer.Deserialize<SubproblemsData>(response.Text);
        return data.Subproblems;
    }

    private async Task<string> SolveSubproblemAsync(string subproblem, string previousContext)
    {
        var prompt = $"""
        {previousContext}

        Now solve this subproblem:
        {subproblem}

        Solution:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }

    private async Task<string> SynthesizeFinalAnswerAsync(
        string originalProblem,
        List<SubproblemSolution> solutions)
    {
        var solutionsText = string.Join("\n\n", solutions.Select((s, i) =>
            $"Step {i + 1}: {s.Subproblem}\nSolution: {s.Solution}"));

        var prompt = $"""
        Original Problem: {originalProblem}

        Solutions to subproblems:
        {solutionsText}

        Synthesize these solutions into a complete final answer:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class SubproblemsData
{
    public List<string> Subproblems { get; set; } = new();
}

public class SubproblemSolution
{
    public string Subproblem { get; set; }
    public string Solution { get; set; }
}

public class LeastToMostResult
{
    public string ComplexProblem { get; set; }
    public List<string> Subproblems { get; set; }
    public List<SubproblemSolution> Solutions { get; set; }
    public string FinalAnswer { get; set; }
    public DateTime Timestamp { get; set; }
}
