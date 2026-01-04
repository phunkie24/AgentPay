using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 33: Meta-Cognition
/// Thinks about its own thinking process and decision-making
/// </summary>
public class MetaCognitionPattern
{
    private readonly ILLMService _llm;

    public MetaCognitionPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<MetaCognitionResult> ExecuteAsync(string task)
    {
        // Initial attempt
        var initialAttempt = await SolveTaskAsync(task);

        // Reflect on approach
        var reflection = await ReflectOnApproachAsync(task, initialAttempt);

        // Evaluate thinking process
        var evaluation = await EvaluateThinkingAsync(task, initialAttempt, reflection);

        // Adjust strategy if needed
        string finalSolution;
        if (evaluation.ShouldAdjust)
        {
            finalSolution = await AdjustAndResolveAsync(task, evaluation.Adjustments);
        }
        else
        {
            finalSolution = initialAttempt.Solution;
        }

        return new MetaCognitionResult
        {
            Task = task,
            InitialAttempt = initialAttempt,
            Reflection = reflection,
            Evaluation = evaluation,
            FinalSolution = finalSolution,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<AttemptResult> SolveTaskAsync(string task)
    {
        var prompt = $$"""
        Task: {{task}}

        Solve this task while being mindful of your thinking process.

        Respond in JSON format:
        {
            "solution": "your solution",
            "approach": "what approach you used",
            "confidence": 0.85,
            "uncertainties": ["any uncertainties"]
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6,
            ResponseFormat = "json"
        });

        return System.Text.Json.JsonSerializer.Deserialize<AttemptResult>(response.Text);
    }

    private async Task<ReflectionData> ReflectOnApproachAsync(
        string task,
        AttemptResult attempt)
    {
        var prompt = $$"""
        Task: {{task}}
        Your approach: {{attempt.Approach}}
        Your solution: {{attempt.Solution}}

        Reflect on your thinking process:
        1. What assumptions did you make?
        2. What alternatives did you consider?
        3. What biases might have influenced your thinking?
        4. How confident are you in this approach?

        Respond in JSON format:
        {
            "assumptions": ["assumption1", "assumption2"],
            "alternatives": ["alternative1", "alternative2"],
            "potentialBiases": ["bias1", "bias2"],
            "confidenceReasoning": "why you're at this confidence level"
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5,
            ResponseFormat = "json"
        });

        return System.Text.Json.JsonSerializer.Deserialize<ReflectionData>(response.Text);
    }

    private async Task<ThinkingEvaluation> EvaluateThinkingAsync(
        string task,
        AttemptResult attempt,
        ReflectionData reflection)
    {
        var prompt = $$"""
        Task: {{task}}
        Solution: {{attempt.Solution}}
        Approach: {{attempt.Approach}}

        Meta-analysis:
        Assumptions: {{string.Join(", ", reflection.Assumptions)}}
        Potential biases: {{string.Join(", ", reflection.PotentialBiases)}}

        Evaluate your thinking process:
        1. Was your approach optimal?
        2. Should you adjust your strategy?
        3. What would improve the solution?

        Respond in JSON format:
        {
            "approachQuality": 0.8,
            "shouldAdjust": false,
            "adjustments": ["if needed, what to change"],
            "rationale": "reasoning for evaluation"
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.4,
            ResponseFormat = "json"
        });

        return System.Text.Json.JsonSerializer.Deserialize<ThinkingEvaluation>(response.Text);
    }

    private async Task<string> AdjustAndResolveAsync(
        string task,
        List<string> adjustments)
    {
        var adjustmentsText = string.Join("\n", adjustments.Select((a, i) => $"{i + 1}. {a}"));

        var prompt = $"""
        Task: {task}

        Based on meta-cognitive evaluation, make these adjustments:
        {adjustmentsText}

        Provide the improved solution:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class AttemptResult
{
    public string Solution { get; set; }
    public string Approach { get; set; }
    public double Confidence { get; set; }
    public List<string> Uncertainties { get; set; } = new();
}

public class ReflectionData
{
    public List<string> Assumptions { get; set; } = new();
    public List<string> Alternatives { get; set; } = new();
    public List<string> PotentialBiases { get; set; } = new();
    public string ConfidenceReasoning { get; set; }
}

public class ThinkingEvaluation
{
    public double ApproachQuality { get; set; }
    public bool ShouldAdjust { get; set; }
    public List<string> Adjustments { get; set; } = new();
    public string Rationale { get; set; }
}

public class MetaCognitionResult
{
    public string Task { get; set; }
    public AttemptResult InitialAttempt { get; set; }
    public ReflectionData Reflection { get; set; }
    public ThinkingEvaluation Evaluation { get; set; }
    public string FinalSolution { get; set; }
    public DateTime Timestamp { get; set; }
}
