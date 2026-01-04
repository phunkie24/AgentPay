using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AgentPay.Infrastructure.Persistence.Configurations;

public class PaymentSessionConfiguration : IEntityTypeConfiguration<PaymentSession>
{
    public void Configure(EntityTypeBuilder<PaymentSession> builder)
    {
        builder.ToTable("PaymentSessions");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.AgentId)
            .IsRequired();

        builder.Property(ps => ps.ServiceId)
            .IsRequired();

        builder.Property(ps => ps.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ps => ps.StartedAt)
            .IsRequired();

        builder.Property(ps => ps.CompletedAt);

        // Value Object: BudgetLimit
        builder.OwnsOne(ps => ps.BudgetLimit, budget =>
        {
            budget.Property(b => b.Value)
                .IsRequired()
                .HasPrecision(18, 8)
                .HasColumnName("BudgetLimit");
        });

        // Value Object: NegotiatedPrice (nullable)
        builder.OwnsOne(ps => ps.NegotiatedPrice, negotiated =>
        {
            negotiated.Property(n => n.Value)
                .HasPrecision(18, 8)
                .HasColumnName("NegotiatedPrice");
        });

        // Collection: PaymentSteps
        builder.OwnsMany(ps => ps.Steps, step =>
        {
            step.ToTable("PaymentSteps");
            step.WithOwner().HasForeignKey("PaymentSessionId");

            step.Property<int>("Id")
                .ValueGeneratedOnAdd();

            step.HasKey("Id");

            step.Property(s => s.Type)
                .IsRequired()
                .HasConversion<string>();

            step.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(1000);

            step.Property(s => s.Data)
                .HasColumnType("nvarchar(max)");

            step.Property(s => s.Timestamp)
                .IsRequired();
        });

        // Indexes
        builder.HasIndex(ps => ps.AgentId);
        builder.HasIndex(ps => ps.ServiceId);
        builder.HasIndex(ps => ps.Status);
        builder.HasIndex(ps => ps.StartedAt);
    }
}
