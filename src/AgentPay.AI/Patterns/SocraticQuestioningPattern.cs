using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 29: Socratic Questioning
/// Uses probing questions to deepen reasoning
/// </summary>
public class SocraticQuestioningPattern
{
    private readonly ILLMService _llm;

    public SocraticQuestioningPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<SocraticResult> ExecuteAsync(
        string topic,
        int questionDepth = 3)
    {
        var dialogue = new List<SocraticExchange>();
        var currentFocus = topic;

        for (int i = 0; i < questionDepth; i++)
        {
            // Ask probing question
            var question = await GenerateSocraticQuestionAsync(currentFocus, dialogue);

            // Get response
            var answer = await GenerateAnswerAsync(question, dialogue);

            dialogue.Add(new SocraticExchange
            {
                QuestionNumber = i + 1,
                Question = question,
                Answer = answer
            });

            // Update focus based on answer
            currentFocus = answer;
        }

        // Synthesize insights
        var insights = await SynthesizeInsightsAsync(topic, dialogue);

        return new SocraticResult
        {
            Topic = topic,
            Dialogue = dialogue,
            Insights = insights,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<string> GenerateSocraticQuestionAsync(
        string focus,
        List<SocraticExchange> previousDialogue)
    {
        var context = string.Join("\n", previousDialogue.Select(d =>
            $"Q: {d.Question}\nA: {d.Answer}"));

        var prompt = $"""
        Previous dialogue:
        {context}

        Current focus: {focus}

        Ask a thought-provoking Socratic question that:
        1. Challenges assumptions
        2. Seeks deeper understanding
        3. Explores implications
        4. Examines evidence

        Question:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.7
        });

        return response.Text.Trim();
    }

    private async Task<string> GenerateAnswerAsync(
        string question,
        List<SocraticExchange> previousDialogue)
    {
        var context = string.Join("\n", previousDialogue.Select(d =>
            $"Q: {d.Question}\nA: {d.Answer}"));

        var prompt = $"""
        Previous dialogue:
        {context}

        Question: {question}

        Provide a thoughtful, reflective answer:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6
        });

        return response.Text.Trim();
    }

    private async Task<string> SynthesizeInsightsAsync(
        string topic,
        List<SocraticExchange> dialogue)
    {
        var dialogueText = string.Join("\n\n", dialogue.Select(d =>
            $"Q{d.QuestionNumber}: {d.Question}\nA{d.QuestionNumber}: {d.Answer}"));

        var prompt = $"""
        Topic: {topic}

        Socratic dialogue:
        {dialogueText}

        Synthesize the key insights and deeper understanding gained through this questioning:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class SocraticExchange
{
    public int QuestionNumber { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
}

public class SocraticResult
{
    public string Topic { get; set; }
    public List<SocraticExchange> Dialogue { get; set; }
    public string Insights { get; set; }
    public DateTime Timestamp { get; set; }
}
