namespace SD.OrderProcessing.Orders.Domain.Contracts.ISC.MessageQ.Messages;

public record OrderPaymentMessage(
    long OrderId,
    long UserOd,
    decimal Amount
);