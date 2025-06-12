using SD.OrderProcessing.Orders.Domain.Models.Enums;

namespace SD.OrderProcessing.Orders.Api.Contracts.Responses.Orders;

public record CreateOrderResponse(
    long OrderId,
    OrderStatus Status
);