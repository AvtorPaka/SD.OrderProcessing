namespace SD.OrderProcessing.Orders.Domain.Contracts.ISC.MessageQ.Messages;

public record OrderPaymentMessage(
    long OrderId,
    long UserId,
    decimal Amount
);