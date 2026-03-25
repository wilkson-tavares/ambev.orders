using Orders.Domain.Entities;

namespace Orders.Domain.Interfaces;

public interface ITaxCalculator
{
    decimal Calculate(Order order);
}