using SD.OrderProcessing.Payment.Domain.Exceptions.Infrastructure.Dal;

namespace SD.OrderProcessing.Payment.Domain.Exceptions.Domain.BalanceAccount;

public class BalanceAccountNotFoundForUserException: DomainException
{
    public long UserId { get; }
    
    public BalanceAccountNotFoundForUserException(string? message, long userId, EntityNotFoundException innerException) : base(message, innerException)
    {
        UserId = userId;
    }
}