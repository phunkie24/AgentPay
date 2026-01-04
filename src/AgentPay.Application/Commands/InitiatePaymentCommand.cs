using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace AgentPay.Application.Commands;

public record InitiatePaymentCommand(
    Guid AgentId,
    Guid ServiceId,
    decimal Amount,
    string Reasoning) : IRequest<Guid>;
