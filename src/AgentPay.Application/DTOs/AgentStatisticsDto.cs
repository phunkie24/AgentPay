using System;

namespace AgentPay.Application.DTOs;

public class AgentStatisticsDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; }
    public int TotalTransactions { get; set; }
    public int CompletedTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public decimal TotalAmountSpent { get; set; }
    public decimal CurrentBalance { get; set; }
    public int ActiveSessionsCount { get; set; }
    public int ReflectionsCount { get; set; }
    public double SuccessRate => TotalTransactions > 0 ? (double)CompletedTransactions / TotalTransactions * 100 : 0;
}
