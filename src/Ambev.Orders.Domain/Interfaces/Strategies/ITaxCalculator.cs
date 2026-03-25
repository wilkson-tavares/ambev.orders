using Orders.Domain.Entities;

namespace Orders.Domain.Interfaces.Strategies;

public interface ITaxCalculator
{
    decimal Calculate(Order order);
}