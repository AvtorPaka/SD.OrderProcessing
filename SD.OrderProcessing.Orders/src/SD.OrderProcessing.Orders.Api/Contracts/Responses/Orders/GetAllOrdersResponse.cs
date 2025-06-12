using SD.OrderProcessing.Orders.Domain.Models.Enums;

namespace SD.OrderProcessing.Orders.Api.Contracts.Responses.Orders;

public record GetAllOrdersResponse(
    long Id,
    decimal Amount,
    string Description,
    OrderStatus Status
);