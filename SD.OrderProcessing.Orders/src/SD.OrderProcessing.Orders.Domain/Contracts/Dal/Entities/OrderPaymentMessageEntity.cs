using SD.OrderProcessing.Orders.Domain.Models.Enums;

namespace SD.OrderProcessing.Orders.Domain.Contracts.Dal.Entities;

public class OrderPaymentMessageEntity
{
    public long Id { get; init; }
    public long OrderId { get; init; }
    public long UserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public decimal Amount { get; init; }
    public MessageState State { get; init; }
}