using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for validating various types of data
/// </summary>
public class ValidationTool : ITool
{
    public string Name => "validation";
    public string Description => "Validate emails, URLs, wallet addresses, and other data formats";

    public Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var type = parameters.GetValueOrDefault("type")?.ToString();

        var result = type switch
        {
            "email" => ValidateEmail(parameters),
            "url" => ValidateUrl(parameters),
            "wallet_address" => ValidateWalletAddress(parameters),
            "phone" => ValidatePhone(parameters),
            "number_range" => ValidateNumberRange(parameters),
            _ => throw new ArgumentException($"Unknown validation type: {type}")
        };

        return Task.FromResult<object>(result);
    }

    private object ValidateEmail(Dictionary<string, object> parameters)
    {
        var email = parameters.GetValueOrDefault("email")?.ToString() ?? throw new ArgumentException("email required");

        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        var isValid = Regex.IsMatch(email, emailRegex);

        return new
        {
            email,
            isValid,
            type = "email"
        };
    }

    private object ValidateUrl(Dictionary<string, object> parameters)
    {
        var url = parameters.GetValueOrDefault("url")?.ToString() ?? throw new ArgumentException("url required");

        var isValid = Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        return new
        {
            url,
            isValid,
            scheme = uriResult?.Scheme,
            host = uriResult?.Host,
            type = "url"
        };
    }

    private object ValidateWalletAddress(Dictionary<string, object> parameters)
    {
        var address = parameters.GetValueOrDefault("address")?.ToString() ?? throw new ArgumentException("address required");

        // Ethereum address validation: 0x followed by 40 hexadecimal characters
        var ethRegex = @"^0x[a-fA-F0-9]{40}$";
        var isValid = Regex.IsMatch(address, ethRegex);

        return new
        {
            address,
            isValid,
            format = "ethereum",
            type = "wallet_address"
        };
    }

    private object ValidatePhone(Dictionary<string, object> parameters)
    {
        var phone = parameters.GetValueOrDefault("phone")?.ToString() ?? throw new ArgumentException("phone required");

        // Basic phone validation: digits, spaces, dashes, parentheses, plus sign
        var phoneRegex = @"^[\d\s\-\(\)\+]+$";
        var isValid = Regex.IsMatch(phone, phoneRegex);

        var digitsOnly = Regex.Replace(phone, @"[^\d]", "");
        var hasValidLength = digitsOnly.Length >= 10 && digitsOnly.Length <= 15;

        return new
        {
            phone,
            isValid = isValid && hasValidLength,
            digitsOnly,
            digitCount = digitsOnly.Length,
            type = "phone"
        };
    }

    private object ValidateNumberRange(Dictionary<string, object> parameters)
    {
        var value = Convert.ToDouble(parameters.GetValueOrDefault("value") ?? throw new ArgumentException("value required"));
        var min = Convert.ToDouble(parameters.GetValueOrDefault("min") ?? double.MinValue);
        var max = Convert.ToDouble(parameters.GetValueOrDefault("max") ?? double.MaxValue);

        var isValid = value >= min && value <= max;

        return new
        {
            value,
            min,
            max,
            isValid,
            type = "number_range"
        };
    }
}
