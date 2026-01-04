using System.ComponentModel.DataAnnotations;

namespace AgentPay.Application.DTOs;

public class CreateAgentRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    public string Role { get; set; }

    [Required]
    [RegularExpression(@"^0x[a-fA-F0-9]{40}$", ErrorMessage = "Invalid Ethereum address")]
    public string WalletAddress { get; set; }

    [Range(0, double.MaxValue)]
    public decimal InitialBalance { get; set; } = 0;
}
