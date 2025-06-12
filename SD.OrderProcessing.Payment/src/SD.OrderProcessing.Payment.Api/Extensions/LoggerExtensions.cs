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