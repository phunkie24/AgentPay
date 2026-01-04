using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 19: Generated Knowledge Prompting
/// Generates relevant knowledge before answering
/// </summary>
public class GeneratedKnowledgePattern
{
    private readonly ILLMService _llm;

    public GeneratedKnowledgePattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<GeneratedKnowledgeResult> ExecuteAsync(string question)
    {
        // Step 1: Generate relevant knowledge
        var knowledge = await GenerateRelevantKnowledgeAsync(question);

        // Step 2: Use knowledge to answer
        var answer = await AnswerWithKnowledgeAsync(question, knowledge);

        return new GeneratedKnowledgeResult
        {
            Question = question,
            GeneratedKnowledge = knowledge,
            Answer = answer,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<List<string>> GenerateRelevantKnowledgeAsync(string question)
    {
        var prompt = $$"""
        Question: {{question}}

        Before answering, generate 3-5 relevant facts or pieces of knowledge that would be helpful.
        Each should be a standalone fact.

        Respond in JSON format:
        {
            "knowledge": ["fact1", "fact2", "fact3"]
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.7,
            ResponseFormat = "json"
        });

        var data = System.Text.Json.JsonSerializer.Deserialize<KnowledgeData>(response.Text);
        return data.Knowledge;
    }

    private async Task<string> AnswerWithKnowledgeAsync(string question, List<string> knowledge)
    {
        var knowledgeText = string.Join("\n", knowledge.Select((k, i) => $"{i + 1}. {k}"));

        var prompt = $"""
        Relevant Knowledge:
        {knowledgeText}

        Question: {question}

        Using the knowledge above, provide a comprehensive answer:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class KnowledgeData
{
    public List<string> Knowledge { get; set; } = new();
}

public class GeneratedKnowledgeResult
{
    public string Question { get; set; }
    public List<string> GeneratedKnowledge { get; set; }
    public string Answer { get; set; }
    public DateTime Timestamp { get; set; }
}
