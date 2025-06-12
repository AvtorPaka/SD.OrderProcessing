using SD.OrderProcessing.Orders.Domain.Models.Enums;

namespace SD.OrderProcessing.Orders.Domain.Models;

public record OrderModel(
    long Id,
    long UserId,
    decimal Amount,
    string Description,
    OrderStatus Status
);