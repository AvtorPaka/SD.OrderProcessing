using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SD.OrderProcessing.Orders.Api.Contracts.Responses;
using SD.OrderProcessing.Orders.Domain.Exceptions.Domain.Orders;

namespace SD.OrderProcessing.Orders.Api.Filters.Utils;

public class ErrorRequestHandler
{
    internal static void HandleOrderNotFoundError(ExceptionContext context,  OrderNotFoundException exception)
    {
        JsonResult result = new JsonResult(
            new ErrorResponse(
                StatusCode: HttpStatusCode.NotFound,
                Message: $"Order with id: {exception.OrderId} not found."
                )
            )
        {
            ContentType = "application/json",
            StatusCode = (int)HttpStatusCode.NotFound
        };

        context.Result = result;
    }
    
    internal static void HandleOrderAmountBadRequestError(ExceptionContext context, OrderInvalidAmountException exception)
    {
        JsonResult result = new JsonResult(
            new ErrorResponse(
                StatusCode: HttpStatusCode.BadRequest,
                Message: $"Invalid order amount: {exception.InvalidOrderAmount}"
            )
        )
        {
            ContentType = "application/json",
            StatusCode = (int)HttpStatusCode.BadRequest
        };

        context.Result = result;
    }
    
    internal static void HandleInternalError(ExceptionContext context)
    {
        JsonResult result = new JsonResult(
            new ErrorResponse(
                StatusCode: HttpStatusCode.InternalServerError,
                Message: "Internal error. Check SD.OP.Orders logs for detailed description"
            )
        )
        {
            ContentType = "application/json",
            StatusCode = (int)HttpStatusCode.InternalServerError
        };

        context.Result = result;
    }
}