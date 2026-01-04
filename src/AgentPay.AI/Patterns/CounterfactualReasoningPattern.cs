using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 32: Counterfactual Reasoning
/// Explores "what if" scenarios and alternative outcomes
/// </summary>
public class CounterfactualReasoningPattern
{
    private readonly ILLMService _llm;

    public CounterfactualReasoningPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<CounterfactualResult> ExecuteAsync(
        string actualScenario,
        List<string> counterfactualChanges)
    {
        var counterfactuals = new List<CounterfactualScenario>();

        foreach (var change in counterfactualChanges)
        {
            var scenario = await ExploreCounterfactualAsync(actualScenario, change);
            counterfactuals.Add(scenario);
        }

        var insights = await SynthesizeInsightsAsync(actualScenario, counterfactuals);

        return new CounterfactualResult
        {
            ActualScenario = actualScenario,
            CounterfactualScenarios = counterfactuals,
            Insights = insights,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<CounterfactualScenario> ExploreCounterfactualAsync(
        string actualScenario,
        string change)
    {
        var prompt = $$"""
        Actual Scenario: {{actualScenario}}

        What if: {{change}}

        Explore this counterfactual scenario:
        1. How would things be different?
        2. What would the outcomes be?
        3. What causal chains would change?

        Respond in JSON format:
        {
            "description": "description of changed scenario",
            "keyDifferences": ["diff1", "diff2"],
            "outcomes": ["outcome1", "outcome2"],
            "likelihood": 0.75
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6,
            ResponseFormat = "json"
        });

        var data = System.Text.Json.JsonSerializer.Deserialize<CounterfactualScenario>(
            response.Text);
        data.Change = change;

        return data;
    }

    private async Task<string> SynthesizeInsightsAsync(
        string actualScenario,
        List<CounterfactualScenario> counterfactuals)
    {
        var counterfactualsText = string.Join("\n\n", counterfactuals.Select((c, i) =>
            $"Counterfactual {i + 1}: {c.Change}\n" +
            $"Outcomes: {string.Join(", ", c.Outcomes)}\n" +
            $"Likelihood: {c.Likelihood}"));

        var prompt = $"""
        Actual Scenario: {actualScenario}

        Counterfactual scenarios explored:
        {counterfactualsText}

        What insights do these counterfactuals reveal about:
        1. Critical factors in the actual scenario
        2. Alternative pathways
        3. Decision points that matter most
        4. Robustness of outcomes

        Insights:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class CounterfactualScenario
{
    public string Change { get; set; }
    public string Description { get; set; }
    public List<string> KeyDifferences { get; set; } = new();
    public List<string> Outcomes { get; set; } = new();
    public double Likelihood { get; set; }
}

public class CounterfactualResult
{
    public string ActualScenario { get; set; }
    public List<CounterfactualScenario> CounterfactualScenarios { get; set; }
    public string Insights { get; set; }
    public DateTime Timestamp { get; set; }
}
