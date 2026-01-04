using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 30: Analogical Reasoning
/// Uses analogies and comparisons to solve problems
/// </summary>
public class AnalogicalReasoningPattern
{
    private readonly ILLMService _llm;

    public AnalogicalReasoningPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<AnalogicalResult> ExecuteAsync(
        string problem,
        List<string> knownDomains = null)
    {
        // Find analogies
        var analogies = await FindAnalogiesAsync(problem, knownDomains);

        // Map solutions from analogies
        var mappedSolutions = new List<MappedSolution>();
        foreach (var analogy in analogies)
        {
            var solution = await MapSolutionAsync(problem, analogy);
            mappedSolutions.Add(solution);
        }

        // Synthesize final solution
        var finalSolution = await SynthesizeSolutionAsync(problem, mappedSolutions);

        return new AnalogicalResult
        {
            Problem = problem,
            Analogies = analogies,
            MappedSolutions = mappedSolutions,
            FinalSolution = finalSolution,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<List<Analogy>> FindAnalogiesAsync(
        string problem,
        List<string> knownDomains)
    {
        var domainsHint = knownDomains != null && knownDomains.Any()
            ? $"\nConsider these domains: {string.Join(", ", knownDomains)}"
            : "";

        var prompt = $$"""
        Problem: {{problem}}
        {{domainsHint}}

        Find 2-3 analogies from other domains that share similar structures or patterns.

        Respond in JSON format:
        {
            "analogies": [
                {
                    "domain": "domain name",
                    "description": "how it's analogous",
                    "keyMapping": "what corresponds to what"
                }
            ]
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.7,
            ResponseFormat = "json"
        });

        var data = System.Text.Json.JsonSerializer.Deserialize<AnalogiesData>(response.Text);
        return data.Analogies;
    }

    private async Task<MappedSolution> MapSolutionAsync(string problem, Analogy analogy)
    {
        var prompt = $"""
        Original Problem: {problem}

        Analogy: {analogy.Description}
        From domain: {analogy.Domain}
        Mapping: {analogy.KeyMapping}

        How would the solution approach from this analogous domain apply to our problem?

        Mapped Solution:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6
        });

        return new MappedSolution
        {
            SourceDomain = analogy.Domain,
            AnalogousApproach = response.Text.Trim()
        };
    }

    private async Task<string> SynthesizeSolutionAsync(
        string problem,
        List<MappedSolution> mappedSolutions)
    {
        var solutionsText = string.Join("\n\n", mappedSolutions.Select((s, i) =>
            $"Approach {i + 1} (from {s.SourceDomain}):\n{s.AnalogousApproach}"));

        var prompt = $"""
        Problem: {problem}

        Solutions inspired by analogies:
        {solutionsText}

        Synthesize these analogical insights into a concrete solution for the original problem:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class Analogy
{
    public string Domain { get; set; }
    public string Description { get; set; }
    public string KeyMapping { get; set; }
}

public class AnalogiesData
{
    public List<Analogy> Analogies { get; set; } = new();
}

public class MappedSolution
{
    public string SourceDomain { get; set; }
    public string AnalogousApproach { get; set; }
}

public class AnalogicalResult
{
    public string Problem { get; set; }
    public List<Analogy> Analogies { get; set; }
    public List<MappedSolution> MappedSolutions { get; set; }
    public string FinalSolution { get; set; }
    public DateTime Timestamp { get; set; }
}
