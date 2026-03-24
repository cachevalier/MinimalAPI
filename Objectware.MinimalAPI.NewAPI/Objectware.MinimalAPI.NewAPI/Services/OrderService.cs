using Objectware.MinimalAPI.NewAPI.Models;

namespace Objectware.MinimalAPI.NewAPI.Services;

public class OrderService : IOrderService
{
    private static readonly List<Order> _store =
    [
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Status = "Pending",   Amount = 49.99m  },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Status = "Confirmed", Amount = 149.00m },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Status = "Pending",   Amount = 9.99m   },
        new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Status = "Cancelled", Amount = 79.50m  },
    ];

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.FirstOrDefault(o => o.Id == id));

    public Task<PagedResult<OrderDto>> GetAllAsync(
        int page,
        int pageSize,
        string? status,
        CancellationToken ct = default)
    {
        var query = _store.AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        var total = query.Count();

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto(o.Id, o.Status, o.Amount));

        return Task.FromResult(new PagedResult<OrderDto>(items, total, page, pageSize));
    }
}
