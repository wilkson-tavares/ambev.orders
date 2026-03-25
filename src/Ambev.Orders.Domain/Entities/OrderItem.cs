using Orders.Domain.Exceptions;

namespace Orders.Domain.Entities;

public class OrderItem
{
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal Value { get; private set; }

    private OrderItem() { }

    public OrderItem(int productId, int quantity, decimal value)
    {
        if (productId <= 0)
            throw new DomainException("Invalid ProductId.");
        if (quantity <= 0)
            throw new DomainException("The quantity must be greater than zero.");
        if (value <= 0)
            throw new DomainException("The value must be greater than zero.");

        ProductId = productId;
        Quantity = quantity;
        Value = value;
    }

    public decimal TotalValue => Value * Quantity;
}