namespace SD.OrderProcessing.Orders.Domain.Models.Enums;

public enum OrderStatus
{
    Failed,
    BalanceNotExists,
    InsufficientFunds,
    Pending,
    Finished
}