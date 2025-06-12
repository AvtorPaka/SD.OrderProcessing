namespace SD.OrderProcessing.Payment.Domain.Exceptions.Domain.BalanceAccount;

public class BalanceInsufficientFundsException: DomainException
{
    public BalanceInsufficientFundsException(string? message) : base(message)
    {
    }
}