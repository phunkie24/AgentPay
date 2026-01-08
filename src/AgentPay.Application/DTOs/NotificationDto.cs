using System;

namespace AgentPay.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
    public string Severity { get; set; } // Info, Warning, Error
}
