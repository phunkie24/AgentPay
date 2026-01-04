namespace AgentPay.MCP.Services;

/// <summary>
/// Provides pre-configured prompts for common AI agent tasks
/// </summary>
public class MCPPromptProvider
{
    private readonly ILogger<MCPPromptProvider> _logger;

    public MCPPromptProvider(ILogger<MCPPromptProvider> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<MCPPrompt>> GetAllPromptsAsync()
    {
        var prompts = new List<MCPPrompt>
        {
            new MCPPrompt(
                Name: "initiate_payment",
                Description: "Create a blockchain payment with proper verification and guardrails",
                Arguments: new[]
                {
                    new MCPPromptArgument("agent_id", "Agent ID who is making the payment", true),
                    new MCPPromptArgument("to_address", "Recipient wallet address", true),
                    new MCPPromptArgument("amount", "Payment amount in MNEE tokens", true),
                    new MCPPromptArgument("purpose", "Purpose of the payment", false)
                }
            ),

            new MCPPrompt(
                Name: "agent_reflection",
                Description: "Perform self-reflection on agent actions and outcomes",
                Arguments: new[]
                {
                    new MCPPromptArgument("agent_id", "Agent ID performing reflection", true),
                    new MCPPromptArgument("action", "The action taken by the agent", true),
                    new MCPPromptArgument("outcome", "The outcome of the action", true)
                }
            ),

            new MCPPrompt(
                Name: "plan_payment_strategy",
                Description: "Create a strategic plan for executing a payment with optimal gas costs",
                Arguments: new[]
                {
                    new MCPPromptArgument("agent_id", "Agent ID who needs the plan", true),
                    new MCPPromptArgument("goal", "The payment goal description", true),
                    new MCPPromptArgument("budget", "Maximum budget for the payment", true)
                }
            ),

            new MCPPrompt(
                Name: "analyze_transaction",
                Description: "Analyze a transaction for anomalies, risks, and optimization opportunities",
                Arguments: new[]
                {
                    new MCPPromptArgument("transaction_id", "Transaction ID to analyze", true)
                }
            ),

            new MCPPrompt(
                Name: "negotiate_service_price",
                Description: "Generate negotiation strategy for service pricing",
                Arguments: new[]
                {
                    new MCPPromptArgument("service_id", "Service ID to negotiate for", true),
                    new MCPPromptArgument("current_price", "Current listed price", true),
                    new MCPPromptArgument("target_price", "Target price to achieve", true)
                }
            )
        };

        return Task.FromResult<IEnumerable<MCPPrompt>>(prompts);
    }

    public Task<MCPPromptMessage[]> GetPromptAsync(string name, Dictionary<string, object>? arguments)
    {
        _logger.LogInformation("Getting prompt: {PromptName}", name);

        var messages = name switch
        {
            "initiate_payment" => GetInitiatePaymentPrompt(arguments),
            "agent_reflection" => GetAgentReflectionPrompt(arguments),
            "plan_payment_strategy" => GetPlanPaymentStrategyPrompt(arguments),
            "analyze_transaction" => GetAnalyzeTransactionPrompt(arguments),
            "negotiate_service_price" => GetNegotiateServicePricePrompt(arguments),
            _ => throw new ArgumentException($"Unknown prompt: {name}")
        };

        return Task.FromResult(messages);
    }

    private MCPPromptMessage[] GetInitiatePaymentPrompt(Dictionary<string, object>? args)
    {
        var agentId = args?.GetValueOrDefault("agent_id")?.ToString() ?? "unknown";
        var toAddress = args?.GetValueOrDefault("to_address")?.ToString() ?? "unknown";
        var amount = args?.GetValueOrDefault("amount")?.ToString() ?? "0";
        var purpose = args?.GetValueOrDefault("purpose")?.ToString() ?? "Service payment";

        return new[]
        {
            new MCPPromptMessage(
                Role: "system",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: "You are an AI agent assistant specializing in blockchain payments. Follow the Plan-and-Execute pattern with Guardrails to ensure safe transactions."
                )
            ),
            new MCPPromptMessage(
                Role: "user",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: $"""
                    Initiate a payment with the following details:
                    - Agent ID: {agentId}
                    - Recipient Address: {toAddress}
                    - Amount: {amount} MNEE
                    - Purpose: {purpose}

                    Before executing:
                    1. Verify the agent has sufficient balance
                    2. Check the recipient address is valid
                    3. Apply guardrail checks (amount limits, whitelist, daily limits)
                    4. Estimate gas costs
                    5. Create a payment plan with verification steps
                    6. Execute the payment with proper error handling

                    Provide your reasoning at each step using Chain-of-Thought.
                    """
                )
            )
        };
    }

    private MCPPromptMessage[] GetAgentReflectionPrompt(Dictionary<string, object>? args)
    {
        var agentId = args?.GetValueOrDefault("agent_id")?.ToString() ?? "unknown";
        var action = args?.GetValueOrDefault("action")?.ToString() ?? "unknown action";
        var outcome = args?.GetValueOrDefault("outcome")?.ToString() ?? "unknown outcome";

        return new[]
        {
            new MCPPromptMessage(
                Role: "system",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: "You are a reflective AI agent that learns from past actions. Use the Reflection pattern to improve future performance."
                )
            ),
            new MCPPromptMessage(
                Role: "user",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: $"""
                    Reflect on this action:
                    - Agent ID: {agentId}
                    - Action Taken: {action}
                    - Outcome: {outcome}

                    Analyze:
                    1. What went well?
                    2. What could be improved?
                    3. What did you learn?
                    4. How will you apply this learning to future actions?
                    5. What patterns do you notice?

                    Provide a structured reflection with clear insights.
                    """
                )
            )
        };
    }

    private MCPPromptMessage[] GetPlanPaymentStrategyPrompt(Dictionary<string, object>? args)
    {
        var agentId = args?.GetValueOrDefault("agent_id")?.ToString() ?? "unknown";
        var goal = args?.GetValueOrDefault("goal")?.ToString() ?? "unknown goal";
        var budget = args?.GetValueOrDefault("budget")?.ToString() ?? "0";

        return new[]
        {
            new MCPPromptMessage(
                Role: "system",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: "You are a strategic planning AI agent. Use the Decomposition and Plan-and-Execute patterns to create optimal payment strategies."
                )
            ),
            new MCPPromptMessage(
                Role: "user",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: $"""
                    Create a payment strategy:
                    - Agent ID: {agentId}
                    - Goal: {goal}
                    - Budget: {budget} MNEE

                    Create a plan that:
                    1. Breaks down the goal into sub-tasks (Decomposition)
                    2. Estimates costs for each sub-task
                    3. Optimizes for gas efficiency
                    4. Includes contingency plans
                    5. Defines success criteria
                    6. Sets up verification checkpoints

                    Provide a step-by-step execution plan.
                    """
                )
            )
        };
    }

    private MCPPromptMessage[] GetAnalyzeTransactionPrompt(Dictionary<string, object>? args)
    {
        var transactionId = args?.GetValueOrDefault("transaction_id")?.ToString() ?? "unknown";

        return new[]
        {
            new MCPPromptMessage(
                Role: "system",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: "You are a transaction analysis AI agent. Use the Verification pattern to analyze transactions thoroughly."
                )
            ),
            new MCPPromptMessage(
                Role: "user",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: $"""
                    Analyze transaction {transactionId}:

                    Check:
                    1. Transaction validity and authenticity
                    2. Gas costs vs expected
                    3. Any anomalies or suspicious patterns
                    4. Compliance with guardrails
                    5. Success probability
                    6. Risk factors
                    7. Optimization opportunities

                    Provide a comprehensive analysis with risk assessment.
                    """
                )
            )
        };
    }

    private MCPPromptMessage[] GetNegotiateServicePricePrompt(Dictionary<string, object>? args)
    {
        var serviceId = args?.GetValueOrDefault("service_id")?.ToString() ?? "unknown";
        var currentPrice = args?.GetValueOrDefault("current_price")?.ToString() ?? "0";
        var targetPrice = args?.GetValueOrDefault("target_price")?.ToString() ?? "0";

        return new[]
        {
            new MCPPromptMessage(
                Role: "system",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: "You are a negotiation AI agent. Use the Debate pattern to find optimal pricing agreements."
                )
            ),
            new MCPPromptMessage(
                Role: "user",
                Content: new MCPPromptContent(
                    Type: "text",
                    Text: $"""
                    Develop a negotiation strategy:
                    - Service ID: {serviceId}
                    - Current Price: {currentPrice} MNEE
                    - Target Price: {targetPrice} MNEE

                    Create a strategy that:
                    1. Analyzes the price gap
                    2. Identifies negotiation leverage points
                    3. Prepares counter-arguments
                    4. Defines acceptable price ranges
                    5. Plans fallback positions
                    6. Sets negotiation tactics

                    Provide a structured negotiation approach with reasoning.
                    """
                )
            )
        };
    }
}

// MCP Prompt Models
public record MCPPrompt(string Name, string Description, MCPPromptArgument[] Arguments);
public record MCPPromptArgument(string Name, string Description, bool Required);
public record MCPPromptMessage(string Role, MCPPromptContent Content);
public record MCPPromptContent(string Type, string Text);
