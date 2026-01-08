using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for generating random data
/// </summary>
public class RandomGeneratorTool : ITool
{
    private readonly Random _random = new();

    public string Name => "random_generator";
    public string Description => "Generate random numbers, strings, and selections";

    public Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var type = parameters.GetValueOrDefault("type")?.ToString();

        var result = type switch
        {
            "number" => GenerateRandomNumber(parameters),
            "string" => GenerateRandomString(parameters),
            "choice" => MakeRandomChoice(parameters),
            "uuid" => GenerateUuid(),
            "boolean" => GenerateBoolean(),
            _ => throw new ArgumentException($"Unknown type: {type}")
        };

        return Task.FromResult<object>(result);
    }

    private object GenerateRandomNumber(Dictionary<string, object> parameters)
    {
        var min = Convert.ToInt32(parameters.GetValueOrDefault("min") ?? 0);
        var max = Convert.ToInt32(parameters.GetValueOrDefault("max") ?? 100);

        var number = _random.Next(min, max + 1);

        return new
        {
            type = "number",
            min,
            max,
            result = number
        };
    }

    private object GenerateRandomString(Dictionary<string, object> parameters)
    {
        var length = Convert.ToInt32(parameters.GetValueOrDefault("length") ?? 10);
        var charset = parameters.GetValueOrDefault("charset")?.ToString() ?? "alphanumeric";

        string chars = charset.ToLower() switch
        {
            "alphanumeric" => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
            "alpha" => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",
            "numeric" => "0123456789",
            "hex" => "0123456789ABCDEF",
            _ => charset
        };

        var result = new string(Enumerable.Range(0, length)
            .Select(_ => chars[_random.Next(chars.Length)])
            .ToArray());

        return new
        {
            type = "string",
            length,
            charset,
            result
        };
    }

    private object MakeRandomChoice(Dictionary<string, object> parameters)
    {
        var choices = parameters.GetValueOrDefault("choices") as IEnumerable<object> ?? throw new ArgumentException("choices required");
        var choicesList = choices.ToList();

        if (!choicesList.Any())
            throw new ArgumentException("choices cannot be empty");

        var selected = choicesList[_random.Next(choicesList.Count)];

        return new
        {
            type = "choice",
            choices = choicesList,
            result = selected
        };
    }

    private object GenerateUuid()
    {
        var uuid = Guid.NewGuid();

        return new
        {
            type = "uuid",
            result = uuid.ToString()
        };
    }

    private object GenerateBoolean()
    {
        var result = _random.Next(2) == 1;

        return new
        {
            type = "boolean",
            result
        };
    }
}
