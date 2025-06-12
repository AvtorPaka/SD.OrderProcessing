namespace SD.OrderProcessing.Orders.Api.Contracts.Requests.Orders;

public record CreateOrderRequest(
    long UserId,
    decimal Amount,
    string? Description
);