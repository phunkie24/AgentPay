using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OllamaSharp;

namespace AgentPay.AI.Services;

public interface ILLMService
{
    Task<LLMResponse> GenerateAsync(string prompt, LLMOptions? options = null);
    Task<List<TokenProbability>> GetTokenProbabilitiesAsync(string text);
}

public class OllamaLLMService : ILLMService
{
    private readonly OllamaApiClient _client;
    private readonly string _model;

    public OllamaLLMService(string baseUrl, string model = "llama3.2:latest")
    {
        _client = new OllamaApiClient(baseUrl);
        _model = model;
    }

    public async Task<LLMResponse> GenerateAsync(string prompt, LLMOptions? options = null)
    {
        options ??= new LLMOptions();

        // OllamaSharp 3.0+ API - Create GenerateRequest
        var request = new OllamaSharp.Models.GenerateRequest
        {
            Model = _model,
            Prompt = prompt,
            Stream = false
        };

        var responseText = string.Empty;
        await foreach (var responseChunk in _client.Generate(request))
        {
            responseText += responseChunk?.Response ?? string.Empty;
        }

        return new LLMResponse
        {
            Text = responseText,
            Model = _model
        };
    }

    public async Task<List<TokenProbability>> GetTokenProbabilitiesAsync(string text)
    {
        // Simplified - in production would use actual token probabilities
        var tokens = text.Split(' ');
        return tokens.Select(t => new TokenProbability
        {
            Token = t,
            Probability = 0.85 // Default high probability
        }).ToList();
    }
}

public class LLMOptions
{
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 4096;
    public string? ResponseFormat { get; set; }
    public string? SystemPrompt { get; set; }
}

public class LLMResponse
{
    public string Text { get; set; }
    public string Model { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class TokenProbability
{
    public string Token { get; set; }
    public double Probability { get; set; }
}
