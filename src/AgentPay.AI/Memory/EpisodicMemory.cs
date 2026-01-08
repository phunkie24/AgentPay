using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AgentPay.AI.Memory;

/// <summary>
/// Episodic memory for storing experiences and events in chronological order
/// Implements: Temporal sequence storage with context
/// </summary>
public class EpisodicMemory
{
    private readonly List<Episode> _episodes = new();
    private int _maxEpisodes;

    public EpisodicMemory(int maxEpisodes = 1000)
    {
        _maxEpisodes = maxEpisodes;
    }

    public void RecordEpisode(
        string eventType,
        string description,
        Dictionary<string, object>? context = null,
        string? outcome = null,
        double? emotionalValence = null)
    {
        var episode = new Episode
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Description = description,
            Context = context ?? new Dictionary<string, object>(),
            Outcome = outcome,
            EmotionalValence = emotionalValence,
            Timestamp = DateTime.UtcNow
        };

        _episodes.Add(episode);

        // Prune old episodes if limit exceeded
        if (_episodes.Count > _maxEpisodes)
        {
            _episodes.RemoveAt(0); // Remove oldest
        }
    }

    public List<Episode> GetRecentEpisodes(int count = 10)
    {
        return _episodes
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToList();
    }

    public List<Episode> GetEpisodesByType(string eventType, int? limit = null)
    {
        var query = _episodes
            .Where(e => e.EventType == eventType)
            .OrderByDescending(e => e.Timestamp);

        return limit.HasValue ? query.Take(limit.Value).ToList() : query.ToList();
    }

    public List<Episode> GetEpisodesByTimeRange(DateTime start, DateTime end)
    {
        return _episodes
            .Where(e => e.Timestamp >= start && e.Timestamp <= end)
            .OrderBy(e => e.Timestamp)
            .ToList();
    }

    public List<Episode> SearchEpisodes(string searchTerm)
    {
        var lowerSearch = searchTerm.ToLower();

        return _episodes
            .Where(e =>
                e.Description.ToLower().Contains(lowerSearch) ||
                e.EventType.ToLower().Contains(lowerSearch) ||
                (e.Outcome != null && e.Outcome.ToLower().Contains(lowerSearch)))
            .OrderByDescending(e => e.Timestamp)
            .ToList();
    }

    public EpisodePattern IdentifyPattern(string eventType, int minOccurrences = 3)
    {
        var episodes = GetEpisodesByType(eventType);

        if (episodes.Count < minOccurrences)
            return null;

        var successRate = episodes.Count(e => e.Outcome != null && e.Outcome.Contains("success"))
            / (double)episodes.Count;

        var avgEmotionalValence = episodes
            .Where(e => e.EmotionalValence.HasValue)
            .Select(e => e.EmotionalValence.Value)
            .DefaultIfEmpty(0)
            .Average();

        return new EpisodePattern
        {
            EventType = eventType,
            Occurrences = episodes.Count,
            SuccessRate = successRate,
            AverageEmotionalValence = avgEmotionalValence,
            FirstOccurrence = episodes.Min(e => e.Timestamp),
            LastOccurrence = episodes.Max(e => e.Timestamp)
        };
    }

    public void Clear()
    {
        _episodes.Clear();
    }

    public int Count => _episodes.Count;
}

public class Episode
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public string Description { get; set; }
    public Dictionary<string, object> Context { get; set; }
    public string? Outcome { get; set; }
    public double? EmotionalValence { get; set; } // -1 (negative) to +1 (positive)
    public DateTime Timestamp { get; set; }
}

public class EpisodePattern
{
    public string EventType { get; set; }
    public int Occurrences { get; set; }
    public double SuccessRate { get; set; }
    public double AverageEmotionalValence { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
}
