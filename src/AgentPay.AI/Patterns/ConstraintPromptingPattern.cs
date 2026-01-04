using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 24: Constraint Prompting
/// Adds specific constraints to guide outputs
/// </summary>
public class ConstraintPromptingPattern
{
    private readonly ILLMService _llm;

    public ConstraintPromptingPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<ConstraintResult> ExecuteAsync(
        string task,
        List<Constraint> constraints)
    {
        var constraintsText = string.Join("\n", constraints.Select((c, i) =>
            $"{i + 1}. {c.Description}"));

        var prompt = $"""
        Task: {task}

        CONSTRAINTS - You MUST follow these rules:
        {constraintsText}

        Provide your response following all constraints:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.4
        });

        var output = response.Text.Trim();

        // Validate constraints
        var violations = await ValidateConstraintsAsync(output, constraints);

        return new ConstraintResult
        {
            Task = task,
            Constraints = constraints,
            Output = output,
            ConstraintViolations = violations,
            AllConstraintsMet = !violations.Any(),
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<List<string>> ValidateConstraintsAsync(
        string output,
        List<Constraint> constraints)
    {
        var violations = new List<string>();

        foreach (var constraint in constraints)
        {
            var validationPrompt = $"""
            Output: {output}
            Constraint: {constraint.Description}

            Does the output violate this constraint? Answer 'yes' or 'no' with brief explanation.
            """;

            var response = await _llm.GenerateAsync(validationPrompt, new LLMOptions
            {
                Temperature = 0.1,
                MaxTokens = 50
            });

            if (response.Text.ToLower().StartsWith("yes"))
            {
                violations.Add($"{constraint.Description}: {response.Text}");
            }
        }

        return violations;
    }
}

public class Constraint
{
    public string Description { get; set; }
    public ConstraintType Type { get; set; }
}

public enum ConstraintType
{
    Length,
    Format,
    Content,
    Style,
    Complexity
}

public class ConstraintResult
{
    public string Task { get; set; }
    public List<Constraint> Constraints { get; set; }
    public string Output { get; set; }
    public List<string> ConstraintViolations { get; set; }
    public bool AllConstraintsMet { get; set; }
    public DateTime Timestamp { get; set; }
}
