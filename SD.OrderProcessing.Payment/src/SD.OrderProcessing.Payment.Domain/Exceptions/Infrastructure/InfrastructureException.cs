namespace SD.OrderProcessing.Payment.Domain.Exceptions.Infrastructure;

public class InfrastructureException: Exception
{
    protected InfrastructureException(string? message) : base(message)
    {
    }

    protected InfrastructureException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}