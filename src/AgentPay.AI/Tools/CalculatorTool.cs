using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for performing mathematical calculations
/// </summary>
public class CalculatorTool : ITool
{
    public string Name => "calculator";
    public string Description => "Perform mathematical calculations including basic arithmetic, percentages, and financial calculations";

    public Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var operation = parameters.GetValueOrDefault("operation")?.ToString();

        var result = operation switch
        {
            "add" => Add(parameters),
            "subtract" => Subtract(parameters),
            "multiply" => Multiply(parameters),
            "divide" => Divide(parameters),
            "percentage" => Percentage(parameters),
            "compound_interest" => CompoundInterest(parameters),
            _ => throw new ArgumentException($"Unknown operation: {operation}")
        };

        return Task.FromResult<object>(result);
    }

    private object Add(Dictionary<string, object> parameters)
    {
        var a = Convert.ToDouble(parameters.GetValueOrDefault("a") ?? throw new ArgumentException("a required"));
        var b = Convert.ToDouble(parameters.GetValueOrDefault("b") ?? throw new ArgumentException("b required"));
        return new { operation = "add", result = a + b };
    }

    private object Subtract(Dictionary<string, object> parameters)
    {
        var a = Convert.ToDouble(parameters.GetValueOrDefault("a") ?? throw new ArgumentException("a required"));
        var b = Convert.ToDouble(parameters.GetValueOrDefault("b") ?? throw new ArgumentException("b required"));
        return new { operation = "subtract", result = a - b };
    }

    private object Multiply(Dictionary<string, object> parameters)
    {
        var a = Convert.ToDouble(parameters.GetValueOrDefault("a") ?? throw new ArgumentException("a required"));
        var b = Convert.ToDouble(parameters.GetValueOrDefault("b") ?? throw new ArgumentException("b required"));
        return new { operation = "multiply", result = a * b };
    }

    private object Divide(Dictionary<string, object> parameters)
    {
        var a = Convert.ToDouble(parameters.GetValueOrDefault("a") ?? throw new ArgumentException("a required"));
        var b = Convert.ToDouble(parameters.GetValueOrDefault("b") ?? throw new ArgumentException("b required"));

        if (b == 0)
            throw new ArgumentException("Cannot divide by zero");

        return new { operation = "divide", result = a / b };
    }

    private object Percentage(Dictionary<string, object> parameters)
    {
        var value = Convert.ToDouble(parameters.GetValueOrDefault("value") ?? throw new ArgumentException("value required"));
        var percentage = Convert.ToDouble(parameters.GetValueOrDefault("percentage") ?? throw new ArgumentException("percentage required"));

        var result = (value * percentage) / 100;
        return new { operation = "percentage", value, percentage, result };
    }

    private object CompoundInterest(Dictionary<string, object> parameters)
    {
        var principal = Convert.ToDouble(parameters.GetValueOrDefault("principal") ?? throw new ArgumentException("principal required"));
        var rate = Convert.ToDouble(parameters.GetValueOrDefault("rate") ?? throw new ArgumentException("rate required"));
        var time = Convert.ToDouble(parameters.GetValueOrDefault("time") ?? throw new ArgumentException("time required"));
        var frequency = Convert.ToInt32(parameters.GetValueOrDefault("frequency") ?? 1); // times per year

        var amount = principal * Math.Pow(1 + (rate / 100 / frequency), frequency * time);
        var interest = amount - principal;

        return new
        {
            operation = "compound_interest",
            principal,
            rate,
            time,
            frequency,
            totalAmount = Math.Round(amount, 2),
            interestEarned = Math.Round(interest, 2)
        };
    }
}
