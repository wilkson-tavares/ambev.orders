using Bogus;
using FluentAssertions;
using NSubstitute;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Exceptions;
using Orders.Domain.Interfaces.Repositories;
using Orders.Domain.Interfaces.Strategies;
using Orders.Domain.Services;

namespace Orders.Tests.Unit.Domain;

public class OrderServiceTests
{
    private readonly IOrderRepository _repository;
    private readonly ITaxCalculator _calculator;
    private readonly OrderService _service;
    private readonly Faker _faker = new("pt_BR");

    public OrderServiceTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _calculator = Substitute.For<ITaxCalculator>();
        _service = new OrderService(_repository, _calculator);
    }

    private (int orderId, int clientId, IEnumerable<OrderItem> items) GenerateValidData()
    {
        var orderId = _faker.Random.Int(1, 9999);
        var clientId = _faker.Random.Int(1, 9999);
        var items = new[]
        {
            new OrderItem(
                productId: _faker.Random.Int(1, 9999),
                quantity: _faker.Random.Int(1, 10),
                value: _faker.Finance.Amount(1, 500))
        };

        return (orderId, clientId, items);
    }

    [Fact]
    public async Task CreateAsync_ValidOrder_ShouldPersistAndReturnOrder()
    {
        var (orderId, clientId, items) = GenerateValidData();

        _repository.ExistsAsync(orderId).Returns(false);
        _calculator.Calculate(Arg.Any<Order>()).Returns(30m);

        var result = await _service.CreateAsync(orderId, clientId, items);

        result.OrderId.Should().Be(orderId);
        result.ClientId.Should().Be(clientId);
        result.Status.Should().Be(OrderStatus.Created);
        result.Tax.Should().Be(30m);

        await _repository.Received(1).AddAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task CreateAsync_DuplicateOrder_ShouldThrowDomainException()
    {
        var (orderId, clientId, items) = GenerateValidData();

        _repository.ExistsAsync(orderId).Returns(true);

        var action = () => _service.CreateAsync(orderId, clientId, items);

        await action.Should()
            .ThrowAsync<DomainException>()
            .WithMessage($"*{orderId}*");

        await _repository.DidNotReceive().AddAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task CreateAsync_ValidOrder_ShouldApplyCalculatedTax()
    {
        var (orderId, clientId, items) = GenerateValidData();
        var expectedTax = _faker.Finance.Amount(1, 100);

        _repository.ExistsAsync(orderId).Returns(false);
        _calculator.Calculate(Arg.Any<Order>()).Returns(expectedTax);

        var result = await _service.CreateAsync(orderId, clientId, items);

        result.Tax.Should().Be(expectedTax);
        _calculator.Received(1).Calculate(Arg.Any<Order>());
    }

    [Fact]
    public async Task GetByIdAsync_ExistingOrder_ShouldReturnOrder()
    {
        var (orderId, clientId, items) = GenerateValidData();
        var order = new Order(orderId, clientId, items);

        _repository.GetByIdAsync(1).Returns(order);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentOrder_ShouldThrowDomainException()
    {
        _repository.GetByIdAsync(Arg.Any<int>()).Returns((Order?)null);

        var action = () => _service.GetByIdAsync(99);

        await action.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ListByStatusAsync_ShouldReturnOrdersOfStatus()
    {
        var (orderId, clientId, items) = GenerateValidData();
        var orders = new List<Order> { new(orderId, clientId, items) };

        _repository.ListByStatusAsync(OrderStatus.Created)
            .Returns(orders.AsReadOnly());

        var result = await _service.ListByStatusAsync(OrderStatus.Created);

        result.Should().HaveCount(1);
        result.First().Status.Should().Be(OrderStatus.Created);
    }
}