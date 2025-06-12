namespace SD.OrderProcessing.Orders.Domain.Exceptions.Domain.Orders;

public class OrderInvalidAmountException: DomainException
{
    public long UserId { get; }
    public decimal InvalidOrderAmount { get; }
    
    public OrderInvalidAmountException(string? message, long userId, decimal invalidAmount) : base(message)
    {
        UserId = userId;
        InvalidOrderAmount = invalidAmount;
    }
}