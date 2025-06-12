using SD.OrderProcessing.Orders.Domain.Models.Enums;

namespace SD.OrderProcessing.Orders.Domain.Contracts.ISC.MessageQ.Messages;

public record PaymentStatusMessage(
    long OrderId,
    OrderStatus Status
);