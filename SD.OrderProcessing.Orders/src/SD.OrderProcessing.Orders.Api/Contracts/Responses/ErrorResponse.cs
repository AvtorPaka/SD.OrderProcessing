using System.Net;

namespace SD.OrderProcessing.Orders.Api.Contracts.Responses;

public record ErrorResponse(
    HttpStatusCode StatusCode,
    string? Message
);