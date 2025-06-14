namespace SD.OrderProcessing.Payment.Infrastructure.Configuration.Options;

public class RabbitMqConnectionOptions
{
    public string HostName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}