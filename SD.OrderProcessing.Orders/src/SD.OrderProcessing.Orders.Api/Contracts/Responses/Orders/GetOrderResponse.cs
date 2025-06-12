using SD.OrderProcessing.Orders.Domain.Models.Enums;

namespace SD.OrderProcessing.Orders.Api.Contracts.Responses.Orders;

public record GetOrderResponse(
    long Id,
    long UserId,
    decimal Amount,
    string Description,
    OrderStatus Status
);