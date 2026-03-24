
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Objectware.MinimalAPI.NewAPI.Endpoints;
using Objectware.MinimalAPI.NewAPI.Models;
using Objectware.MinimalAPI.NewAPI.Services;
using Xunit;


namespace Objectware.MinimalAPI.NewAPI.Tests;
public class OrderEndpointsTests
{
    [Fact]
    public async Task GetOrders_ReturnsOk()
    {
        var orderService = Substitute.For<IOrderService>();
        orderService
            .GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<OrderDto>([], 0, 1, 10));

        var result = await OrderEndpoints.GetOrders(
            orderService,
            CancellationToken.None,
            1, 10, null);

        Assert.IsType<Ok<PagedResult<OrderDto>>>(result);
    }

    [Fact]
    public async Task ConfirmOrder_ReturnsNotFound_WhenOrderMissing()
    {
        var orderService = Substitute.For<IOrderService>();
        var paymentService = Substitute.For<IPaymentService>();
        var logger = Substitute.For<ILogger<Program>>();

        orderService
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var result = await OrderEndpoints.ConfirmOrder(
            Guid.NewGuid(),
            new ConfirmOrderRequest(new PaymentInfo("tok_test", 99.99m)),
            orderService,
            paymentService,
            logger,
            CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task ConfirmOrder_ReturnsOk_WhenPaymentSucceeds()
    {
        var orderId = Guid.NewGuid();
        var orderService = Substitute.For<IOrderService>();
        var paymentService = Substitute.For<IPaymentService>();
        var logger = Substitute.For<ILogger<Program>>();

        orderService
            .GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(new Order { Id = orderId });

        paymentService
            .ProcessAsync(Arg.Any<PaymentInfo>(), Arg.Any<CancellationToken>())
            .Returns(new PaymentResult(true, "txn_abc123", null));

        var result = await OrderEndpoints.ConfirmOrder(
            orderId,
            new ConfirmOrderRequest(new PaymentInfo("tok_test", 99.99m)),
            orderService,
            paymentService,
            logger,
            CancellationToken.None);

        var ok = Assert.IsType<Ok<OrderConfirmationDto>>(result);
        Assert.Equal("txn_abc123", ok.Value!.TransactionId);
    }

    [Fact]
    public async Task ConfirmOrder_ReturnsUnprocessableEntity_WhenPaymentFails()
    {
        var orderId = Guid.NewGuid();
        var orderService = Substitute.For<IOrderService>();
        var paymentService = Substitute.For<IPaymentService>();
        var logger = Substitute.For<ILogger<Program>>();

        orderService
            .GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(new Order { Id = orderId });

        paymentService
            .ProcessAsync(Arg.Any<PaymentInfo>(), Arg.Any<CancellationToken>())
            .Returns(new PaymentResult(false, null, "Insufficient funds"));

        var result = await OrderEndpoints.ConfirmOrder(
            orderId,
            new ConfirmOrderRequest(new PaymentInfo("tok_test", 99.99m)),
            orderService,
            paymentService,
            logger,
            CancellationToken.None);

        Assert.IsType<UnprocessableEntity<string?>>(result);
    }
}
