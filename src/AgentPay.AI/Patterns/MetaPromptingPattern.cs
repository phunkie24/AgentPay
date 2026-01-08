using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 18: Meta-Prompting
/// Uses LLM to generate/improve prompts for specific tasks
/// </summary>
public class MetaPromptingPattern
{
    private readonly ILLMService _llm;

    public MetaPromptingPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<MetaPromptResult> ExecuteAsync(string taskGoal, string initialPrompt = null)
    {
        // Generate improved prompt
        var improvedPrompt = await GenerateImprovedPromptAsync(taskGoal, initialPrompt);

        // Test the prompt
        var testResult = await TestPromptAsync(improvedPrompt, taskGoal);

        // Iterate if needed
        if (testResult.Score < 0.8 && initialPrompt != null)
        {
            improvedPrompt = await RefinePromptAsync(improvedPrompt, testResult.Feedback);
        }

        return new MetaPromptResult
        {
            TaskGoal = taskGoal,
            InitialPrompt = initialPrompt,
            ImprovedPrompt = improvedPrompt,
            TestResult = testResult,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<string> GenerateImprovedPromptAsync(string taskGoal, string initialPrompt)
    {
        var baseContext = initialPrompt != null
            ? $"Current prompt: {initialPrompt}\n\nImprove this prompt to better achieve:"
            : "Create an effective prompt to achieve:";

        var prompt = $"""
        {baseContext}
        Goal: {taskGoal}

        Design a clear, specific prompt that:
        1. Clearly states the objective
        2. Provides necessary context
        3. Specifies desired output format
        4. Includes helpful constraints or guidelines

        Generated prompt:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.7
        });

        return response.Text.Trim();
    }

    private async Task<PromptTestResult> TestPromptAsync(string prompt, string goal)
    {
        var testPrompt = $$"""
        Evaluate this prompt for the given goal:

        Goal: {{goal}}
        Prompt: {{prompt}}

        Rate from 0-1 on:
        1. Clarity
        2. Specificity
        3. Likelihood of success

        Respond in JSON:
        {
            "score": 0.85,
            "strengths": ["..."],
            "weaknesses": ["..."],
            "feedback": "..."
        }
        """;

        var response = await _llm.GenerateAsync(testPrompt, new LLMOptions
        {
            Temperature = 0.3,
            ResponseFormat = "json"
        });

        return System.Text.Json.JsonSerializer.Deserialize<PromptTestResult>(response.Text);
    }

    private async Task<string> RefinePromptAsync(string prompt, string feedback)
    {
        var refinePrompt = $"""
        Current prompt: {prompt}

        Feedback: {feedback}

        Refine the prompt based on this feedback:
        """;

        var response = await _llm.GenerateAsync(refinePrompt, new LLMOptions
        {
            Temperature = 0.6
        });

        return response.Text.Trim();
    }
}

public class PromptTestResult
{
    public double Score { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public string Feedback { get; set; }
}

public class MetaPromptResult
{
    public string TaskGoal { get; set; }
    public string InitialPrompt { get; set; }
    public string ImprovedPrompt { get; set; }
    public PromptTestResult TestResult { get; set; }
    public DateTime Timestamp { get; set; }
}
