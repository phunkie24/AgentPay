using AgentPay.Application.Commands;
using AgentPay.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AgentPay.Web.Controllers;

public class AgentController : Controller
{
    private readonly IMediator _mediator;

    public AgentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var agents = await _mediator.Send(new GetActiveAgentsQuery());
        return View(agents);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAgentCommand command)
    {
        var agentId = await _mediator.Send(command);
        return RedirectToAction(nameof(Details), new { id = agentId });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var agent = await _mediator.Send(new GetAgentByIdQuery(id));
        if (agent == null)
            return NotFound();

        var transactions = await _mediator.Send(new GetTransactionsByAgentQuery(id));
        var statistics = await _mediator.Send(new GetAgentStatisticsQuery(id));

        ViewBag.Transactions = transactions;
        ViewBag.Statistics = statistics;

        return View(agent);
    }

    public async Task<IActionResult> List()
    {
        var agents = await _mediator.Send(new GetActiveAgentsQuery());
        return View(agents);
    }

    [HttpPost]
    public async Task<IActionResult> Activate(Guid id)
    {
        await _mediator.Send(new ActivateAgentCommand(id));
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _mediator.Send(new DeactivateAgentCommand(id));
        return RedirectToAction(nameof(Details), new { id });
    }
}
