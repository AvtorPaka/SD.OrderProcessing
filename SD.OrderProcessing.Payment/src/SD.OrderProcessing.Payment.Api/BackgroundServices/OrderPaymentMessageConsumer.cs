using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SD.OrderProcessing.Payment.Api.Extensions;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Payment.Domain.Contracts.ISC.MessageQ.Messages;
using SD.OrderProcessing.Payment.Domain.Exceptions.Infrastructure.Dal;
using SD.OrderProcessing.Payment.Infrastructure.Configuration.Options;

namespace SD.OrderProcessing.Payment.Api.BackgroundServices;

public class OrderPaymentMessageConsumer : BackgroundService
{
    private const string OrderPaymentQueueName = "ord_pay_mq";
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqConnectionOptions _mqConnectionOptions;
    private readonly ILogger<OrderPaymentMessageConsumer> _logger;

    public OrderPaymentMessageConsumer(
        IServiceProvider serviceProvider,
        IOptions<RabbitMqConnectionOptions> connectionOptions,
        ILogger<OrderPaymentMessageConsumer> logger)
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

                var orderPaymentConsumer = new AsyncEventingBasicConsumer(channel);

                orderPaymentConsumer.ReceivedAsync += async (model, args) =>
                {
                    IChannel currentChannel = ((AsyncEventingBasicConsumer)model).Channel;
                    try
                    {
                        OrderPaymentMessage orderMessage = DeserializeMessage(args.Body);

                        await SaveOrderMessageToTheInbox(orderMessage, cancellationToken);

                        if (currentChannel.IsOpen)
                        {
                            await currentChannel.BasicAckAsync(
                                deliveryTag: args.DeliveryTag,
                                multiple: false
                            );

                        }
                    }
                    catch (EntityAlreadyExistsException)
                    {
                        _logger.LogOrderPaymentConsumerPaymentAlreadyPersist(
                            curTime: DateTime.UtcNow
                            );
                        
                        if (currentChannel.IsOpen)
                        {
                            await currentChannel.BasicNackAsync(
                                deliveryTag: args.DeliveryTag,
                                multiple: false,
                                requeue: false
                            );
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogOrderPaymentConsumerUnexpectedException(
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
                    queue: OrderPaymentQueueName,
                    autoAck: false,
                    consumer: orderPaymentConsumer,
                    cancellationToken: cancellationToken
                );
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogOrderPaymentConsumerEnd(
                    curTime: DateTime.UtcNow
                );
            }
            catch (Exception ex)
            {
                _logger.LogOrderPaymentConsumerUnexpectedException(
                    curTime: DateTime.UtcNow,
                    exception: ex
                );

                await Task.Delay(TimeSpan.FromSeconds(10), CancellationToken.None);
            }
        }
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
    
    private async Task SaveOrderMessageToTheInbox(OrderPaymentMessage paymentMessage,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        IBalanceWithdrawUpdatesRepository paymentOperationsRepository =
            scope.ServiceProvider.GetRequiredService<IBalanceWithdrawUpdatesRepository>();

        using var transaction = paymentOperationsRepository.CreateTransactionScope();

        await paymentOperationsRepository.Create(
            entities: [
                new BalanceWithdrawUpdateEntity
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    Amount = paymentMessage.Amount,
                    OrderId = paymentMessage.OrderId,
                    UserId = paymentMessage.UserId
                }
            ],
            cancellationToken: cancellationToken
        );

        transaction.Complete();
    }

    private OrderPaymentMessage DeserializeMessage(ReadOnlyMemory<byte> body)
    {
        var json = Encoding.UTF8.GetString(body.Span);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<OrderPaymentMessage>(json, options)
               ?? throw new InvalidOperationException("Deserialization failed. Invalid message format");
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
            queue: OrderPaymentQueueName,
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