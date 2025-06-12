namespace SD.OrderProcessing.Payment.Domain.Models;

public record BalanceAccountModel(
    long Id,
    long UserId,
    decimal Balance,
    long Version
);