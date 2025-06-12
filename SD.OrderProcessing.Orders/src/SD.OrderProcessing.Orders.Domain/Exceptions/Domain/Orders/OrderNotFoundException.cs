using SD.OrderProcessing.Orders.Domain.Exceptions.Infrastructure.Dal;

namespace SD.OrderProcessing.Orders.Domain.Exceptions.Domain.Orders;

public class OrderNotFoundException: DomainException
{
    public long OrderId { get;  }
    
    public OrderNotFoundException(string? message, long orderId, EntityNotFoundException? innerException) : base(message, innerException)
    {
        OrderId = orderId;
    }
}