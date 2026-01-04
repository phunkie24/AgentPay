using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 17: Automatic Prompt Engineer (APE)
/// Automatically generates and optimizes prompts
/// </summary>
public class AutomaticPromptEngineerPattern
{
    private readonly ILLMService _llm;

    public AutomaticPromptEngineerPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<APEResult> ExecuteAsync(
        string taskDescription,
        List<Example> examplePairs,
        int candidatesCount = 5)
    {
        // Generate multiple prompt candidates
        var candidates = await GeneratePromptCandidatesAsync(
            taskDescription,
            examplePairs,
            candidatesCount);

        // Evaluate each candidate
        var evaluatedCandidates = new List<EvaluatedPrompt>();
        foreach (var candidate in candidates)
        {
            var score = await EvaluatePromptAsync(candidate, examplePairs);
            evaluatedCandidates.Add(new EvaluatedPrompt
            {
                Prompt = candidate,
                Score = score
            });
        }

        var bestPrompt = evaluatedCandidates.OrderByDescending(p => p.Score).First();

        return new APEResult
        {
            TaskDescription = taskDescription,
            AllCandidates = evaluatedCandidates,
            BestPrompt = bestPrompt.Prompt,
            BestScore = bestPrompt.Score,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<List<string>> GeneratePromptCandidatesAsync(
        string taskDescription,
        List<Example> examples,
        int count)
    {
        var examplesText = string.Join("\n", examples.Select(e =>
            $"Input: {e.Input}\nOutput: {e.Output}"));

        var prompt = $$"""
        Task: {{taskDescription}}

        Example input-output pairs:
        {{examplesText}}

        Generate {{count}} different prompts that could effectively perform this task.
        Make each prompt unique in style and approach.

        Respond in JSON format:
        {
            "prompts": ["prompt1", "prompt2", ...]
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.9,
            ResponseFormat = "json"
        });

        var data = System.Text.Json.JsonSerializer.Deserialize<PromptCandidates>(response.Text);
        return data.Prompts;
    }

    private async Task<double> EvaluatePromptAsync(string prompt, List<Example> examples)
    {
        int correct = 0;

        foreach (var example in examples)
        {
            var fullPrompt = $"{prompt}\n\nInput: {example.Input}\nOutput:";
            var response = await _llm.GenerateAsync(fullPrompt, new LLMOptions
            {
                Temperature = 0.3
            });

            // Simple similarity check (in real implementation, use better metric)
            if (response.Text.Trim().ToLower().Contains(example.Output.ToLower()))
            {
                correct++;
            }
        }

        return (double)correct / examples.Count;
    }
}

public class PromptCandidates
{
    public List<string> Prompts { get; set; } = new();
}

public class EvaluatedPrompt
{
    public string Prompt { get; set; }
    public double Score { get; set; }
}

public class APEResult
{
    public string TaskDescription { get; set; }
    public List<EvaluatedPrompt> AllCandidates { get; set; }
    public string BestPrompt { get; set; }
    public double BestScore { get; set; }
    public DateTime Timestamp { get; set; }
}
