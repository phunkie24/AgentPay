using AgentPay.Application.Commands;
using AgentPay.Application.Queries;
using AgentPay.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AgentPay.Web.Controllers;

public class TransactionController : Controller
{
    private readonly IMediator _mediator;

    public TransactionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var transactions = await _mediator.Send(new GetPendingTransactionsQuery());
        return View(transactions);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var transaction = await _mediator.Send(new GetTransactionByIdQuery(id));
        if (transaction == null)
            return NotFound();
        return View(transaction);
    }

    public async Task<IActionResult> ByAgent(Guid agentId)
    {
        var transactions = await _mediator.Send(new GetTransactionsByAgentQuery(agentId));
        return View("Index", transactions);
    }

    [HttpPost]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request)
    {
        // This would trigger the payment orchestration service
        return Ok(new { message = "Payment initiated" });
    }

    [HttpPost]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteTransactionCommand command)
    {
        await _mediator.Send(command);
        return RedirectToAction(nameof(Details), new { id });
    }
}
