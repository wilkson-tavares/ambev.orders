using Bogus;
using FluentAssertions;
using Orders.Domain.Entities;
using Orders.Domain.Strategies;

namespace Orders.Tests.Unit.Domain;

public class TaxCalculatorTests
{
    private readonly Faker _faker = new("pt_BR");

    private Order CreateOrderWithValue(decimal itemValue)
    {
        var item = new OrderItem(
            productId: _faker.Random.Int(1, 9999),
            quantity: 1,
            value: itemValue);

        return new Order(
            orderId: _faker.Random.Int(1, 9999),
            clientId: _faker.Random.Int(1, 9999),
            items: [item]);
    }

    [Fact]
    public void TaxAtual_ShouldCalculate30Percent()
    {
        var order = CreateOrderWithValue(100m);
        var calculator = new TaxAtualStrategy();

        var result = calculator.Calculate(order);

        result.Should().Be(30m);
    }

    [Fact]
    public void TaxReforma_ShouldCalculate20Percent()
    {
        var order = CreateOrderWithValue(100m);
        var calculator = new TaxReformaStrategy();

        var result = calculator.Calculate(order);

        result.Should().Be(20m);
    }

    [Fact]
    public void TaxAtual_ShouldRoundToTwoDecimalPlaces()
    {
        var order = CreateOrderWithValue(52.70m);
        var calculator = new TaxAtualStrategy();

        var result = calculator.Calculate(order);

        result.Should().Be(15.81m);
    }

    [Fact]
    public void TaxReforma_ShouldRoundToTwoDecimalPlaces()
    {
        var order = CreateOrderWithValue(52.70m);
        var calculator = new TaxReformaStrategy();

        var result = calculator.Calculate(order);

        result.Should().Be(10.54m);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(200)]
    [InlineData(999.99)]
    public void TaxAtual_ShouldCalculateCorrectlyForVariousValues(decimal value)
    {
        var order = CreateOrderWithValue(value);
        var calculator = new TaxAtualStrategy();

        var result = calculator.Calculate(order);

        result.Should().Be(Math.Round(value * 0.3m, 2));
    }
}