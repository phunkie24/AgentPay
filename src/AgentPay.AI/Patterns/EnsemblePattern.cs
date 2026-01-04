using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 28: Ensemble Methods
/// Combines multiple model outputs for better results
/// </summary>
public class EnsemblePattern
{
    private readonly ILLMService _llm;

    public EnsemblePattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<EnsembleResult> ExecuteAsync(
        string task,
        int ensembleSize = 3,
        EnsembleStrategy strategy = EnsembleStrategy.MajorityVote)
    {
        var outputs = new List<EnsembleMember>();

        // Generate multiple outputs with variation
        for (int i = 0; i < ensembleSize; i++)
        {
            var response = await _llm.GenerateAsync(task, new LLMOptions
            {
                Temperature = 0.7 + (i * 0.1) // Vary temperature
            });

            outputs.Add(new EnsembleMember
            {
                MemberId = i + 1,
                Output = response.Text.Trim(),
                Temperature = 0.7 + (i * 0.1)
            });
        }

        // Apply ensemble strategy
        var finalOutput = strategy switch
        {
            EnsembleStrategy.MajorityVote => await MajorityVoteAsync(outputs),
            EnsembleStrategy.Synthesis => await SynthesizeOutputsAsync(task, outputs),
            EnsembleStrategy.BestQuality => await SelectBestQualityAsync(task, outputs),
            _ => outputs.First().Output
        };

        return new EnsembleResult
        {
            Task = task,
            Strategy = strategy,
            EnsembleMembers = outputs,
            FinalOutput = finalOutput,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<string> MajorityVoteAsync(List<EnsembleMember> outputs)
    {
        // For majority vote, find most common output
        return outputs
            .GroupBy(o => o.Output)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    private async Task<string> SynthesizeOutputsAsync(string task, List<EnsembleMember> outputs)
    {
        var outputsText = string.Join("\n\n", outputs.Select((o, i) =>
            $"Version {i + 1}:\n{o.Output}"));

        var prompt = $"""
        Task: {task}

        Multiple versions of the solution:
        {outputsText}

        Synthesize these into a single, best answer that incorporates the strengths of each:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }

    private async Task<string> SelectBestQualityAsync(string task, List<EnsembleMember> outputs)
    {
        var scoredOutputs = new List<(EnsembleMember member, double score)>();

        foreach (var output in outputs)
        {
            var score = await ScoreOutputAsync(task, output.Output);
            scoredOutputs.Add((output, score));
        }

        return scoredOutputs.OrderByDescending(s => s.score).First().member.Output;
    }

    private async Task<double> ScoreOutputAsync(string task, string output)
    {
        var prompt = $"""
        Task: {task}
        Output: {output}

        Rate this output from 0-1 based on quality, accuracy, and completeness.
        Respond with just the number.
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.1,
            MaxTokens = 10
        });

        return double.TryParse(response.Text.Trim(), out double score) ? score : 0.5;
    }
}

public enum EnsembleStrategy
{
    MajorityVote,
    Synthesis,
    BestQuality
}

public class EnsembleMember
{
    public int MemberId { get; set; }
    public string Output { get; set; }
    public double Temperature { get; set; }
}

public class EnsembleResult
{
    public string Task { get; set; }
    public EnsembleStrategy Strategy { get; set; }
    public List<EnsembleMember> EnsembleMembers { get; set; }
    public string FinalOutput { get; set; }
    public DateTime Timestamp { get; set; }
}
