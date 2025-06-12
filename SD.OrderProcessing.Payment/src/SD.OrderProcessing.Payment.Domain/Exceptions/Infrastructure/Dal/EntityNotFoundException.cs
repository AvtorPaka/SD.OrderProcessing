namespace SD.OrderProcessing.Payment.Domain.Exceptions.Infrastructure.Dal;

public class EntityNotFoundException: InfrastructureException
{
    public EntityNotFoundException(string? message) : base(message)
    {
    }

    public EntityNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}