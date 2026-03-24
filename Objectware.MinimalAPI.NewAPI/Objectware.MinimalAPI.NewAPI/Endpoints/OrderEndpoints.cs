using Microsoft.AspNetCore.Mvc;
using Objectware.MinimalAPI.NewAPI.Models;
using Objectware.MinimalAPI.NewAPI.Services;

namespace Objectware.MinimalAPI.NewAPI.Endpoints;

public static class OrderEndpoints
{
    internal static async Task<IResult> GetOrders(
        IOrderService orderService,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var result = await orderService.GetAllAsync(page, pageSize, status, ct);
        return TypedResults.Ok(result);
    }

    internal static async Task<IResult> ConfirmOrder(
        Guid id,
        ConfirmOrderRequest request,
        IOrderService orderService,
        IPaymentService paymentService,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var order = await orderService.GetByIdAsync(id, ct);
        if (order is null)
            return TypedResults.NotFound();

        var paymentResult = await paymentService.ProcessAsync(request.PaymentInfo, ct);
        if (!paymentResult.Success)
        {
            logger.LogWarning("Payment failed for order {OrderId}", id);
            return TypedResults.UnprocessableEntity(paymentResult.Error);
        }

        logger.LogInformation("Order {OrderId} confirmed", id);
        return TypedResults.Ok(
            new OrderConfirmationDto(id, paymentResult.TransactionId!));
    }
}
