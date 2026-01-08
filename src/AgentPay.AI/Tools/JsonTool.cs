using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for working with JSON data
/// </summary>
public class JsonTool : ITool
{
    public string Name => "json";
    public string Description => "Parse, validate, and manipulate JSON data";

    public Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var action = parameters.GetValueOrDefault("action")?.ToString();

        var result = action switch
        {
            "parse" => ParseJson(parameters),
            "stringify" => StringifyJson(parameters),
            "validate" => ValidateJson(parameters),
            "extract_value" => ExtractValue(parameters),
            "merge" => MergeJson(parameters),
            _ => throw new ArgumentException($"Unknown action: {action}")
        };

        return Task.FromResult<object>(result);
    }

    private object ParseJson(Dictionary<string, object> parameters)
    {
        var jsonString = parameters.GetValueOrDefault("json")?.ToString() ?? throw new ArgumentException("json required");

        try
        {
            var parsed = JsonSerializer.Deserialize<object>(jsonString);
            return new
            {
                success = true,
                parsed,
                type = parsed?.GetType().Name
            };
        }
        catch (JsonException ex)
        {
            return new
            {
                success = false,
                error = ex.Message
            };
        }
    }

    private object StringifyJson(Dictionary<string, object> parameters)
    {
        var data = parameters.GetValueOrDefault("data") ?? throw new ArgumentException("data required");
        var indent = Convert.ToBoolean(parameters.GetValueOrDefault("indent") ?? false);

        var options = new JsonSerializerOptions
        {
            WriteIndented = indent
        };

        var json = JsonSerializer.Serialize(data, options);

        return new
        {
            success = true,
            json,
            length = json.Length
        };
    }

    private object ValidateJson(Dictionary<string, object> parameters)
    {
        var jsonString = parameters.GetValueOrDefault("json")?.ToString() ?? throw new ArgumentException("json required");

        try
        {
            JsonDocument.Parse(jsonString);
            return new
            {
                valid = true,
                message = "JSON is valid"
            };
        }
        catch (JsonException ex)
        {
            return new
            {
                valid = false,
                error = ex.Message
            };
        }
    }

    private object ExtractValue(Dictionary<string, object> parameters)
    {
        var jsonString = parameters.GetValueOrDefault("json")?.ToString() ?? throw new ArgumentException("json required");
        var path = parameters.GetValueOrDefault("path")?.ToString() ?? throw new ArgumentException("path required");

        try
        {
            using var doc = JsonDocument.Parse(jsonString);
            var element = NavigateJsonPath(doc.RootElement, path);

            return new
            {
                success = true,
                path,
                value = element.ToString(),
                type = element.ValueKind.ToString()
            };
        }
        catch (Exception ex)
        {
            return new
            {
                success = false,
                error = ex.Message
            };
        }
    }

    private object MergeJson(Dictionary<string, object> parameters)
    {
        var json1 = parameters.GetValueOrDefault("json1")?.ToString() ?? throw new ArgumentException("json1 required");
        var json2 = parameters.GetValueOrDefault("json2")?.ToString() ?? throw new ArgumentException("json2 required");

        try
        {
            var dict1 = JsonSerializer.Deserialize<Dictionary<string, object>>(json1);
            var dict2 = JsonSerializer.Deserialize<Dictionary<string, object>>(json2);

            foreach (var kvp in dict2)
            {
                dict1[kvp.Key] = kvp.Value;
            }

            var merged = JsonSerializer.Serialize(dict1, new JsonSerializerOptions { WriteIndented = true });

            return new
            {
                success = true,
                merged
            };
        }
        catch (Exception ex)
        {
            return new
            {
                success = false,
                error = ex.Message
            };
        }
    }

    private JsonElement NavigateJsonPath(JsonElement element, string path)
    {
        var parts = path.Split('.');
        var current = element;

        foreach (var part in parts)
        {
            current = current.GetProperty(part);
        }

        return current;
    }
}
