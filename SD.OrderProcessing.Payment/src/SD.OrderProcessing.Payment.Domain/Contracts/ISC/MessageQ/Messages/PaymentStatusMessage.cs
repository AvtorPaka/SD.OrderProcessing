using SD.OrderProcessing.Payment.Domain.Models.Enums;

namespace SD.OrderProcessing.Payment.Domain.Contracts.ISC.MessageQ.Messages;

public record PaymentStatusMessage(
    long OrderId,
    OrderStatus Status
);