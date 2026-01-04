using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;

namespace AgentPay.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> GetByIdAsync(Guid id);
    Task<Transaction> CreateAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task<IEnumerable<Transaction>> GetByAgentIdAsync(Guid agentId);
    Task<IEnumerable<Transaction>> GetPendingTransactionsAsync();
}
