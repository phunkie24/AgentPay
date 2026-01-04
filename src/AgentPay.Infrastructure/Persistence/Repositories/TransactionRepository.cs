using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgentPay.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction> GetByIdAsync(Guid id)
    {
        return await _context.Transactions.FindAsync(id);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Transaction>> GetByAgentIdAsync(Guid agentId)
    {
        return await _context.Transactions
            .Where(t => t.AgentId == agentId)
            .OrderByDescending(t => t.InitiatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetPendingTransactionsAsync()
    {
        return await _context.Transactions
            .Where(t => t.Status == TransactionStatus.Pending)
            .ToListAsync();
    }
}
