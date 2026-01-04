using System;

namespace AgentPay.Application.DTOs;

public class TransactionReceiptDto
{
    public string TransactionHash { get; set; }
    public string BlockHash { get; set; }
    public long BlockNumber { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public decimal GasUsed { get; set; }
    public decimal GasPrice { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
