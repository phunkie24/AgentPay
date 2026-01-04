using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 26: Iterative Refinement
/// Progressively improves output through multiple iterations
/// </summary>
public class IterativeRefinementPattern
{
    private readonly ILLMService _llm;

    public IterativeRefinementPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<IterativeRefinementResult> ExecuteAsync(
        string task,
        int maxIterations = 3,
        double qualityThreshold = 0.9)
    {
        var iterations = new List<RefinementIteration>();
        var currentOutput = await GenerateInitialOutputAsync(task);

        for (int i = 0; i < maxIterations; i++)
        {
            var quality = await AssessQualityAsync(task, currentOutput);
            var feedback = await GenerateFeedbackAsync(task, currentOutput);

            iterations.Add(new RefinementIteration
            {
                IterationNumber = i + 1,
                Output = currentOutput,
                QualityScore = quality,
                Feedback = feedback
            });

            if (quality >= qualityThreshold)
            {
                break;
            }

            currentOutput = await RefineOutputAsync(task, currentOutput, feedback);
        }

        return new IterativeRefinementResult
        {
            Task = task,
            Iterations = iterations,
            FinalOutput = currentOutput,
            FinalQuality = iterations.Last().QualityScore,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<string> GenerateInitialOutputAsync(string task)
    {
        var response = await _llm.GenerateAsync(task, new LLMOptions
        {
            Temperature = 0.7
        });
        return response.Text.Trim();
    }

    private async Task<double> AssessQualityAsync(string task, string output)
    {
        var prompt = $"""
        Task: {task}
        Output: {output}

        Rate the quality of this output from 0-1. Consider:
        - Completeness
        - Accuracy
        - Clarity
        - Relevance

        Respond with just the number.
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.1,
            MaxTokens = 10
        });

        return double.TryParse(response.Text.Trim(), out double score) ? score : 0.5;
    }

    private async Task<string> GenerateFeedbackAsync(string task, string output)
    {
        var prompt = $"""
        Task: {task}
        Current Output: {output}

        Provide specific, actionable feedback for improvement:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }

    private async Task<string> RefineOutputAsync(string task, string currentOutput, string feedback)
    {
        var prompt = $"""
        Task: {task}
        Current Output: {currentOutput}

        Feedback: {feedback}

        Provide an improved version:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6
        });

        return response.Text.Trim();
    }
}

public class RefinementIteration
{
    public int IterationNumber { get; set; }
    public string Output { get; set; }
    public double QualityScore { get; set; }
    public string Feedback { get; set; }
}

public class IterativeRefinementResult
{
    public string Task { get; set; }
    public List<RefinementIteration> Iterations { get; set; }
    public string FinalOutput { get; set; }
    public double FinalQuality { get; set; }
    public DateTime Timestamp { get; set; }
}
