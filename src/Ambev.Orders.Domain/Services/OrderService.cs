using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Exceptions;
using Orders.Domain.Interfaces.Repositories;
using Orders.Domain.Interfaces.Services;
using Orders.Domain.Interfaces.Strategies;

namespace Orders.Domain.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ITaxCalculator _calculator;

    public OrderService(IOrderRepository repository, ITaxCalculator calculator)
    {
        _repository = repository;
        _calculator = calculator;
    }

    public async Task<Order> CreateAsync(
        int orderId,
        int customerId,
        IEnumerable<OrderItem> items,
        CancellationToken ct = default)
    {
        if (await _repository.ExistsAsync(orderId, ct))
            throw new DomainException($"An order with id '{orderId}' already exists.");

        var order = new Order(orderId, customerId, items);

        var tax = _calculator.Calculate(order);
        order.ApplyTax(tax);

        await _repository.AddAsync(order, ct);

        return order;
    }

    public async Task<Order> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);

        if (order is null)
            throw new DomainException($"Order '{id}' not found.");

        return order;
    }

    public async Task<IReadOnlyCollection<Order>> ListByStatusAsync(
        OrderStatus status,
        CancellationToken ct = default)
        => await _repository.ListByStatusAsync(status, ct);

    public async Task<Order> StartProcessingAsync(int id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);

        if (order is null)
            throw new DomainException($"Order '{id}' not found.");

        order.StartProcessing();

        await _repository.UpdateAsync(order, ct);

        return order;
    }

    public async Task<Order> SendAsync(int id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);

        if (order is null)
            throw new DomainException($"Order '{id}' not found.");

        order.Send();

        await _repository.UpdateAsync(order, ct);

        return order;
    }
}
