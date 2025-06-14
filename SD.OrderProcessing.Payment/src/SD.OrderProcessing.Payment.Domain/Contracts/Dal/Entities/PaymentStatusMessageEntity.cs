using SD.OrderProcessing.Payment.Domain.Models.Enums;

namespace SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;

public class PaymentStatusMessageEntity
{
    public long Id { get; init; }
    public long OrderId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public OrderStatus OrderStatus { get; init; }
    public MessageState State { get; init; }
}