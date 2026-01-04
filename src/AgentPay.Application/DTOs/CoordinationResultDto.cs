using System;
using System.Collections.Generic;

namespace AgentPay.Application.DTOs;

public class CoordinationResultDto
{
    public Guid CoordinationId { get; set; }
    public List<Guid> ParticipatingAgents { get; set; } = new();
    public string Objective { get; set; }
    public string Status { get; set; }
    public List<string> Actions { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccessful { get; set; }
}
