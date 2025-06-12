namespace SD.OrderProcessing.Payment.Domain.Exceptions.Infrastructure.Dal;

public class EntityAlreadyExistsException: InfrastructureException
{
    public EntityAlreadyExistsException(string? message) : base(message)
    {
    }

    public EntityAlreadyExistsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}