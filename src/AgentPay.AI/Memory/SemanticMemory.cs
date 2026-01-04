using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AgentPay.AI.Memory;

/// <summary>
/// Semantic memory for storing structured knowledge and facts
/// Implements knowledge graph-like storage
/// </summary>
public class SemanticMemory
{
    private readonly Dictionary<string, KnowledgeNode> _nodes = new();
    private readonly List<KnowledgeRelation> _relations = new();

    public void AddKnowledge(string subject, string predicate, string objectValue, double confidence = 1.0)
    {
        // Ensure nodes exist
        EnsureNode(subject);
        EnsureNode(objectValue);

        // Add relation
        _relations.Add(new KnowledgeRelation
        {
            Id = Guid.NewGuid(),
            Subject = subject,
            Predicate = predicate,
            Object = objectValue,
            Confidence = confidence,
            Timestamp = DateTime.UtcNow
        });
    }

    public List<KnowledgeRelation> Query(string? subject = null, string? predicate = null, string? objectValue = null)
    {
        return _relations
            .Where(r =>
                (subject == null || r.Subject == subject) &&
                (predicate == null || r.Predicate == predicate) &&
                (objectValue == null || r.Object == objectValue))
            .ToList();
    }

    public List<string> GetRelated(string entity, string? relationshipType = null)
    {
        var related = _relations
            .Where(r => r.Subject == entity && (relationshipType == null || r.Predicate == relationshipType))
            .Select(r => r.Object)
            .Distinct()
            .ToList();

        return related;
    }

    public KnowledgeNode? GetNode(string key)
    {
        return _nodes.GetValueOrDefault(key);
    }

    public void UpdateNode(string key, Dictionary<string, object> properties)
    {
        if (_nodes.TryGetValue(key, out var node))
        {
            foreach (var prop in properties)
            {
                node.Properties[prop.Key] = prop.Value;
            }
            node.LastUpdated = DateTime.UtcNow;
        }
    }

    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "TotalNodes", _nodes.Count },
            { "TotalRelations", _relations.Count },
            { "UniquePredicates", _relations.Select(r => r.Predicate).Distinct().Count() }
        };
    }

    private void EnsureNode(string key)
    {
        if (!_nodes.ContainsKey(key))
        {
            _nodes[key] = new KnowledgeNode
            {
                Key = key,
                Properties = new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}

public class KnowledgeNode
{
    public string Key { get; set; }
    public Dictionary<string, object> Properties { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class KnowledgeRelation
{
    public Guid Id { get; set; }
    public string Subject { get; set; }
    public string Predicate { get; set; }
    public string Object { get; set; }
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; }
}
