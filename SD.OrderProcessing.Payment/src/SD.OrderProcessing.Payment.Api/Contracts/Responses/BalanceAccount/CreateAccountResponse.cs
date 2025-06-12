namespace SD.OrderProcessing.Payment.Api.Contracts.Responses.BalanceAccount;

public record CreateAccountResponse(
    long AccountId,
    decimal Balance
);