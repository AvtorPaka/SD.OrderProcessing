using SD.OrderProcessing.Orders.Domain.Models.Enums;

namespace SD.OrderProcessing.Orders.Domain.Contracts.Dal.Entities;

public class OrderEntity
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
}