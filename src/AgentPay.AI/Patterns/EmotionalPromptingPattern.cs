using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 21: Emotional Prompting
/// Uses emotionally charged language to improve performance
/// </summary>
public class EmotionalPromptingPattern
{
    private readonly ILLMService _llm;

    public EmotionalPromptingPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<EmotionalPromptResult> ExecuteAsync(
        string task,
        EmotionalTone tone = EmotionalTone.Encouraging)
    {
        var emotionalPrefix = GetEmotionalPrefix(tone);

        var prompt = $"""
        {emotionalPrefix}

        Task: {task}

        Your response:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6
        });

        return new EmotionalPromptResult
        {
            Task = task,
            EmotionalTone = tone,
            EmotionalPrefix = emotionalPrefix,
            Output = response.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }

    private string GetEmotionalPrefix(EmotionalTone tone)
    {
        return tone switch
        {
            EmotionalTone.Encouraging => "This is very important to my career. I need you to do your absolute best work.",
            EmotionalTone.Urgent => "This is extremely urgent and critical. Lives may depend on getting this right.",
            EmotionalTone.Confident => "I know you can excel at this. Show me your best capabilities.",
            EmotionalTone.Collaborative => "We're working together on this. Let's combine our strengths to find the best solution.",
            _ => "Please help me with the following task."
        };
    }
}

public enum EmotionalTone
{
    Encouraging,
    Urgent,
    Confident,
    Collaborative
}

public class EmotionalPromptResult
{
    public string Task { get; set; }
    public EmotionalTone EmotionalTone { get; set; }
    public string EmotionalPrefix { get; set; }
    public string Output { get; set; }
    public DateTime Timestamp { get; set; }
}
