using Objectware.MinimalAPI.OldAPI.Models;

namespace Objectware.MinimalAPI.OldAPI.Services;

public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(PaymentInfo info, CancellationToken ct = default);
}
