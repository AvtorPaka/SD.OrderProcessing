using System.Net;

namespace SD.OrderProcessing.Payment.Api.Contracts.Responses;

public record ErrorResponse(
    HttpStatusCode StatusCode,
    string? Message
);