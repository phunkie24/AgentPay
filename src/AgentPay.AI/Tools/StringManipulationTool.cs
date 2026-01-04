using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;

namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for string manipulation and text processing
/// </summary>
public class StringManipulationTool : ITool
{
    public string Name => "string_manipulation";
    public string Description => "Perform string operations like formatting, parsing, and transformation";

    public Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var action = parameters.GetValueOrDefault("action")?.ToString();

        var result = action switch
        {
            "uppercase" => ToUpperCase(parameters),
            "lowercase" => ToLowerCase(parameters),
            "trim" => Trim(parameters),
            "replace" => Replace(parameters),
            "split" => Split(parameters),
            "concat" => Concat(parameters),
            "extract_numbers" => ExtractNumbers(parameters),
            "word_count" => WordCount(parameters),
            _ => throw new ArgumentException($"Unknown action: {action}")
        };

        return Task.FromResult<object>(result);
    }

    private object ToUpperCase(Dictionary<string, object> parameters)
    {
        var text = parameters.GetValueOrDefault("text")?.ToString() ?? throw new ArgumentException("text required");
        return new { original = text, result = text.ToUpper() };
    }

    private object ToLowerCase(Dictionary<string, object> parameters)
    {
        var text = parameters.GetValueOrDefault("text")?.ToString() ?? throw new ArgumentException("text required");
        return new { original = text, result = text.ToLower() };
    }

    private object Trim(Dictionary<string, object> parameters)
    {
        var text = parameters.GetValueOrDefault("text")?.ToString() ?? throw new ArgumentException("text required");
        return new { original = text, result = text.Trim() };
    }

    private object Replace(Dictionary<string, object> parameters)
    {
        var text = parameters.GetValueOrDefault("text")?.ToString() ?? throw new ArgumentException("text required");
        var oldValue = parameters.GetValueOrDefault("old_value")?.ToString() ?? throw new ArgumentException("old_value required");
        var newValue = parameters.GetValueOrDefault("new_value")?.ToString() ?? "";

        return new
        {
            original = text,
            replaced = oldValue,
            with = newValue,
            result = text.Replace(oldValue, newValue)
        };
    }

    private object Split(Dictionary<string, object> parameters)
    {
        var text = parameters.GetValueOrDefault("text")?.ToString() ?? throw new ArgumentException("text required");
        var delimiter = parameters.GetValueOrDefault("delimiter")?.ToString() ?? ",";

        var parts = text.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

        return new
        {
            original = text,
            delimiter,
            parts,
            count = parts.Length
        };
    }

    private object Concat(Dictionary<string, object> parameters)
    {
        var parts = parameters.GetValueOrDefault("parts") as IEnumerable<object> ?? throw new ArgumentException("parts required");
        var separator = parameters.GetValueOrDefault("separator")?.ToString() ?? "";

        var result = string.Join(separator, parts.Select(p => p?.ToString() ?? ""));

        return new
        {
            parts = parts.ToList(),
            separator,
            result
        };
    }

    private object ExtractNumbers(Dictionary<string, object> parameters)
    {
        var text = parameters.GetValueOrDefault("text")?.ToString() ?? throw new ArgumentException("text required");

        var numbers = Regex.Matches(text, @"-?\d+\.?\d*")
            .Select(m => m.Value)
            .ToList();

        return new
        {
            original = text,
            numbers,
            count = numbers.Count
        };
    }

    private object WordCount(Dictionary<string, object> parameters)
    {
        var text = parameters.GetValueOrDefault("text")?.ToString() ?? throw new ArgumentException("text required");

        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        return new
        {
            text,
            wordCount = words.Length,
            characterCount = text.Length,
            characterCountNoSpaces = text.Replace(" ", "").Length
        };
    }
}
