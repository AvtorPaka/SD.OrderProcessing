using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SD.OrderProcessing.Orders.Api.Extensions;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Orders.Domain.Contracts.ISC.MessageQ.Messages;
using SD.OrderProcessing.Orders.Infrastructure.Configuration.Options;

namespace SD.OrderProcessing.Orders.Api.BackgroundServices;

public class PaymentStatusMessageConsumer : BackgroundService
{
    private const string PaymentStatusQueueName = "pay_status_mq";
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqConnectionOptions _mqConnectionOptions;
    private readonly ILogger<PaymentStatusMessageConsumer> _logger;

    public PaymentStatusMessageConsumer(
        IServiceProvider serviceProvider,
        IOptions<RabbitMqConnectionOptions> connectionOptions,
        ILogger<PaymentStatusMessageConsumer> logger
    )
    {
        _serviceProvider = serviceProvider;
        _mqConnectionOptions = connectionOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => 
            _logger.LogInformation("[{CurTime}] Payment status messages consumer is stopping.", DateTime.UtcNow)
        );

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using IConnection rmqConnection = await InitializeConnection(cancellationToken);
                _logger.LogConsumerMqConnectionInitialized(
                    curTime: DateTime.UtcNow
                );
                await using IChannel channel = await OpenChannelWithQueueAsync(rmqConnection, cancellationToken);

                var paymentStatusConsumer = new AsyncEventingBasicConsumer(channel);

                paymentStatusConsumer.ReceivedAsync += async (model, args) =>
                {
                    IChannel currentChannel = ((AsyncEventingBasicConsumer)model).Channel;
                    try
                    {
                        PaymentStatusMessage statusMessage = DeserializeMessage(args.Body);

                        await UpdateOrderStatusForMessage(statusMessage, cancellationToken);

                        if (currentChannel.IsOpen)
                        {
                            await currentChannel.BasicAckAsync(
                                deliveryTag: args.DeliveryTag,
                                multiple: false
                            );
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogPaymentStatusConsumerUnexpectedException(
                            curTime: DateTime.UtcNow,
                            exception: ex
                        );

                        bool ntRequeue = ex is not InvalidOperationException;

                        if (currentChannel.IsOpen)
                        {
                            await currentChannel.BasicNackAsync(
                                deliveryTag: args.DeliveryTag,
                                multiple: false,
                                requeue: ntRequeue
                            );
                            
                        }
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: PaymentStatusQueueName,
                    autoAck: false,
                    consumer: paymentStatusConsumer,
                    cancellationToken: cancellationToken
                );
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogPaymentStatusConsumerEnd(
                    curTime: DateTime.UtcNow
                );
            }
            catch (Exception ex)
            {
                _logger.LogPaymentStatusConsumerUnexpectedException(
                    curTime: DateTime.UtcNow,
                    exception: ex
                );

                await Task.Delay(TimeSpan.FromSeconds(10), CancellationToken.None);
            }
        }
    }

    private async Task UpdateOrderStatusForMessage(PaymentStatusMessage paymentStatusMessage,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        IOrdersRepository ordersRepository = scope.ServiceProvider.GetRequiredService<IOrdersRepository>();

        using var transaction = ordersRepository.CreateTransactionScope();

        await ordersRepository.UpdateStatus(
            orderId: paymentStatusMessage.OrderId,
            newStatus: paymentStatusMessage.Status,
            cancellationToken: cancellationToken
        );

        transaction.Complete();
    }

    private PaymentStatusMessage DeserializeMessage(ReadOnlyMemory<byte> body)
    {
        var json = Encoding.UTF8.GetString(body.Span);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<PaymentStatusMessage>(json, options)
               ?? throw new InvalidOperationException("Deserialization failed. Invalid message format");
    }


    private async Task<IConnection> InitializeConnection(CancellationToken cancellationToken)
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = _mqConnectionOptions.HostName,
            UserName = _mqConnectionOptions.UserName,
            Password = _mqConnectionOptions.Password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        return await connectionFactory.CreateConnectionAsync(cancellationToken);
    }

    private async Task<IChannel> OpenChannelWithQueueAsync(IConnection rmqConnection,
        CancellationToken cancellationToken)
    {
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true,
            outstandingPublisherConfirmationsRateLimiter: new ThrottlingRateLimiter(256)
        );

        IChannel rmqChannel = await rmqConnection.CreateChannelAsync(channelOptions, cancellationToken);

        await rmqChannel.QueueDeclareAsync(
            queue: PaymentStatusQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken
        );
        
        await rmqChannel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken: cancellationToken
        );

        return rmqChannel;
    }
}