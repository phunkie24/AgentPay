using System;
using System.Collections.Generic;

namespace AgentPay.Application.DTOs;

public class GuardrailsCheckDto
{
    public Guid TransactionId { get; set; }
    public bool Passed { get; set; }
    public List<CheckResultDto> Checks { get; set; } = new();
    public DateTime CheckedAt { get; set; }
    public string FailureReason { get; set; }
}

public class CheckResultDto
{
    public string CheckName { get; set; }
    public bool Passed { get; set; }
    public string Reason { get; set; }
}
