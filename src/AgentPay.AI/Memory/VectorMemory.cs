using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AgentPay.AI.Memory;

/// <summary>
/// Vector-based memory for semantic search using embeddings
/// Implements: Pattern 28 (Long-Term Memory with embeddings)
/// </summary>
public class VectorMemory
{
    private readonly List<VectorMemoryEntry> _entries = new();

    public void Store(string content, float[] embedding, Dictionary<string, object>? metadata = null)
    {
        _entries.Add(new VectorMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = content,
            Embedding = embedding,
            Metadata = metadata ?? new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow
        });
    }

    public List<VectorMemoryEntry> Search(float[] queryEmbedding, int topK = 5, double threshold = 0.7)
    {
        var scored = _entries
            .Select(entry => new
            {
                Entry = entry,
                Similarity = CosineSimilarity(queryEmbedding, entry.Embedding)
            })
            .Where(x => x.Similarity >= threshold)
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .Select(x => x.Entry)
            .ToList();

        return scored;
    }

    public List<VectorMemoryEntry> GetAll()
    {
        return _entries.ToList();
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public int Count => _entries.Count;

    private double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have same length");

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;

        return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }
}

public class VectorMemoryEntry
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public float[] Embedding { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime Timestamp { get; set; }
}
