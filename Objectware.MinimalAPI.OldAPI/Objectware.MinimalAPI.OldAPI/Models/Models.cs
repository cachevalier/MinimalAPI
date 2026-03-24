namespace Objectware.MinimalAPI.OldAPI.Models;

public record ConfirmOrderRequest(PaymentInfo PaymentInfo);
public record PaymentInfo(string CardToken, decimal Amount);
public record OrderConfirmationDto(Guid OrderId, string TransactionId);
public record PaymentResult(bool Success, string? TransactionId, string? Error);
public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
public record OrderDto(Guid Id, string Status, decimal Amount);

public class Order
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal Amount { get; set; }
}