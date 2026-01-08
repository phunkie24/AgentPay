using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for logging messages and events
/// </summary>
public class LoggingTool : ITool
{
    private readonly ILogger<LoggingTool> _logger;

    public string Name => "logging";
    public string Description => "Log messages, warnings, and errors for debugging and monitoring";

    public LoggingTool(ILogger<LoggingTool> logger)
    {
        _logger = logger;
    }

    public Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var level = parameters.GetValueOrDefault("level")?.ToString() ?? "info";
        var message = parameters.GetValueOrDefault("message")?.ToString() ?? throw new ArgumentException("message required");
        var category = parameters.GetValueOrDefault("category")?.ToString() ?? "Agent";
        var metadata = parameters.GetValueOrDefault("metadata") as Dictionary<string, object>;

        var logMessage = $"[{category}] {message}";
        if (metadata != null && metadata.Any())
        {
            var metadataString = string.Join(", ", metadata.Select(kv => $"{kv.Key}={kv.Value}"));
            logMessage += $" | {metadataString}";
        }

        switch (level.ToLower())
        {
            case "debug":
                _logger.LogDebug(logMessage);
                break;
            case "info":
                _logger.LogInformation(logMessage);
                break;
            case "warning":
                _logger.LogWarning(logMessage);
                break;
            case "error":
                _logger.LogError(logMessage);
                break;
            case "critical":
                _logger.LogCritical(logMessage);
                break;
            default:
                _logger.LogInformation(logMessage);
                break;
        }

        return Task.FromResult<object>(new
        {
            success = true,
            level,
            category,
            message,
            timestamp = DateTime.UtcNow
        });
    }
}
