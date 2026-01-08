using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 22: Role Prompting
/// Assigns specific roles/personas to guide behavior
/// </summary>
public class RolePromptingPattern
{
    private readonly ILLMService _llm;

    public RolePromptingPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<RolePromptResult> ExecuteAsync(
        string task,
        string role,
        List<string> roleCharacteristics = null)
    {
        var characteristics = roleCharacteristics != null && roleCharacteristics.Any()
            ? $"\n\nAs a {role}, you:\n" + string.Join("\n", roleCharacteristics.Select(c => $"- {c}"))
            : "";

        var prompt = $"""
        You are a {role}.{characteristics}

        Task: {task}

        Respond in character:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.7
        });

        return new RolePromptResult
        {
            Task = task,
            Role = role,
            Characteristics = roleCharacteristics ?? new List<string>(),
            Output = response.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }
}

public class RolePromptResult
{
    public string Task { get; set; }
    public string Role { get; set; }
    public List<string> Characteristics { get; set; }
    public string Output { get; set; }
    public DateTime Timestamp { get; set; }
}
