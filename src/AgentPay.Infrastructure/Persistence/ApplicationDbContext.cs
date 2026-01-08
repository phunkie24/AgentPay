using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgentPay.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Agent> Agents { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<PaymentSession> PaymentSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
