using AgentPay.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AgentPay.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var agents = await _mediator.Send(new GetActiveAgentsQuery());
        var transactions = await _mediator.Send(new GetPendingTransactionsQuery());

        ViewBag.ActiveAgents = agents.Count;
        ViewBag.PendingTransactions = transactions.Count;

        return View();
    }

    public async Task<IActionResult> Agents()
    {
        var agents = await _mediator.Send(new GetActiveAgentsQuery());
        return View(agents);
    }

    public async Task<IActionResult> Transactions()
    {
        var transactions = await _mediator.Send(new GetPendingTransactionsQuery());
        return View(transactions);
    }

    public IActionResult Analytics()
    {
        return View();
    }
}
