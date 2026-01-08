using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AgentPay.Infrastructure.Persistence.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.ToTable("Agents");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>();

        // Value Object: WalletAddress
        builder.OwnsOne(a => a.WalletAddress, wallet =>
        {
            wallet.Property(w => w.Value)
                .IsRequired()
                .HasMaxLength(42)
                .HasColumnName("WalletAddress");
        });

        // Complex Object: AgentCapabilities
        builder.OwnsOne(a => a.Capabilities, capabilities =>
        {
            capabilities.Property(c => c.CanNegotiate)
                .HasColumnName("CanNegotiate");

            capabilities.Property(c => c.CanPlan)
                .HasColumnName("CanPlan");

            capabilities.Property(c => c.CanReflect)
                .HasColumnName("CanReflect");

            capabilities.Property(c => c.MaxTransactionAmount)
                .HasColumnName("MaxTransactionAmount")
                .HasPrecision(18, 8);

            capabilities.Property(c => c.AllowedServiceCategories)
                .HasColumnName("AllowedServiceCategories")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
                .HasColumnType("nvarchar(max)");

            capabilities.Property(c => c.EnabledTools)
                .HasColumnName("EnabledTools")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
                .HasColumnType("nvarchar(max)");
        });

        // Complex Object: LongTermMemory
        builder.OwnsOne(a => a.LongTermMemory, memory =>
        {
            // Store as JSON
            memory.Property<string>("_serializedData")
                .HasColumnName("LongTermMemory")
                .HasColumnType("nvarchar(max)");
        });

        builder.Property(a => a.MNEEBalance)
            .IsRequired()
            .HasPrecision(18, 8);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.LastActiveAt);

        // Sessions - owned collection
        builder.OwnsMany(a => a.Sessions, session =>
        {
            session.ToTable("AgentSessions");
            session.WithOwner().HasForeignKey("AgentId");
            session.HasKey("Id");

            session.Property(s => s.Id)
                .HasColumnName("Id");

            session.Property(s => s.Purpose)
                .HasMaxLength(500);

            session.Property(s => s.StartedAt);
            session.Property(s => s.EndedAt);

            session.Property(s => s.Status)
                .HasConversion<string>();

            session.OwnsMany(s => s.Messages, message =>
            {
                message.ToTable("SessionMessages");
                message.WithOwner().HasForeignKey("SessionId");

                message.Property<int>("Id").ValueGeneratedOnAdd();
                message.HasKey("Id");

                message.Property(m => m.Role).HasMaxLength(50);
                message.Property(m => m.Content).HasColumnType("nvarchar(max)");
                message.Property(m => m.Timestamp);
            });
        });

        // Reflections - owned collection
        builder.OwnsMany(a => a.Reflections, reflection =>
        {
            reflection.ToTable("AgentReflections");
            reflection.WithOwner().HasForeignKey("AgentId");
            reflection.HasKey(r => r.Id);

            reflection.Property(r => r.Insights)
                .HasMaxLength(2000);

            reflection.Property(r => r.Learnings)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
                .HasColumnType("nvarchar(max)");

            reflection.Property(r => r.Timestamp);

            reflection.OwnsOne(r => r.Action, action =>
            {
                action.Property(a => a.Type)
                    .HasColumnName("ActionType")
                    .HasMaxLength(100);

                action.Property(a => a.Description)
                    .HasColumnName("ActionDescription")
                    .HasMaxLength(1000);

                action.Property(a => a.Parameters)
                    .HasColumnName("ActionParameters")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null))
                    .HasColumnType("nvarchar(max)");
            });

            reflection.OwnsOne(r => r.Outcome, outcome =>
            {
                outcome.Property(o => o.Success)
                    .HasColumnName("OutcomeSuccess");

                outcome.Property(o => o.Result)
                    .HasColumnName("OutcomeResult")
                    .HasMaxLength(1000);

                outcome.Property(o => o.Metrics)
                    .HasColumnName("OutcomeMetrics")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null))
                    .HasColumnType("nvarchar(max)");
            });
        });

        // Ignore domain events (not persisted)
        builder.Ignore(a => a.DomainEvents);

        // Indexes
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Role);
        builder.HasIndex(a => a.CreatedAt);
    }
}
