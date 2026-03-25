using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Domain.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Order>> ListByStatusAsync(OrderStatus status, CancellationToken ct = default);
    Task<bool> ExistsAsync(int orderId, CancellationToken ct = default);
}