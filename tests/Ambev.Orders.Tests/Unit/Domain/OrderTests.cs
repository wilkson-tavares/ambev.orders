using Bogus;
using FluentAssertions;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Exceptions;

namespace Orders.Tests.Unit.Domain;

public class OrderTests
{
    private readonly Faker _faker = new("pt_BR");

    private OrderItem InvalidItem() => new(
        productId: _faker.Random.Int(1, 9999),
        quantity: _faker.Random.Int(1, 10),
        value: _faker.Finance.Amount(1, 500));

    [Fact]
    public void Constructor_ValidData_MustCreateOrderWithStatusCreated()
    {
        var order = new Order(1, 1, [InvalidItem()]);

        order.Status.Should().Be(OrderStatus.Created);
        order.Items.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidOrderId_MustThrowDomainException(int orderId)
    {
        var action = () => new Order(orderId, 1, [InvalidItem()]);

        action.Should().Throw<DomainException>()
            .WithMessage("*OrderId*");
    }

    [Fact]
    public void Constructor_NoItems_MustThrowDomainException()
    {
        var action = () => new Order(1, 1, []);

        action.Should().Throw<DomainException>()
            .WithMessage("*item*");
    }

    [Fact]
    public void StartProcessing_StatusCreated_MustChangeStatus()
    {
        var order = new Order(1, 1, [InvalidItem()]);

        order.StartProcessing();

        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void Send_StatusProcessing_MustChangeStatus()
    {
        var order = new Order(1, 1, [InvalidItem()]);
        order.StartProcessing();

        order.Send();

        order.Status.Should().Be(OrderStatus.Sent);
    }

    [Fact]
    public void Send_StatusCreated_MustThrowDomainException()
    {
        var order = new Order(1, 1, [InvalidItem()]);

        var action = () => order.Send();

        action.Should().Throw<DomainException>()
            .WithMessage("*Created*");
    }
}