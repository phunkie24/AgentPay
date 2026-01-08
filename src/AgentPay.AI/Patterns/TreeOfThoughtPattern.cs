using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Agents.Base;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 2: Tree of Thought (ToT)
/// Explores multiple reasoning paths and selects the best one
/// </summary>
public class TreeOfThoughtPattern
{
    private readonly ILLMService _llm;

    public TreeOfThoughtPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<ToTResult> ExecuteAsync(string problem, int branches = 3, int depth = 2)
    {
        var rootNode = new ThoughtNode { Content = problem, Level = 0 };
        await ExpandNodeAsync(rootNode, branches, depth);

        var bestPath = FindBestPath(rootNode);

        return new ToTResult
        {
            Problem = problem,
            BestPath = bestPath,
            AllNodes = CollectAllNodes(rootNode),
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task ExpandNodeAsync(ThoughtNode node, int branches, int maxDepth)
    {
        if (node.Level >= maxDepth) return;

        var prompt = $$"""
        Current thinking: {{node.Content}}

        Generate {{branches}} different ways to continue this line of reasoning.
        Each should explore a different approach.

        Respond in JSON format:
        {
            "thoughts": ["thought1", "thought2", "thought3"],
            "scores": [0.8, 0.6, 0.9]
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.8,
            ResponseFormat = "json"
        });

        var expansion = System.Text.Json.JsonSerializer.Deserialize<ThoughtExpansion>(response.Text);

        for (int i = 0; i < expansion.Thoughts.Count && i < branches; i++)
        {
            var childNode = new ThoughtNode
            {
                Content = expansion.Thoughts[i],
                Score = expansion.Scores[i],
                Level = node.Level + 1,
                Parent = node
            };

            node.Children.Add(childNode);
            await ExpandNodeAsync(childNode, branches, maxDepth);
        }
    }

    private List<ThoughtNode> FindBestPath(ThoughtNode root)
    {
        var bestLeaf = FindBestLeaf(root);
        var path = new List<ThoughtNode>();

        var current = bestLeaf;
        while (current != null)
        {
            path.Insert(0, current);
            current = current.Parent;
        }

        return path;
    }

    private ThoughtNode FindBestLeaf(ThoughtNode node)
    {
        if (!node.Children.Any()) return node;

        return node.Children
            .Select(FindBestLeaf)
            .OrderByDescending(n => n.Score)
            .First();
    }

    private List<ThoughtNode> CollectAllNodes(ThoughtNode root)
    {
        var nodes = new List<ThoughtNode> { root };
        foreach (var child in root.Children)
        {
            nodes.AddRange(CollectAllNodes(child));
        }
        return nodes;
    }
}

public class ThoughtNode
{
    public string Content { get; set; }
    public double Score { get; set; }
    public int Level { get; set; }
    public ThoughtNode? Parent { get; set; }
    public List<ThoughtNode> Children { get; set; } = new();
}

public class ThoughtExpansion
{
    public List<string> Thoughts { get; set; } = new();
    public List<double> Scores { get; set; } = new();
}

public class ToTResult
{
    public string Problem { get; set; }
    public List<ThoughtNode> BestPath { get; set; }
    public List<ThoughtNode> AllNodes { get; set; }
    public DateTime Timestamp { get; set; }
}
