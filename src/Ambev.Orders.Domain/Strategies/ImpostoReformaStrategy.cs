using Orders.Domain.Entities;
using Orders.Domain.Interfaces;

namespace Orders.Domain.Strategies;

public sealed class TaxReformaStrategy : ITaxCalculator
{
    private const decimal Aliquot = 0.2m;

    public decimal Calculate(Order order)
        => Math.Round(order.TotalItemsValue * Aliquot, 2);
}