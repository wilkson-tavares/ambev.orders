using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Exceptions;

namespace Orders.Domain.Entities;

public class Order
{
    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int ClientId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Tax { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalItemsValue => _items.Sum(i => i.TotalValue);

    private Order() { }

    public Order(int orderId, int clientId, IEnumerable<OrderItem> items)
    {
        if (orderId <= 0)
            throw new DomainException("Invalid OrderId.");
        if (clientId <= 0)
            throw new DomainException("Invalid ClientId.");
        if (items is null || !items.Any())
            throw new DomainException("The order must contain at least one item.");

        OrderId = orderId;
        ClientId = clientId;
        Status = OrderStatus.Created;

        _items.AddRange(items);
    }

    public void ApplyTax(decimal taxValue)
    {
        if (taxValue < 0)
            throw new DomainException("Tax cannot be negative.");

        Tax = taxValue;
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.Created)
            throw new DomainException($"Order cannot be processed in status '{Status}'.");

        Status = OrderStatus.Processing;
    }

    public void Send()
    {
        if (Status != OrderStatus.Processing)
            throw new DomainException($"Order cannot be sent in status '{Status}'.");

        Status = OrderStatus.Sent;
    }
}