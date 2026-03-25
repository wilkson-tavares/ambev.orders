using Orders.Domain.Entities;
using Orders.Domain.Interfaces.Strategies;

namespace Orders.Domain.Strategies;

public sealed class TaxAtualStrategy : ITaxCalculator
{
    private const decimal Aliquot = 0.3m;

    public decimal Calculate(Order order)
        => Math.Round(order.TotalItemsValue * Aliquot, 2);
}