using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SD.OrderProcessing.Payment.Api.Contracts.Responses;
using SD.OrderProcessing.Payment.Domain.Exceptions.Domain.BalanceAccount;

namespace SD.OrderProcessing.Payment.Api.Filters.Utils;

internal static class ErrorRequestHandler
{
    internal static void HandleUserBalanceAccountNotFoundError(ExceptionContext context,  BalanceAccountNotFoundForUserException exception)
    {
        JsonResult result = new JsonResult(
            new ErrorResponse(
                StatusCode: HttpStatusCode.NotFound,
                Message: $"Balance account for user with id: {exception.UserId} not found."
                )
            )
        {
            ContentType = "application/json",
            StatusCode = (int)HttpStatusCode.NotFound
        };

        context.Result = result;
    }
    
    internal static void HandleAccountDepositBadRequestError(ExceptionContext context, BalanceAccountDepositBadRequestException exception)
    {
        JsonResult result = new JsonResult(
            new ErrorResponse(
                StatusCode: HttpStatusCode.BadRequest,
                Message: $"Invalid depisot sum: {exception.InvalidSum}"
            )
        )
        {
            ContentType = "application/json",
            StatusCode = (int)HttpStatusCode.BadRequest
        };

        context.Result = result;
    }

    internal static void HandleAccountAlreadyExistsForUser(ExceptionContext context,
        BalanceAccountAlreadyExistsException exception)
    {
        JsonResult result = new JsonResult(
            new ErrorResponse(
                StatusCode: HttpStatusCode.Conflict,
                Message: $"Balance account for user with id: {exception.UserId} already exists"
                )
            )
        {
            ContentType = "application/json",
            StatusCode = (int)HttpStatusCode.Conflict
        };

        context.Result = result;
    }

    
    internal static void HandleInternalError(ExceptionContext context)
    {
        JsonResult result = new JsonResult(
            new ErrorResponse(
                StatusCode: HttpStatusCode.InternalServerError,
                Message: "Internal error. Check SD.OP.Payment logs for detailed description"
            )
        )
        {
            ContentType = "application/json",
            StatusCode = (int)HttpStatusCode.InternalServerError
        };

        context.Result = result;
    }
}