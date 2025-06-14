namespace SD.OrderProcessing.Payment.Domain.Contracts.ISC.MessageQ.Messages;

public record OrderPaymentMessage(
    long OrderId,
    long UserId,
    decimal Amount
);