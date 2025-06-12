using SD.OrderProcessing.Payment.Domain.Exceptions.Infrastructure.Dal;

namespace SD.OrderProcessing.Payment.Domain.Exceptions.Domain.BalanceAccount;

public class BalanceAccountAlreadyExistsException: DomainException
{
    public long UserId { get; }
    
    public BalanceAccountAlreadyExistsException(string? message, long userId, EntityAlreadyExistsException? innerException) : base(message, innerException)
    {
        UserId = userId;
    }
}