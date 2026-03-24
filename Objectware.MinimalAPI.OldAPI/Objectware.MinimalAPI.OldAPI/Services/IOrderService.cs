using Objectware.MinimalAPI.OldAPI.Models;

namespace Objectware.MinimalAPI.OldAPI.Services;

public interface IOrderService
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PagedResult<OrderDto>> GetAllAsync(
        int page,
        int pageSize,
        string? status,
        CancellationToken ct = default);
}
