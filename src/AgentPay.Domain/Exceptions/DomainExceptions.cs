using System;

namespace AgentPay.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(string message) : base(message) { }
}

public class InvalidAgentConfigException : DomainException
{
    public InvalidAgentConfigException(string message) : base(message) { }
}

public class PaymentFailedException : DomainException
{
    public PaymentFailedException(string message) : base(message) { }
}

public class GuardrailsViolationException : DomainException
{
    public GuardrailsViolationException(string message) : base(message) { }
}
