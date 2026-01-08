using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 14: Active Prompting
/// Actively selects most uncertain/valuable examples for human annotation
/// </summary>
public class ActivePromptingPattern
{
    private readonly ILLMService _llm;

    public ActivePromptingPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<ActivePromptingResult> ExecuteAsync(
        List<string> unlabeledExamples,
        int selectTopK = 3)
    {
        var uncertaintyScores = new List<UncertainExample>();

        foreach (var example in unlabeledExamples)
        {
            var uncertainty = await MeasureUncertaintyAsync(example);
            uncertaintyScores.Add(new UncertainExample
            {
                Example = example,
                UncertaintyScore = uncertainty
            });
        }

        var mostUncertain = uncertaintyScores
            .OrderByDescending(u => u.UncertaintyScore)
            .Take(selectTopK)
            .ToList();

        return new ActivePromptingResult
        {
            AllExamples = uncertaintyScores,
            MostUncertainExamples = mostUncertain,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<double> MeasureUncertaintyAsync(string example)
    {
        // Generate multiple predictions with different temperatures
        var predictions = new List<string>();

        for (int i = 0; i < 5; i++)
        {
            var response = await _llm.GenerateAsync(example, new LLMOptions
            {
                Temperature = 0.8 + (i * 0.05) // Varying temperature
            });
            predictions.Add(response.Text.Trim());
        }

        // Measure disagreement (uncertainty)
        var uniquePredictions = predictions.Distinct().Count();
        return (double)uniquePredictions / predictions.Count;
    }
}

public class UncertainExample
{
    public string Example { get; set; }
    public double UncertaintyScore { get; set; }
}

public class ActivePromptingResult
{
    public List<UncertainExample> AllExamples { get; set; }
    public List<UncertainExample> MostUncertainExamples { get; set; }
    public DateTime Timestamp { get; set; }
}
