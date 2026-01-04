using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.ValueObjects;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 9: Self-Ask
/// Agent asks follow-up questions to gather necessary information
/// </summary>
public class SelfAskPattern
{
    private readonly ILLMService _llm;

    public SelfAskPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<SelfAskResult> ExecuteAsync(string question, int maxDepth = 3)
    {
        var qaChain = new List<QuestionAnswerPair>();
        await AskAndAnswerAsync(question, 0, maxDepth, qaChain);

        var finalAnswer = await SynthesizeFinalAnswerAsync(question, qaChain);

        return new SelfAskResult
        {
            OriginalQuestion = question,
            QuestionAnswerChain = qaChain,
            FinalAnswer = finalAnswer,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task AskAndAnswerAsync(
        string question,
        int currentDepth,
        int maxDepth,
        List<QuestionAnswerPair> qaChain)
    {
        if (currentDepth >= maxDepth) return;

        // Determine if we need to ask follow-up questions
        var followUpPrompt = $$"""
        Question: {{question}}

        To answer this question, do we need to ask any follow-up questions first?
        If yes, what follow-up questions should we ask?

        Respond in JSON format:
        {
            "needsFollowUp": true/false,
            "followUpQuestions": ["question1", "question2"]
        }
        """;

        var response = await _llm.GenerateAsync(followUpPrompt, new LLMOptions
        {
            Temperature = 0.3,
            ResponseFormat = "json"
        });

        var followUp = System.Text.Json.JsonSerializer.Deserialize<FollowUpAnalysis>(response.Text);

        if (followUp.NeedsFollowUp && followUp.FollowUpQuestions.Any())
        {
            foreach (var subQuestion in followUp.FollowUpQuestions)
            {
                await AskAndAnswerAsync(subQuestion, currentDepth + 1, maxDepth, qaChain);
            }
        }

        // Answer the current question
        var contextFromPrevious = string.Join("\n", qaChain.Select(qa =>
            $"Q: {qa.Question}\nA: {qa.Answer}"));

        var answerPrompt = $"""
        Previous context:
        {contextFromPrevious}

        Now answer this question:
        {question}

        Answer:
        """;

        var answerResponse = await _llm.GenerateAsync(answerPrompt, new LLMOptions
        {
            Temperature = 0.5
        });

        qaChain.Add(new QuestionAnswerPair
        {
            Question = question,
            Answer = answerResponse.Text.Trim(),
            Depth = currentDepth
        });
    }

    private async Task<string> SynthesizeFinalAnswerAsync(
        string originalQuestion,
        List<QuestionAnswerPair> qaChain)
    {
        var context = string.Join("\n\n", qaChain.Select((qa, i) =>
            $"{i + 1}. Q: {qa.Question}\n   A: {qa.Answer}"));

        var prompt = $"""
        Original Question: {originalQuestion}

        Information gathered:
        {context}

        Based on all this information, provide a comprehensive final answer:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class FollowUpAnalysis
{
    public bool NeedsFollowUp { get; set; }
    public List<string> FollowUpQuestions { get; set; } = new();
}

public class QuestionAnswerPair
{
    public string Question { get; set; }
    public string Answer { get; set; }
    public int Depth { get; set; }
}

public class SelfAskResult
{
    public string OriginalQuestion { get; set; }
    public List<QuestionAnswerPair> QuestionAnswerChain { get; set; }
    public string FinalAnswer { get; set; }
    public DateTime Timestamp { get; set; }
}
