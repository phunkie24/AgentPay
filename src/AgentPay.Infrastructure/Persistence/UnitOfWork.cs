using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Repositories;
using AgentPay.Infrastructure.Persistence.Repositories;

namespace AgentPay.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IAgentRepository Agents { get; }
    public ITransactionRepository Transactions { get; }
    public IServiceRepository Services { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Agents = new AgentRepository(context);
        Transactions = new TransactionRepository(context);
        Services = new ServiceRepository(context);
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task RollbackAsync()
    {
        await Task.CompletedTask;
        // EF Core doesn't need explicit rollback
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
