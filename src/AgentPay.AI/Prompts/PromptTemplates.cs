using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AgentPay.AI.Prompts;

/// <summary>
/// Reusable prompt templates with placeholder substitution
/// </summary>
public class PromptTemplate
{
    private readonly string _template;
    private readonly Dictionary<string, string> _placeholders;

    public PromptTemplate(string template)
    {
        _template = template;
        _placeholders = new Dictionary<string, string>();
    }

    public PromptTemplate Set(string key, string value)
    {
        _placeholders[key] = value;
        return this;
    }

    public PromptTemplate SetMultiple(Dictionary<string, string> values)
    {
        foreach (var kvp in values)
        {
            _placeholders[kvp.Key] = kvp.Value;
        }
        return this;
    }

    public string Build()
    {
        var result = _template;
        foreach (var kvp in _placeholders)
        {
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return result;
    }

    // Common templates
    public static class Common
    {
        public const string ChainOfThought = """
            Task: {{task}}

            Let's think through this step by step:

            1. What are we trying to achieve?
            2. What information do we have?
            3. What steps should we take?
            4. What's the expected outcome?

            Your reasoning:
            """;

        public const string FewShot = """
            Here are some examples:

            {{examples}}

            Now apply the same pattern to:
            {{input}}

            Your response:
            """;

        public const string RolePlay = """
            You are {{role}}.

            Background: {{background}}

            Task: {{task}}

            Respond in character:
            """;

        public const string Critique = """
            Content to critique:
            {{content}}

            Evaluate this content on:
            1. {{criterion_1}}
            2. {{criterion_2}}
            3. {{criterion_3}}

            Provide constructive feedback:
            """;

        public const string Synthesis = """
            Multiple perspectives:

            {{perspective_1}}

            {{perspective_2}}

            {{perspective_3}}

            Synthesize these into a unified viewpoint:
            """;

        public const string Decomposition = """
            Complex task: {{task}}

            Break this down into:
            1. Smaller, manageable subtasks
            2. Clear dependencies
            3. Success criteria

            Decomposed plan:
            """;

        public const string Verification = """
            Claim: {{claim}}
            Evidence: {{evidence}}

            Verify:
            1. Is the evidence relevant?
            2. Does it support the claim?
            3. Are there contradictions?
            4. What's your confidence level?

            Verification result:
            """;

        public const string Optimization = """
            Current approach: {{current}}

            Constraints:
            {{constraints}}

            Goals:
            {{goals}}

            Suggest optimizations that:
            1. Improve efficiency
            2. Maintain correctness
            3. Respect constraints

            Optimized approach:
            """;
    }
}

/// <summary>
/// Pre-built prompt template instances
/// </summary>
public static class PromptTemplates
{
    public static PromptTemplate ChainOfThought() =>
        new PromptTemplate(PromptTemplate.Common.ChainOfThought);

    public static PromptTemplate FewShot() =>
        new PromptTemplate(PromptTemplate.Common.FewShot);

    public static PromptTemplate RolePlay() =>
        new PromptTemplate(PromptTemplate.Common.RolePlay);

    public static PromptTemplate Critique() =>
        new PromptTemplate(PromptTemplate.Common.Critique);

    public static PromptTemplate Synthesis() =>
        new PromptTemplate(PromptTemplate.Common.Synthesis);

    public static PromptTemplate Decomposition() =>
        new PromptTemplate(PromptTemplate.Common.Decomposition);

    public static PromptTemplate Verification() =>
        new PromptTemplate(PromptTemplate.Common.Verification);

    public static PromptTemplate Optimization() =>
        new PromptTemplate(PromptTemplate.Common.Optimization);
}
