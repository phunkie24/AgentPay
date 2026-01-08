using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.ValueObjects;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 10: Multi-Agent Debate
/// Multiple agents debate different perspectives to reach a consensus
/// </summary>
public class MultiAgentDebatePattern
{
    private readonly ILLMService _llm;

    public MultiAgentDebatePattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<DebateResult> ExecuteAsync(
        string topic,
        List<string> agentPersonas,
        int rounds = 3)
    {
        var debate = new List<DebateRound>();

        for (int round = 0; round < rounds; round++)
        {
            var roundResponses = new List<AgentResponse>();

            foreach (var persona in agentPersonas)
            {
                var previousDebate = string.Join("\n\n", debate.SelectMany(r =>
                    r.Responses.Select(resp => $"{resp.Persona}: {resp.Argument}")));

                var response = await GenerateArgumentAsync(topic, persona, previousDebate);
                roundResponses.Add(response);
            }

            debate.Add(new DebateRound
            {
                RoundNumber = round + 1,
                Responses = roundResponses
            });
        }

        var consensus = await ReachConsensusAsync(topic, debate);

        return new DebateResult
        {
            Topic = topic,
            Participants = agentPersonas,
            Debate = debate,
            Consensus = consensus,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<AgentResponse> GenerateArgumentAsync(
        string topic,
        string persona,
        string previousDebate)
    {
        var prompt = $"""
        You are a {persona}.

        Topic for debate: {topic}

        Previous arguments:
        {previousDebate}

        Provide your perspective on this topic. Consider previous arguments and either:
        1. Support them with additional reasoning
        2. Present a counterargument
        3. Offer a synthesis of different viewpoints

        Your argument:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.8
        });

        return new AgentResponse
        {
            Persona = persona,
            Argument = response.Text.Trim()
        };
    }

    private async Task<string> ReachConsensusAsync(string topic, List<DebateRound> debate)
    {
        var allArguments = string.Join("\n\n", debate.SelectMany((round, i) =>
            round.Responses.Select(r => $"Round {i + 1} - {r.Persona}:\n{r.Argument}")));

        var prompt = $"""
        Topic: {topic}

        All arguments presented:
        {allArguments}

        Synthesize these different perspectives into a balanced consensus view that:
        1. Acknowledges valid points from all sides
        2. Resolves key disagreements
        3. Provides a nuanced final position

        Consensus:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5
        });

        return response.Text.Trim();
    }
}

public class DebateRound
{
    public int RoundNumber { get; set; }
    public List<AgentResponse> Responses { get; set; } = new();
}

public class AgentResponse
{
    public string Persona { get; set; }
    public string Argument { get; set; }
}

public class DebateResult
{
    public string Topic { get; set; }
    public List<string> Participants { get; set; }
    public List<DebateRound> Debate { get; set; }
    public string Consensus { get; set; }
    public DateTime Timestamp { get; set; }
}
