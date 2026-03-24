using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Objectware.MinimalAPI.OldAPI.Models;
using Objectware.MinimalAPI.OldAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace Objectware.MinimalAPI.OldAPI.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(
    IOrderService orderService,
    IPaymentService paymentService,
    ILogger<OrdersController> logger,
    IValidator<ConfirmOrderRequest> validator) : ControllerBase
{
    [HttpGet]
    [Tags("Orders")]
    [EndpointSummary("Get paginated orders")]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await orderService.GetAllAsync(page, pageSize, status, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/confirm")]
    [Tags("Orders")]
    [EndpointSummary("Confirm an order")]
    [ProducesResponseType(typeof(OrderConfirmationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderConfirmationDto>> ConfirmOrder(
        Guid id,
        [FromBody] ConfirmOrderRequest request,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        var order = await orderService.GetByIdAsync(id, ct);
        if (order is null)
            return NotFound();

        var paymentResult = await paymentService.ProcessAsync(request.PaymentInfo, ct);
        if (!paymentResult.Success)
        {
            logger.LogWarning("Payment failed for order {OrderId}", id);
            return UnprocessableEntity(paymentResult.Error);
        }

        logger.LogInformation("Order {OrderId} confirmed", id);
        return Ok(new OrderConfirmationDto(id, paymentResult.TransactionId!));
    }
}