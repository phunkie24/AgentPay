#!/bin/bash

# Complete Code Generation Script for AgentPay
# Generates ALL remaining files for MNEE Hackathon submission

echo "🚀 Generating Complete AgentPay Codebase..."
echo "=============================================="

# Create all remaining C# files
cat > src/AgentPay.Domain/ValueObjects/WalletAddress.cs << 'EOF'
namespace AgentPay.Domain.ValueObjects;

public record WalletAddress
{
    public string Value { get; init; }

    private WalletAddress(string value) => Value = value;

    public static WalletAddress Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Wallet address cannot be empty");

        if (!address.StartsWith("0x"))
            throw new ArgumentException("Invalid Ethereum address format");

        if (address.Length != 42)
            throw new ArgumentException("Ethereum address must be 42 characters");

        return new WalletAddress(address);
    }

    public override string ToString() => Value;
}
EOF

cat > src/AgentPay.Domain/ValueObjects/TransactionHash.cs << 'EOF'
namespace AgentPay.Domain.ValueObjects;

public record TransactionHash
{
    public string Value { get; init; }

    private TransactionHash(string value) => Value = value;

    public static TransactionHash Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Transaction hash cannot be empty");

        if (!hash.StartsWith("0x"))
            throw new ArgumentException("Invalid transaction hash format");

        return new TransactionHash(hash);
    }

    public override string ToString() => Value;
}
EOF

cat > src/AgentPay.Domain/Events/DomainEvents.cs << 'EOF'
namespace AgentPay.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public record AgentCreatedEvent(Guid AgentId, string Name, AgentRole Role) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record PaymentInitiatedEvent(Guid TransactionId, Guid AgentId, decimal Amount) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record PaymentCompletedEvent(Guid TransactionId, string TransactionHash, decimal Amount) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record PaymentFailedEvent(Guid TransactionId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record TransactionBlockedEvent(Guid TransactionId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record AgentActivatedEvent(Guid AgentId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record AgentDeactivatedEvent(Guid AgentId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record BalanceUpdatedEvent(Guid AgentId, decimal OldBalance, decimal NewBalance) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record ReflectionCreatedEvent(Guid AgentId, Guid ReflectionId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
EOF

cat > src/AgentPay.Infrastructure/Persistence/ApplicationDbContext.cs << 'EOF'
using AgentPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgentPay.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Agent> Agents { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
EOF

cat > src/AgentPay.Infrastructure/Blockchain/MNEEContractService.cs << 'EOF'
using Nethereum.Web3;
using Nethereum.Contracts;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Infrastructure.Blockchain;

public class MNEEContractService
{
    private readonly Web3 _web3;
    private readonly string _contractAddress;
    private readonly Contract _contract;

    public MNEEContractService(string rpcUrl, string contractAddress)
    {
        _web3 = new Web3(rpcUrl);
        _contractAddress = contractAddress;
        
        // MNEE ERC20 ABI
        var abi = @"[{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":"""",""type"":""bool""}],""type"":""function""}]";
        
        _contract = _web3.Eth.GetContract(abi, _contractAddress);
    }

    public async Task<decimal> GetBalanceAsync(string address)
    {
        var balanceFunction = _contract.GetFunction("balanceOf");
        var balance = await balanceFunction.CallAsync<BigInteger>(address);
        return Web3.Convert.FromWei(balance);
    }

    public async Task<string> TransferAsync(string fromAddress, string toAddress, decimal amount)
    {
        var transferFunction = _contract.GetFunction("transfer");
        var amountInWei = Web3.Convert.ToWei(amount);
        
        var receipt = await transferFunction.SendTransactionAndWaitForReceiptAsync(
            fromAddress,
            new HexBigInteger(300000),
            new HexBigInteger(0),
            null,
            toAddress,
            amountInWei);
        
        return receipt.TransactionHash;
    }
}
EOF

cat > src/AgentPay.AI/Services/LLMService.cs << 'EOF'
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
        
        var request = new GenerateRequest
        {
            Model = _model,
            Prompt = prompt,
            Options = new RequestOptions
            {
                Temperature = options.Temperature,
                NumPredict = options.MaxTokens
            }
        };

        var response = await _client.Generate(request);
        
        return new LLMResponse
        {
            Text = response.Response,
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
EOF

cat > src/AgentPay.AI/Tools/IToolRegistry.cs << 'EOF'
namespace AgentPay.AI.Tools;

public interface IToolRegistry
{
    ITool GetTool(string name);
    List<ToolDefinition> GetAvailableTools();
    Task<object> ExecuteAsync(string toolName, Dictionary<string, object> parameters);
}

public interface ITool
{
    string Name { get; }
    string Description { get; }
    Task<object> ExecuteAsync(Dictionary<string, object> parameters);
}

public class ToolDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, ParameterDefinition> Parameters { get; set; }
}

public class ParameterDefinition
{
    public string Type { get; set; }
    public string Description { get; set; }
    public bool Required { get; set; }
}

public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new();

    public void RegisterTool(ITool tool)
    {
        _tools[tool.Name] = tool;
    }

    public ITool GetTool(string name)
    {
        return _tools.GetValueOrDefault(name) 
            ?? throw new ArgumentException($"Tool '{name}' not found");
    }

    public List<ToolDefinition> GetAvailableTools()
    {
        return _tools.Values.Select(t => new ToolDefinition
        {
            Name = t.Name,
            Description = t.Description
        }).ToList();
    }

    public async Task<object> ExecuteAsync(string toolName, Dictionary<string, object> parameters)
    {
        var tool = GetTool(toolName);
        return await tool.ExecuteAsync(parameters);
    }
}
EOF

cat > src/AgentPay.Web/Program.cs << 'EOF'
using AgentPay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Connection"];
});

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
EOF

cat > src/AgentPay.Web/Controllers/HomeController.cs << 'EOF'
using Microsoft.AspNetCore.Mvc;

namespace AgentPay.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        return View();
    }
}
EOF

cat > src/AgentPay.Web/Views/Home/Index.cshtml << 'EOF'
@{
    ViewData["Title"] = "AgentPay - Autonomous AI Payment Infrastructure";
}

<div class="hero">
    <h1>🤖 AgentPay</h1>
    <h2>Autonomous AI Agent Payment Infrastructure</h2>
    <p>Enable your AI agents to discover, negotiate, and execute payments using MNEE stablecoin</p>
    <a href="/Dashboard" class="btn-primary">Get Started</a>
</div>

<div class="features">
    <div class="feature-card">
        <h3>🧠 33 GenAI Patterns</h3>
        <p>Complete implementation of all agentic AI patterns</p>
    </div>
    <div class="feature-card">
        <h3>💰 MNEE Integration</h3>
        <p>Native stablecoin payments on Ethereum</p>
    </div>
    <div class="feature-card">
        <h3>🔒 Enterprise Security</h3>
        <p>Guardrails and self-check patterns</p>
    </div>
</div>
EOF

echo "✅ All core C# files generated"
echo ""
echo "📊 Summary:"
find src -name "*.cs" | wc -l | xargs echo "   - C# files:"
find . -name "*.cshtml" | wc -l | xargs echo "   - View files:"
echo ""
