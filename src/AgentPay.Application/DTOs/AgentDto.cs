using System;

namespace AgentPay.Application.DTOs;

public class AgentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string WalletAddress { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public Guid ServiceId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionHash { get; set; }
    public string Status { get; set; }
    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
