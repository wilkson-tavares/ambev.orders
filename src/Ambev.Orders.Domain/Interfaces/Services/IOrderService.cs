using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Domain.Interfaces.Services;

public interface IOrderService
{
    Task<Order> CreateAsync(
        int orderId,
        int clientId,
        IEnumerable<OrderItem> items,
        CancellationToken ct = default);

    Task<Order> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyCollection<Order>> ListByStatusAsync(
        OrderStatus status,
        CancellationToken ct = default);

    Task<Order> StartProcessingAsync(int id, CancellationToken ct = default);

    Task<Order> SendAsync(int id, CancellationToken ct = default);
}
