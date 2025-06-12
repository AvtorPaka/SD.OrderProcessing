namespace SD.OrderProcessing.Payment.Domain.Exceptions.Domain.BalanceAccount;

public class BalanceAccountDepositBadRequestException: DomainException
{
    public long UserId { get; }
    public decimal InvalidSum { get; }
    
    public BalanceAccountDepositBadRequestException(string? message, long userId, decimal invalidSum) : base(message)
    {
        UserId = userId;
        InvalidSum = invalidSum;
    }
}