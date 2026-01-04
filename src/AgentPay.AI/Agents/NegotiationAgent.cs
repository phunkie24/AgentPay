using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Tools;
using AgentPay.Domain.Entities;
using AgentPay.Domain.ValueObjects;
using AgentPay.AI.Agents.Base;
using AgentPay.AI.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgentPay.AI.Agents;

/// <summary>
/// Negotiation Agent: Price negotiation and terms optimization
/// Implements: Pattern 24 (Debate), Pattern 13 (Chain of Thought)
/// </summary>
public class NegotiationAgent : BaseAgent
{
    private readonly ILLMService _llm;
    private readonly ILogger<NegotiationAgent> _logger;

    public NegotiationAgent(
        ILLMService llm,
        IToolRegistry toolRegistry,
        ILogger<NegotiationAgent> logger)
        : base(toolRegistry, logger)
    {
        _llm = llm;
        _logger = logger;
        Name = "Negotiation Agent";
        Role = AgentRole.Negotiator;
    }

    public override async Task<AgentResult> ExecuteAsync(AgentTask task, AgentContext context)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Extract negotiation parameters
            var servicePrice = (decimal)task.Parameters["service_price"];
            var budget = (decimal)task.Parameters["budget"];
            var serviceName = task.Parameters["service_name"].ToString();

            // Pattern: Debate (Pattern 24) - Multi-round negotiation
            var rounds = new List<NegotiationRound>();
            var currentOffer = budget * 0.7m; // Start at 70% of budget
            var accepted = false;
            var maxRounds = 5;
            var round = 1;

            while (!accepted && round <= maxRounds)
            {
                // Generate negotiation strategy
                var strategy = await GenerateStrategyAsync(
                    servicePrice, currentOffer, budget, round, context);

                // Make counteroffer
                var counterOffer = await GenerateCounterOfferAsync(
                    strategy, servicePrice, currentOffer, context);

                // Evaluate seller response (simulated)
                var sellerResponse = await SimulateSellerResponseAsync(
                    counterOffer, servicePrice, round);

                rounds.Add(new NegotiationRound
                {
                    Round = round,
                    OurOffer = counterOffer,
                    SellerResponse = sellerResponse.Response,
                    SellerCounterOffer = sellerResponse.CounterOffer,
                    Reasoning = strategy
                });

                if (sellerResponse.Accepted)
                {
                    accepted = true;
                    currentOffer = sellerResponse.CounterOffer ?? counterOffer;
                }
                else if (sellerResponse.CounterOffer.HasValue)
                {
                    currentOffer = sellerResponse.CounterOffer.Value;
                }

                round++;
            }

            // Final evaluation
            var finalPrice = accepted ? currentOffer : servicePrice;
            var savings = servicePrice - finalPrice;
            var savingsPercent = (savings / servicePrice) * 100;

            return new AgentResult
            {
                Success = accepted,
                Output = JsonSerializer.Serialize(new
                {
                    FinalPrice = finalPrice,
                    OriginalPrice = servicePrice,
                    Savings = savings,
                    SavingsPercent = savingsPercent,
                    RoundsNeeded = rounds.Count,
                    Accepted = accepted
                }),
                Reasoning = $"Negotiation completed in {rounds.Count} rounds. " +
                           $"Achieved {savingsPercent:F1}% savings.",
                ExecutionTime = DateTime.UtcNow - startTime,
                Metadata = new Dictionary<string, object>
                {
                    ["rounds"] = rounds,
                    ["final_price"] = finalPrice,
                    ["savings_percent"] = savingsPercent
                },
                ConfidenceScore = accepted ? 0.9 : 0.5
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Negotiation failed");
            return new AgentResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.UtcNow - startTime
            };
        }
    }

    private async Task<string> GenerateStrategyAsync(
        decimal servicePrice,
        decimal currentOffer,
        decimal budget,
        int round,
        AgentContext context)
    {
        var prompt = $$"""
        You are a skilled negotiation agent.

        Situation:
        - Service Price: {{servicePrice}} MNEE
        - Our Budget: {{budget}} MNEE
        - Current Offer: {{currentOffer}} MNEE
        - Negotiation Round: {{round}}

        Generate a negotiation strategy:
        1. What's our position strength?
        2. What arguments should we make?
        3. What's our walk-away price?
        4. What concessions can we offer?

        Respond concisely (2-3 sentences):
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6,
            MaxTokens = 200
        });

        return response.Text;
    }

    private async Task<decimal> GenerateCounterOfferAsync(
        string strategy,
        decimal servicePrice,
        decimal currentOffer,
        AgentContext context)
    {
        var prompt = $$"""
        Based on this strategy:
        {{strategy}}

        Service Price: {{servicePrice}} MNEE
        Current Offer: {{currentOffer}} MNEE

        What should our next offer be? Consider:
        - Being reasonable but firm
        - Showing willingness to meet halfway
        - Not exceeding our budget significantly

        Respond with just a number (the offer amount):
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.3,
            MaxTokens = 50
        });

        // Parse the number from response
        if (decimal.TryParse(response.Text.Trim(), out var offer))
        {
            return Math.Min(offer, servicePrice);
        }

        // Fallback: increment by 10%
        return Math.Min(currentOffer * 1.1m, servicePrice);
    }

    private async Task<SellerResponse> SimulateSellerResponseAsync(
        decimal ourOffer,
        decimal servicePrice,
        int round)
    {
        // Simulation logic - in real implementation, this would interact with actual seller
        var acceptanceThreshold = servicePrice * 0.85m; // Will accept 85% or higher
        var minAcceptable = servicePrice * 0.70m; // Won't go below 70%

        if (ourOffer >= acceptanceThreshold)
        {
            return new SellerResponse
            {
                Accepted = true,
                Response = "Deal accepted",
                CounterOffer = ourOffer
            };
        }
        else if (ourOffer < minAcceptable)
        {
            return new SellerResponse
            {
                Accepted = false,
                Response = "Offer too low, here's my counteroffer",
                CounterOffer = servicePrice * 0.90m
            };
        }
        else
        {
            // Counter with midpoint
            var counter = (ourOffer + servicePrice) / 2;
            return new SellerResponse
            {
                Accepted = false,
                Response = "Let's meet in the middle",
                CounterOffer = counter
            };
        }
    }

    public override async Task<ReflectionResult> ReflectOnResultAsync(AgentResult result)
    {
        if (!result.Success)
        {
            return new ReflectionResult
            {
                ShouldRetry = true,
                Insights = "Negotiation failed - should retry with different strategy",
                Improvements = new() { "Adjust initial offer", "Be more flexible" }
            };
        }

        var savingsPercent = (double)result.Metadata["savings_percent"];
        var shouldRetry = savingsPercent < 5; // If we didn't save at least 5%, consider retry

        return new ReflectionResult
        {
            ShouldRetry = shouldRetry,
            Insights = $"Achieved {savingsPercent:F1}% savings",
            Improvements = savingsPercent < 10 
                ? new() { "Be more aggressive in initial offer" }
                : new() { "Strategy was effective" },
            Learnings = new Dictionary<string, object>
            {
                ["effective_opening_offer"] = "70-75% of asking price",
                ["avg_rounds_needed"] = result.Metadata["rounds"]
            }
        };
    }
}

public class NegotiationRound
{
    public int Round { get; set; }
    public decimal OurOffer { get; set; }
    public string SellerResponse { get; set; }
    public decimal? SellerCounterOffer { get; set; }
    public string Reasoning { get; set; }
}

public class SellerResponse
{
    public bool Accepted { get; set; }
    public string Response { get; set; }
    public decimal? CounterOffer { get; set; }
}
