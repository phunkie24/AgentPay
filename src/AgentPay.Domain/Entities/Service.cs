using System;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Domain.Entities;

public class Service
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public WalletAddress ProviderAddress { get; private set; }
    public MNEEAmount ListedPrice { get; private set; }
    public ServiceCategory Category { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private Service() { }

    public static Service Create(
        string name,
        string description,
        WalletAddress providerAddress,
        MNEEAmount listedPrice,
        ServiceCategory category)
    {
        return new Service
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ProviderAddress = providerAddress,
            ListedPrice = listedPrice,
            Category = category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(MNEEAmount newPrice)
    {
        ListedPrice = newPrice;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public enum ServiceCategory
{
    DataAPI,
    ComputeResource,
    AIModel,
    Storage,
    Analytics,
    Other
}
