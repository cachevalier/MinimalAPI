using Objectware.MinimalAPI.NewAPI.Models;

namespace Objectware.MinimalAPI.NewAPI.Services;

public interface IOrderService
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PagedResult<OrderDto>> GetAllAsync(
        int page,
        int pageSize,
        string? status,
        CancellationToken ct = default);
}
