using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 3: Self-Consistency
/// Generates multiple reasoning paths and takes a majority vote
/// </summary>
public class SelfConsistencyPattern
{
    private readonly ILLMService _llm;

    public SelfConsistencyPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<SelfConsistencyResult> ExecuteAsync(string problem, int numSamples = 5)
    {
        var responses = new List<ReasoningPath>();

        for (int i = 0; i < numSamples; i++)
        {
            var prompt = $"""
            Problem: {problem}

            Solve this step by step. Show your reasoning and provide a final answer.
            """;

            var response = await _llm.GenerateAsync(prompt, new LLMOptions
            {
                Temperature = 0.8, // Higher temperature for diversity
                MaxTokens = 500
            });

            responses.Add(new ReasoningPath
            {
                Reasoning = response.Text,
                Answer = ExtractAnswer(response.Text)
            });
        }

        var answerGroups = responses
            .GroupBy(r => r.Answer)
            .OrderByDescending(g => g.Count())
            .ToList();

        var consensusAnswer = answerGroups.First().Key;
        var confidence = (double)answerGroups.First().Count() / numSamples;

        return new SelfConsistencyResult
        {
            Problem = problem,
            AllPaths = responses,
            ConsensusAnswer = consensusAnswer,
            Confidence = confidence,
            AlternativeAnswers = answerGroups.Skip(1).Select(g => new Answer
            {
                Value = g.Key,
                Count = g.Count()
            }).ToList(),
            Timestamp = DateTime.UtcNow
        };
    }

    private string ExtractAnswer(string text)
    {
        // Simple extraction - look for "answer:" or last line
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var answerLine = lines.FirstOrDefault(l =>
            l.ToLower().Contains("answer:") ||
            l.ToLower().Contains("final answer:"));

        return answerLine ?? lines.LastOrDefault() ?? text;
    }
}

public class ReasoningPath
{
    public string Reasoning { get; set; }
    public string Answer { get; set; }
}

public class Answer
{
    public string Value { get; set; }
    public int Count { get; set; }
}

public class SelfConsistencyResult
{
    public string Problem { get; set; }
    public List<ReasoningPath> AllPaths { get; set; }
    public string ConsensusAnswer { get; set; }
    public double Confidence { get; set; }
    public List<Answer> AlternativeAnswers { get; set; }
    public DateTime Timestamp { get; set; }
}
