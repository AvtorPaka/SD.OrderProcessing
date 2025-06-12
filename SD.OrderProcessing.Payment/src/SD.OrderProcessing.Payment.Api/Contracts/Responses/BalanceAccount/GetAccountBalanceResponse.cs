namespace SD.OrderProcessing.Payment.Api.Contracts.Responses.BalanceAccount;

public record GetAccountBalanceResponse(
    long AccountId,
    decimal Balance
);