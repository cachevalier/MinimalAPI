using Objectware.MinimalAPI.OldAPI.Models;

namespace Objectware.MinimalAPI.OldAPI.Services;

public class PaymentService : IPaymentService
{
    public Task<PaymentResult> ProcessAsync(PaymentInfo info, CancellationToken ct = default)
    {
        // Simule un échec si le montant est négatif ou nul
        if (info.Amount <= 0)
            return Task.FromResult(
                new PaymentResult(false, null, "Amount must be greater than zero"));

        var transactionId = $"txn_{Guid.NewGuid():N}";
        return Task.FromResult(new PaymentResult(true, transactionId, null));
    }
}
