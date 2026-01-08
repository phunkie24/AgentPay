using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentPay.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        // Value Object: ProviderAddress
        builder.OwnsOne(s => s.ProviderAddress, provider =>
        {
            provider.Property(p => p.Value)
                .IsRequired()
                .HasMaxLength(42)
                .HasColumnName("ProviderAddress");
        });

        // Value Object: ListedPrice
        builder.OwnsOne(s => s.ListedPrice, price =>
        {
            price.Property(p => p.Value)
                .IsRequired()
                .HasPrecision(18, 8)
                .HasColumnName("ListedPrice");
        });

        // Indexes
        builder.HasIndex(s => s.Category);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => new { s.Category, s.IsActive });
    }
}
