using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Objectware.MinimalAPI.OldAPI.Controllers;
using Objectware.MinimalAPI.OldAPI.Models;
using Objectware.MinimalAPI.OldAPI.Services;
using Xunit;

namespace Objectware.MinimalAPI.OldAPI.Tests;

public class OrdersControllerTests      
{
    [Fact]
    public async Task ConfirmOrder_ReturnsNotFound_WhenOrderMissing()
    {
        var orderService = Substitute.For<IOrderService>();
        var paymentService = Substitute.For<IPaymentService>();
        var validator = Substitute.For<IValidator<ConfirmOrderRequest>>();
        var logger = Substitute.For<ILogger<OrdersController>>();

        orderService
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        validator
            .ValidateAsync(Arg.Any<ConfirmOrderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        // Le controller a besoin d'un HttpContext pour ModelState
        var controller = new OrdersController(
            orderService, paymentService, logger, validator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.ConfirmOrder(
            Guid.NewGuid(),
            new ConfirmOrderRequest(new PaymentInfo("tok_test", 99.99m)),
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task ConfirmOrder_ReturnsOk_WhenPaymentSucceeds()
    {
        var orderId = Guid.NewGuid();
        var orderService = Substitute.For<IOrderService>();
        var paymentService = Substitute.For<IPaymentService>();
        var validator = Substitute.For<IValidator<ConfirmOrderRequest>>();
        var logger = Substitute.For<ILogger<OrdersController>>();

        orderService
            .GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(new Order { Id = orderId });

        paymentService
            .ProcessAsync(Arg.Any<PaymentInfo>(), Arg.Any<CancellationToken>())
            .Returns(new PaymentResult(true, "txn_abc123", null));

        validator
            .ValidateAsync(Arg.Any<ConfirmOrderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var controller = new OrdersController(
            orderService, paymentService, logger, validator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.ConfirmOrder(
            orderId,
            new ConfirmOrderRequest(new PaymentInfo("tok_test", 99.99m)),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<OrderConfirmationDto>(ok.Value);
        Assert.Equal("txn_abc123", dto.TransactionId);
    }

    [Fact]
    public async Task ConfirmOrder_ReturnsUnprocessableEntity_WhenPaymentFails()
    {
        var orderId = Guid.NewGuid();
        var orderService = Substitute.For<IOrderService>();
        var paymentService = Substitute.For<IPaymentService>();
        var validator = Substitute.For<IValidator<ConfirmOrderRequest>>();
        var logger = Substitute.For<ILogger<OrdersController>>();

        orderService
            .GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(new Order { Id = orderId });

        paymentService
            .ProcessAsync(Arg.Any<PaymentInfo>(), Arg.Any<CancellationToken>())
            .Returns(new PaymentResult(false, null, "Insufficient funds"));

        validator
            .ValidateAsync(Arg.Any<ConfirmOrderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var controller = new OrdersController(
            orderService, paymentService, logger, validator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.ConfirmOrder(
            orderId,
            new ConfirmOrderRequest(new PaymentInfo("tok_test", 99.99m)),
            CancellationToken.None);

        Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetOrders_ReturnsOk()
    {
        var orderService = Substitute.For<IOrderService>();
        var paymentService = Substitute.For<IPaymentService>();
        var validator = Substitute.For<IValidator<ConfirmOrderRequest>>();
        var logger = Substitute.For<ILogger<OrdersController>>();

        orderService
            .GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<OrderDto>([], 0, 1, 10));

        var controller = new OrdersController(
            orderService, paymentService, logger, validator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.GetOrders(ct: CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }
}
