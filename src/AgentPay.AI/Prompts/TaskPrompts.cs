using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AgentPay.AI.Prompts;

/// <summary>
/// Task-specific prompt templates
/// </summary>
public static class TaskPrompts
{
    public static string PaymentRequest(string recipient, decimal amount, string purpose) => $"""
        Process a payment request with the following details:

        Recipient: {recipient}
        Amount: {amount} MNEE
        Purpose: {purpose}

        Please:
        1. Verify the recipient wallet address format
        2. Confirm the amount is valid and within limits
        3. Check your current balance
        4. Execute the transaction if all checks pass
        5. Provide the transaction hash for tracking

        If any verification fails, explain the issue clearly.
        """;

    public static string NegotiatePrice(string service, decimal currentPrice, decimal targetPrice) => $"""
        Negotiate the price for the following service:

        Service: {service}
        Current Price: {currentPrice} MNEE
        Target Price: {targetPrice} MNEE

        Approach:
        1. Research market rates for similar services
        2. Identify value propositions
        3. Propose a fair counteroffer
        4. Justify your position with data
        5. Aim for a win-win outcome

        Be professional and data-driven in your negotiation.
        """;

    public static string VerifyTransaction(string transactionHash, decimal expectedAmount) => $"""
        Verify the following transaction:

        Transaction Hash: {transactionHash}
        Expected Amount: {expectedAmount} MNEE

        Verification steps:
        1. Query the blockchain for transaction details
        2. Confirm the transaction status (pending/confirmed)
        3. Verify the amount matches expectations
        4. Check the recipient address
        5. Validate any additional conditions

        Provide a clear verification result with all findings.
        """;

    public static string CreateExecutionPlan(string goal, List<string> constraints) => $"""
        Create a detailed execution plan for:

        Goal: {goal}

        Constraints:
        {string.Join("\n", constraints.Select((c, i) => $"{i + 1}. {c}"))}

        Your plan should include:
        1. Clear, actionable steps
        2. Dependencies between steps
        3. Success criteria for each step
        4. Risk assessment
        5. Estimated timeline

        Make the plan specific, measurable, and achievable.
        """;

    public static string ReflectOnOutcome(string action, string outcome, bool wasSuccessful) => $"""
        Reflect on this completed action:

        Action Taken: {action}
        Outcome: {outcome}
        Success: {wasSuccessful}

        Provide reflection on:
        1. What went well?
        2. What could be improved?
        3. What was learned?
        4. What would you do differently next time?
        5. Any patterns or insights?

        Be honest and constructive in your analysis.
        """;

    public static string SummarizeContext(string fullContext, int targetTokens) => $"""
        Summarize the following context to approximately {targetTokens} tokens:

        {fullContext}

        Preserve:
        - Key facts and decisions
        - Important outcomes
        - Critical details needed for continuity

        Remove:
        - Redundant information
        - Unnecessary details
        - Tangential discussions

        Maintain clarity and coherence in the summary.
        """;

    public static string AnalyzeRisk(string action, Dictionary<string, object> context) => $"""
        Analyze the risks for the following action:

        Action: {action}

        Context:
        {string.Join("\n", context.Select(kv => $"{kv.Key}: {kv.Value}"))}

        Risk analysis should cover:
        1. Potential negative outcomes
        2. Probability of each risk
        3. Impact severity (low/medium/high)
        4. Mitigation strategies
        5. Overall risk assessment

        Provide a comprehensive risk evaluation.
        """;

    public static string GenerateAlternatives(string problem, string currentSolution) => $"""
        Generate alternative solutions for:

        Problem: {problem}
        Current Solution: {currentSolution}

        Provide 3-5 alternative approaches that:
        1. Solve the same problem
        2. Differ in methodology or approach
        3. Have different trade-offs

        For each alternative, explain:
        - How it works
        - Advantages
        - Disadvantages
        - Best use cases
        """;
}
