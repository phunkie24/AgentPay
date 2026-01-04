using System;
using System.ComponentModel.DataAnnotations;

namespace AgentPay.Application.DTOs;

public class InitiatePaymentRequest
{
    [Required]
    public Guid AgentId { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    [Range(0.000001, double.MaxValue)]
    public decimal Amount { get; set; }

    [StringLength(2000)]
    public string Reasoning { get; set; }
}
