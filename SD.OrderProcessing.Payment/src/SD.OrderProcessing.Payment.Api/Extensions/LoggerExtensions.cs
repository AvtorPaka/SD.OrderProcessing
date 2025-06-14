namespace SD.OrderProcessing.Payment.Api.Extensions;

public static partial class LoggerExtensions
{
    #region Info

    [LoggerMessage(
        LogLevel.Information,
        EventId = 2000,
        Message = "[{CallId}] [{CurTime}] Start executing call. Endpoint: {EndpointRoute}"
    )]
    public static partial void LogRequestStart(this ILogger logger,
        DateTime curTime,
        string callId,
        string endpointRoute);


    [LoggerMessage(
        LogLevel.Information,
        EventId = 2001,
        Message = "[{CallId}] [{CurTime}] Ended executing call. Endpoint: {EndpointRoute}"
    )]
    public static partial void LogRequestEnd(this ILogger logger,
        DateTime curTime,
        string callId,
        string endpointRoute);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2002,
        Message = "[{CurTime}] Withdraw payment operations processor start executing."
    )]
    public static partial void LogWithdrawOperationsProcessorStart(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2003,
        Message = "[{CurTime}] Withdraw payment operations processor stopped executing."
    )]
    public static partial void LogWithdrawOperationsProcessorStop(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2004,
        Message = "[{CurTime}] Withdraw payment operations processor start processing payments."
    )]
    public static partial void LogWithdrawOperationsProcessorStartProcessing(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2005,
        Message = "[{CurTime}] Withdraw payment operations processor ended processing payments."
    )]
    public static partial void LogWithdrawOperationsProcessorStopProcessing(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2006,
        Message = "[{CurTime}] Order Payment Status messages producer start executing."
    )]
    public static partial void LogPaymentStatusMessagesProducerStart(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2007,
        Message = "[{CurTime}] Order Payment Status messages producer stopped executing."
    )]
    public static partial void LogPaymentStatusMessagesProducerEnd(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2008,
        Message = "[{CurTime}] Order Payment Status messages producer RabbitMQ connection initialized."
    )]
    public static partial void LogProducerMqConnectionInitialized(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2009,
        Message = "[{CurTime}] Order Payment Status messages producer start processing {MessagesAmount} messages."
    )]
    public static partial void LogPaymentStatusMessagesProducerStartProcessing(this ILogger logger,
        DateTime curTime,
        int messagesAmount);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2010,
        Message = "[{CurTime}] Order Payment Status messages producer ended processing {MessagesAmount} messages."
    )]
    public static partial void LogPaymentStatusMessagesProducerEndProcessing(this ILogger logger,
        DateTime curTime,
        int messagesAmount);

    [LoggerMessage(
        LogLevel.Information,
        EventId = 20011,
        Message = "[{CurTime}] Order Payment messages consumer RabbitMQ connection initialized."
    )]
    public static partial void LogConsumerMqConnectionInitialized(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2012,
        Message = "[{CurTime}] Order payment messages consumer stopped executing."
    )]
    public static partial void LogOrderPaymentConsumerEnd(this ILogger logger,
        DateTime curTime);
    
    #endregion

    #region Error

    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4000,
        Message = "[{CallId}] [{CurTime}] Unexpected exception occured during request processing."
    )]
    public static partial void LogInternalError(this ILogger logger,
        Exception exception,
        string callId,
        DateTime curTime);
    
    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4004,
        Message = "[{CurTime}] Unexpected exception occured during  payment operations processing."
    )]
    public static partial void LogWithdrawOperationsProcessorUnexpectedError(this ILogger logger,
        Exception exception,
        DateTime curTime);
    
    
    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4005,
        Message = "[{CurTime}] Unexpected exception occured during payment status messages processing."
    )]
    public static partial void LogPaymentStatusMessagesProducesUnexpectedException(this ILogger logger,
        Exception exception,
        DateTime curTime);
    
    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4006,
        Message = "[{CurTime}] Unexpected exception occured during order payment status message publish with id: {messageId}"
    )]
    public static partial void LogPaymentStatusMessagesProducerInvalidPublish(this ILogger logger,
        Exception exception,
        DateTime curTime,
        long messageId);
    
    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4007,
        Message = "[{CurTime}] Unexpected exception occured during order payment messages processing."
    )]
    public static partial void LogOrderPaymentConsumerUnexpectedException(this ILogger logger,
        Exception exception,
        DateTime curTime);
    
    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4008,
        Message = "[{CurTime}] Payment operation for order has been already consumed."
    )]
    public static partial void LogOrderPaymentConsumerPaymentAlreadyPersist(this ILogger logger,
        DateTime curTime);

    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4001,
        Message =
            "[{CallId}] [{CurTime}] Balance account for user with id: {UserId} couldn't be found."
    )]
    public static partial void LogBalanceAccountNotFoundForUser(this ILogger logger,
        string callId,
        DateTime curTime,
        long userId
    );

    [LoggerMessage(
        LogLevel.Error,
        EventId = 4002,
        Message = "[{CallId}] [{CurTime}] Invalid deposit amount: {InvalidAmount} for account with user id: {UserId}"
    )]
    public static partial void LogInvalidDepositAmountBadRequest(this ILogger logger,
        string callId,
        DateTime curTime,
        decimal invalidAmount,
        long userId
    );
    
    [LoggerMessage(
        LogLevel.Error,
        EventId = 4003,
        Message = "[{CallId}] [{CurTime}] Balance account already exists for user with id: {UserId}"
    )]
    public static partial void LogBalanceAccountAlreadyExists(this ILogger logger,
        string callId,
        DateTime curTime,
        long userId
    );

    #endregion

    #region Debug

    [LoggerMessage(
        LogLevel.Debug,
        EventId = 1000,
        Message = "[{CallId}] [{CurTime}] Request headers:\n{Headers}"
    )]
    public static partial void LogRequestHeaders(this ILogger logger,
        DateTime curTime,
        string callId,
        string headers);

    #endregion
}