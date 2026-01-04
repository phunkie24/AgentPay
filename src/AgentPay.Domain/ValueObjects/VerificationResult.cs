using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentPay.Domain.ValueObjects;

public class VerificationResult
{
    public bool IsVerified { get; private set; }
    public string Reason { get; private set; }
    public VerificationLevel Level { get; private set; }
    public List<VerificationCheck> Checks { get; private set; } = new();
    public Dictionary<string, object> Metadata { get; private set; } = new();
    public DateTime VerifiedAt { get; private set; }
    public string VerifiedBy { get; private set; }

    private VerificationResult() { }

    public static VerificationResult Success(
        VerificationLevel level,
        string verifiedBy,
        List<VerificationCheck> checks = null)
    {
        return new VerificationResult
        {
            IsVerified = true,
            Reason = "All verification checks passed",
            Level = level,
            Checks = checks ?? new List<VerificationCheck>(),
            VerifiedAt = DateTime.UtcNow,
            VerifiedBy = verifiedBy
        };
    }

    public static VerificationResult Failure(
        string reason,
        VerificationLevel level,
        string verifiedBy,
        List<VerificationCheck> checks = null)
    {
        return new VerificationResult
        {
            IsVerified = false,
            Reason = reason,
            Level = level,
            Checks = checks ?? new List<VerificationCheck>(),
            VerifiedAt = DateTime.UtcNow,
            VerifiedBy = verifiedBy
        };
    }

    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
    }

    public bool HasFailedChecks => Checks.Any(c => !c.Passed);
    public List<VerificationCheck> FailedChecks => Checks.Where(c => !c.Passed).ToList();
    public List<VerificationCheck> PassedChecks => Checks.Where(c => c.Passed).ToList();
}

public enum VerificationLevel
{
    None = 0,
    Basic = 1,
    Standard = 2,
    Enhanced = 3,
    Maximum = 4
}

public record VerificationCheck(
    string CheckName,
    bool Passed,
    string Message,
    DateTime CheckedAt,
    Dictionary<string, object> Details = null)
{
    public Dictionary<string, object> Details { get; init; } = Details ?? new Dictionary<string, object>();
}
