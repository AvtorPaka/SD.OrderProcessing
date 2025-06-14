namespace SD.OrderProcessing.Payment.Domain.Models.Enums;

public enum OrderStatus
{
    Failed,
    Balance_Not_Exists,
    Insufficient_Funds,
    Pending,
    Finished
}