using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AgentPay.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.AgentId)
            .IsRequired();

        builder.Property(t => t.ServiceId)
            .IsRequired();

        builder.Property(t => t.Reasoning)
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.FailureReason)
            .HasMaxLength(1000);

        builder.Property(t => t.GasUsed)
            .HasDefaultValue(0);

        builder.Property(t => t.GasPriceGwei)
            .HasPrecision(18, 8)
            .HasDefaultValue(0m);

        builder.Property(t => t.InitiatedAt)
            .IsRequired();

        builder.Property(t => t.CompletedAt);

        builder.Property(t => t.FailedAt);

        // Value Object: MNEEAmount
        builder.OwnsOne(t => t.Amount, amount =>
        {
            amount.Property(a => a.Value)
                .IsRequired()
                .HasPrecision(18, 8)
                .HasColumnName("Amount");
        });

        // Value Object: TransactionHash
        builder.OwnsOne(t => t.Hash, hash =>
        {
            hash.Property(h => h.Value)
                .HasMaxLength(66)
                .HasColumnName("TransactionHash");
        });

        // Value Object: FromAddress
        builder.OwnsOne(t => t.FromAddress, from =>
        {
            from.Property(f => f.Value)
                .IsRequired()
                .HasMaxLength(42)
                .HasColumnName("FromAddress");
        });

        // Value Object: ToAddress
        builder.OwnsOne(t => t.ToAddress, to =>
        {
            to.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(42)
                .HasColumnName("ToAddress");
        });

        // Complex Object: VerificationResult
        builder.OwnsOne(t => t.VerificationResult, verification =>
        {
            verification.Property(v => v.IsVerified)
                .HasColumnName("IsVerified")
                .HasDefaultValue(false);

            verification.Property(v => v.FailureReason)
                .HasColumnName("VerificationFailureReason")
                .HasMaxLength(1000);

            verification.Property(v => v.Metadata)
                .HasColumnName("VerificationMetadata")
                .HasConversion(
                    v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions)null) : null,
                    v => v != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null) : null)
                .HasColumnType("nvarchar(max)");
        });

        // Complex Object: GuardrailsCheck
        builder.OwnsOne(t => t.GuardrailsCheck, guardrails =>
        {
            guardrails.Property(g => g.CheckedAt)
                .HasColumnName("GuardrailsCheckedAt");

            guardrails.Property(g => g.Checks)
                .HasColumnName("GuardrailsChecks")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<GuardrailCheck>>(v, (JsonSerializerOptions)null))
                .HasColumnType("nvarchar(max)");

            guardrails.Ignore(g => g.Passed);
            guardrails.Ignore(g => g.FailureReason);
        });

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);

        // Indexes
        builder.HasIndex(t => t.AgentId);
        builder.HasIndex(t => t.ServiceId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.InitiatedAt);
        builder.HasIndex(t => new { t.AgentId, t.Status });
    }
}
