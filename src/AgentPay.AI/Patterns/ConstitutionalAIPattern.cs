using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 13: Constitutional AI
/// Uses principles/rules to guide and critique AI responses
/// </summary>
public class ConstitutionalAIPattern
{
    private readonly ILLMService _llm;

    public ConstitutionalAIPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<ConstitutionalResult> ExecuteAsync(
        string task,
        List<ConstitutionalPrinciple> principles)
    {
        // Generate initial response
        var initialResponse = await GenerateInitialResponseAsync(task);

        // Critique based on principles
        var critiques = new List<PrincipleCritique>();
        foreach (var principle in principles)
        {
            var critique = await CritiqueAgainstPrincipleAsync(
                task,
                initialResponse,
                principle);
            critiques.Add(critique);
        }

        // Revise based on critiques
        var revisedResponse = await ReviseResponseAsync(
            task,
            initialResponse,
            critiques);

        return new ConstitutionalResult
        {
            Task = task,
            Principles = principles,
            InitialResponse = initialResponse,
            Critiques = critiques,
            RevisedResponse = revisedResponse,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<string> GenerateInitialResponseAsync(string task)
    {
        var prompt = $"""
        Task: {task}

        Provide your response:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.7
        });

        return response.Text.Trim();
    }

    private async Task<PrincipleCritique> CritiqueAgainstPrincipleAsync(
        string task,
        string response,
        ConstitutionalPrinciple principle)
    {
        var prompt = $$"""
        Principle: {{principle.Name}}
        Description: {{principle.Description}}

        Task: {{task}}
        Response: {{response}}

        Evaluate if this response adheres to the principle.

        Respond in JSON format:
        {
            "adheres": true/false,
            "score": 0.85,
            "reasoning": "...",
            "suggestedRevisions": ["..."]
        }
        """;

        var critiqueResponse = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.3,
            ResponseFormat = "json"
        });

        var critique = System.Text.Json.JsonSerializer.Deserialize<PrincipleCritique>(
            critiqueResponse.Text);
        critique.PrincipleName = principle.Name;

        return critique;
    }

    private async Task<string> ReviseResponseAsync(
        string task,
        string initialResponse,
        List<PrincipleCritique> critiques)
    {
        var critiquesSummary = string.Join("\n\n", critiques.Select(c =>
            $"Principle: {c.PrincipleName}\n" +
            $"Adheres: {c.Adheres}\n" +
            $"Reasoning: {c.Reasoning}\n" +
            $"Suggestions: {string.Join(", ", c.SuggestedRevisions)}"));

        var prompt = $"""
        Original Task: {task}
        Initial Response: {initialResponse}

        Critiques based on constitutional principles:
        {critiquesSummary}

        Revise the response to better align with these principles:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class ConstitutionalPrinciple
{
    public string Name { get; set; }
    public string Description { get; set; }
    public double Weight { get; set; } = 1.0; // Importance weight
}

public class PrincipleCritique
{
    public string PrincipleName { get; set; }
    public bool Adheres { get; set; }
    public double Score { get; set; }
    public string Reasoning { get; set; }
    public List<string> SuggestedRevisions { get; set; } = new();
}

public class ConstitutionalResult
{
    public string Task { get; set; }
    public List<ConstitutionalPrinciple> Principles { get; set; }
    public string InitialResponse { get; set; }
    public List<PrincipleCritique> Critiques { get; set; }
    public string RevisedResponse { get; set; }
    public DateTime Timestamp { get; set; }
}
