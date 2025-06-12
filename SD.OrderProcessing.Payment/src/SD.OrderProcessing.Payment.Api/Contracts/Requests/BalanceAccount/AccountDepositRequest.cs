namespace SD.OrderProcessing.Payment.Api.Contracts.Requests.BalanceAccount;

public record AccountDepositRequest(
    long UserId,
    decimal Amount
);