using System;
using System.Threading.Tasks;

namespace AgentPay.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IAgentRepository Agents { get; }
    ITransactionRepository Transactions { get; }
    IServiceRepository Services { get; }
    Task<int> CommitAsync();
    Task RollbackAsync();
}
