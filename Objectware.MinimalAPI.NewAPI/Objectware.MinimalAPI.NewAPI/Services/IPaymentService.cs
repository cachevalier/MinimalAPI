using Objectware.MinimalAPI.NewAPI.Models;

namespace Objectware.MinimalAPI.NewAPI.Services;

public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(PaymentInfo info, CancellationToken ct = default);
}
