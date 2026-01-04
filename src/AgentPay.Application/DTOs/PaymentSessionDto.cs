using System;
using System.Collections.Generic;

namespace AgentPay.Application.DTOs;

public class PaymentSessionDto
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public Guid ServiceId { get; set; }
    public string Status { get; set; }
    public decimal BudgetLimit { get; set; }
    public decimal? NegotiatedPrice { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<PaymentStepDto> Steps { get; set; } = new();
}

public class PaymentStepDto
{
    public string Type { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string Data { get; set; }
}
