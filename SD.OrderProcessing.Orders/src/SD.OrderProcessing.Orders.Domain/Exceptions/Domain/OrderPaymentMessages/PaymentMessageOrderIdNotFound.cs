using SD.OrderProcessing.Orders.Domain.Exceptions.Infrastructure.Dal;

namespace SD.OrderProcessing.Orders.Domain.Exceptions.Domain.OrderPaymentMessages;

public class PaymentMessageOrderIdNotFound: DomainException
{
    public long UserId { get; }
    public long OrderId { get;  }
    
    public PaymentMessageOrderIdNotFound(string? message, long orderId, long userId, EntityNotFoundException? innerException) : base(message, innerException)
    {
        OrderId = orderId;
        UserId = userId;
    }
}