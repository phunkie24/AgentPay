using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 7: Task Decomposition
/// Breaks complex tasks into smaller subtasks
/// </summary>
public class DecompositionPattern
{
    private readonly ILLMService _llm;

    public DecompositionPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<DecompositionResult> ExecuteAsync(string complexTask)
    {
        var prompt = $$"""
        Complex Task: {{complexTask}}

        Break this down into smaller, manageable subtasks. Each subtask should be:
        1. Specific and actionable
        2. Independent when possible
        3. Ordered logically

        Respond in JSON format:
        {
            "subtasks": [
                {"id": 1, "description": "...", "dependencies": []},
                {"id": 2, "description": "...", "dependencies": [1]}
            ]
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5,
            ResponseFormat = "json"
        });

        var decomposition = System.Text.Json.JsonSerializer.Deserialize<TaskDecomposition>(response.Text);

        return new DecompositionResult
        {
            OriginalTask = complexTask,
            Subtasks = decomposition.Subtasks,
            ExecutionOrder = DetermineExecutionOrder(decomposition.Subtasks),
            Timestamp = DateTime.UtcNow
        };
    }

    private List<int> DetermineExecutionOrder(List<Subtask> subtasks)
    {
        var order = new List<int>();
        var completed = new HashSet<int>();

        while (completed.Count < subtasks.Count)
        {
            var next = subtasks
                .Where(st => !completed.Contains(st.Id))
                .Where(st => st.Dependencies.All(d => completed.Contains(d)))
                .FirstOrDefault();

            if (next != null)
            {
                order.Add(next.Id);
                completed.Add(next.Id);
            }
            else
            {
                break; // Circular dependency or error
            }
        }

        return order;
    }
}

public class TaskDecomposition
{
    public List<Subtask> Subtasks { get; set; } = new();
}

public class Subtask
{
    public int Id { get; set; }
    public string Description { get; set; }
    public List<int> Dependencies { get; set; } = new();
    public string? EstimatedEffort { get; set; }
}

public class DecompositionResult
{
    public string OriginalTask { get; set; }
    public List<Subtask> Subtasks { get; set; }
    public List<int> ExecutionOrder { get; set; }
    public DateTime Timestamp { get; set; }
}
